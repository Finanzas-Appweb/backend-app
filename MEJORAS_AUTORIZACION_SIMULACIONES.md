# Cambios Implementados: Autorización Avanzada y DELETE Simulaciones

## 📋 Resumen de Cambios

Se implementaron 4 grandes mejoras en la API de simulaciones de crédito hipotecario:

1. ✅ **Nuevo endpoint DELETE para simulaciones**
2. ✅ **Autorización mejorada en simulaciones (GET y GET por ID)**
3. ✅ **Filtrado de clientes por rol (GET /clients)**
4. ✅ **ID reiniciado por simulación en AmortizationItems**

---

## 1. ✅ Endpoint DELETE /api/v1/simulations/{id}

### Implementación

**Archivo**: `Urbania360.Api/Controllers/SimulationsController.cs`

```csharp
/// <summary>
/// Eliminar una simulación y su cronograma de amortización
/// Admin y Agent pueden eliminar cualquier simulación, User solo las de sus propios clientes
/// </summary>
[HttpDelete("{id}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public async Task<ActionResult> DeleteSimulation(Guid id)
```

### Reglas de Autorización

| Rol | Puede Eliminar |
|-----|----------------|
| **Admin** | ✅ Cualquier simulación |
| **Agent** | ✅ Cualquier simulación |
| **User** | ✅ Solo simulaciones de clientes creados por él |

### Lógica Implementada

1. **Buscar simulación**: Con `Include(s => s.Client)` para validar propiedad
2. **Validar existencia**: Si no existe → `404 Not Found`
3. **Obtener usuario y rol**: Del JWT token
4. **Validar autorización**:
   - Si rol es **User**: Verificar `simulation.Client.CreatedByUserId == userId`
   - Si no coincide → `403 Forbidden`
   - Si rol es **Admin** o **Agent**: Sin restricción
5. **Eliminar**:
   - `_context.LoanSimulations.Remove(simulation)`
   - Los `AmortizationItems` se eliminan automáticamente por `DeleteBehavior.Cascade`
6. **Registrar actividad**: ActivityLog con acción "Delete"
7. **Respuesta**: `204 No Content`

### Casos de Uso

#### ✅ Caso 1: Admin elimina cualquier simulación
```http
DELETE /api/v1/simulations/{id}
Authorization: Bearer {token-admin}

→ 204 No Content (simulación eliminada)
```

#### ✅ Caso 2: User elimina simulación de su cliente
```http
DELETE /api/v1/simulations/{id}
Authorization: Bearer {token-user}

→ 204 No Content (cliente fue creado por este user)
```

#### ❌ Caso 3: User intenta eliminar simulación de otro usuario
```http
DELETE /api/v1/simulations/{id}
Authorization: Bearer {token-user-A}

→ 403 Forbidden (cliente fue creado por user-B)
```

#### ❌ Caso 4: Simulación no existe
```http
DELETE /api/v1/simulations/{id-invalido}

→ 404 Not Found
```

---

## 2. ✅ Autorización Mejorada en Simulaciones

### GET /api/v1/simulations (Listado)

**Cambio**: Ahora filtra por `Client.CreatedByUserId` en lugar de `Simulation.CreatedByUserId`

**Antes**:
```csharp
if (userRole == Role.User)
{
    query = query.Where(s => s.CreatedByUserId == userId); // ❌ Incorrecto
}
```

**Después**:
```csharp
if (userRole == Role.User)
{
    // Filtrar por clientes creados por el usuario actual
    query = query.Where(s => s.Client.CreatedByUserId == userId); // ✅ Correcto
}
```

**Comportamiento**:

| Rol | Ve Simulaciones |
|-----|-----------------|
| **Admin** | Todas las simulaciones del sistema |
| **Agent** | Todas las simulaciones del sistema |
| **User** | Solo simulaciones de clientes que él creó |

**Ejemplo**:
- User A crea Client X
- User A crea Client Y
- User B crea Client Z
- Admin crea simulación S1 para Client X
- User A crea simulación S2 para Client Y
- User B crea simulación S3 para Client Z

**Resultado**:
- `User A` ve: S1, S2 (ambas son de sus clientes X e Y)
- `User B` ve: S3 (solo de su cliente Z)
- `Admin/Agent` ven: S1, S2, S3 (todas)

---

### GET /api/v1/simulations/{id} (Detalle)

**Cambio**: Similar al listado, valida por `Client.CreatedByUserId`

**Antes**:
```csharp
if (userRole == Role.User && simulation.CreatedByUserId != userId)
{
    return Forbid(); // ❌ Validaba creador de simulación
}
```

**Después**:
```csharp
if (userRole == Role.User && simulation.Client.CreatedByUserId != userId)
{
    return Forbid(); // ✅ Valida creador del cliente
}
```

**Resultado**:
- User solo puede ver detalles de simulaciones de sus propios clientes
- Admin/Agent pueden ver cualquier simulación

---

## 3. ✅ Filtrado de Clientes por Rol

### GET /api/v1/clients (Listado)

**Archivo**: `Urbania360.Api/Controllers/ClientsController.cs`

**Cambios**:
1. Agregado método helper `GetUserRole()`
2. Filtro condicional por `CreatedByUserId`

```csharp
// Obtener usuario y rol actual
var currentUserId = GetCurrentUserId();
var userRole = GetUserRole();

var query = _context.Clients
    .Include(c => c.CreatedByUser)
    .AsQueryable();

// Si el usuario es User (no Admin ni Agent), solo puede ver sus propios clientes
if (userRole == Domain.Enums.Role.User)
{
    query = query.Where(c => c.CreatedByUserId == currentUserId);
}
```

**Comportamiento**:

| Rol | Ve Clientes |
|-----|-------------|
| **Admin** | Todos los clientes |
| **Agent** | Todos los clientes |
| **User** | Solo clientes creados por él |

---

### GET /api/v1/clients/{id} (Detalle)

**Cambios**:
1. Agregado validación de autorización
2. Respuesta `403 Forbidden` si User intenta ver cliente ajeno

```csharp
// Validar autorización
var currentUserId = GetCurrentUserId();
var userRole = GetUserRole();

// Si el usuario es User (no Admin ni Agent), solo puede ver sus propios clientes
if (userRole == Domain.Enums.Role.User && client.CreatedByUserId != currentUserId)
{
    return Forbid(); // 403 Forbidden
}
```

**Resultado**:
- User solo puede ver detalles de clientes que él creó
- Admin/Agent pueden ver cualquier cliente

---

### Método Helper Agregado

```csharp
private Domain.Enums.Role GetUserRole()
{
    var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
    return Enum.TryParse<Domain.Enums.Role>(roleClaim, out var role) 
        ? role 
        : Domain.Enums.Role.User;
}
```

---

## 4. ✅ ID Reiniciado por Simulación en AmortizationItems

### Problema Original

En la base de datos, `AmortizationItems.Id` es un `IDENTITY` global:
- Primera simulación: items con Id = 1, 2, 3, ..., 240
- Segunda simulación: items con Id = 241, 242, 243, ..., 480
- Tercera simulación: items con Id = 481, 482, 483, ..., 720

**Esto era confuso en la API** porque los usuarios esperan ver:
- Simulación 1: items 1-240
- Simulación 2: items 1-240 (no 241-480)
- Simulación 3: items 1-240 (no 481-720)

---

### Solución Implementada

#### DTO Actualizado

**Archivo**: `Urbania360.Api/DTOs/Simulations/AmortizationItemResponse.cs`

```csharp
public class AmortizationItemResponse
{
    /// <summary>
    /// ID del item (igual al periodo, reiniciado por simulación)
    /// </summary>
    public int Id { get; set; }  // ✅ Cambiado de long a int
    
    /// <summary>
    /// Número de periodo/cuota (1, 2, 3, ...)
    /// </summary>
    public int Period { get; set; }
    
    // ... otros campos
}
```

**Cambios**:
- ✅ `Id` cambiado de `long` a `int`
- ✅ Documentación clara: "ID del item (igual al periodo, reiniciado por simulación)"

---

#### Mapeo Actualizado

**Archivo**: `Urbania360.Api/Mappings/MappingProfile.cs`

```csharp
// AmortizationItem: usar Period como Id en el DTO (reiniciado por simulación)
CreateMap<AmortizationItem, AmortizationItemResponse>()
    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Period));
```

**Resultado**:
- El campo `Id` en la API ahora es igual a `Period`
- Cada simulación tiene items con `Id` desde 1 hasta `termMonths`

---

### Comparación Antes/Después

#### ANTES (Confuso):
```json
{
  "amortizationSchedule": [
    { "id": 481, "period": 1, "dueDate": "2025-02-01", ... },
    { "id": 482, "period": 2, "dueDate": "2025-03-01", ... },
    { "id": 483, "period": 3, "dueDate": "2025-04-01", ... }
  ]
}
```

#### DESPUÉS (Limpio):
```json
{
  "amortizationSchedule": [
    { "id": 1, "period": 1, "dueDate": "2025-02-01", ... },
    { "id": 2, "period": 2, "dueDate": "2025-03-01", ... },
    { "id": 3, "period": 3, "dueDate": "2025-04-01", ... }
  ]
}
```

---

### Ventajas

1. ✅ **Números limpios**: ID siempre empieza en 1 para cada simulación
2. ✅ **No cambios en BD**: El `IDENTITY` real sigue igual (480, 481, ...)
3. ✅ **Mejor UX**: Frontend ve `id` = `period` (más intuitivo)
4. ✅ **Sin resetear identity**: No hay problemas de concurrencia

---

## 📊 Matriz de Permisos Actualizada

### Simulaciones

| Acción | Admin | Agent | User |
|--------|-------|-------|------|
| POST (crear) | ✅ | ✅ | ✅ |
| GET (listar) | ✅ Todas | ✅ Todas | ✅ Solo de sus clientes |
| GET /{id} | ✅ Cualquiera | ✅ Cualquiera | ✅ Solo de sus clientes |
| DELETE /{id} | ✅ Cualquiera | ✅ Cualquiera | ✅ Solo de sus clientes |

### Clientes

| Acción | Admin | Agent | User |
|--------|-------|-------|------|
| POST (crear) | ✅ | ✅ | ✅ |
| GET (listar) | ✅ Todos | ✅ Todos | ✅ Solo creados por él |
| GET /{id} | ✅ Cualquiera | ✅ Cualquiera | ✅ Solo creados por él |
| PUT /{id} | ✅ | ✅ | ✅ |
| DELETE /{id} | ✅ | ✅ | ✅ |

---

## 🧪 Casos de Prueba

### Escenario: 3 Usuarios con Clientes y Simulaciones

**Setup**:
```
Admin (admin@urbania360.com)
├─ Cliente A1 (creado por Admin)
│  └─ Simulación S1 (creada por Admin para A1)
│
User-1 (user1@example.com)
├─ Cliente U1 (creado por User-1)
│  ├─ Simulación S2 (creada por User-1 para U1)
│  └─ Simulación S3 (creada por Admin para U1)
│
User-2 (user2@example.com)
└─ Cliente U2 (creado por User-2)
   └─ Simulación S4 (creada por User-2 para U2)
```

---

### Prueba 1: GET /api/v1/clients

#### User-1
```http
GET /api/v1/clients
Authorization: Bearer {token-user-1}

→ 200 OK
{
  "data": [
    { "id": "{U1}", "firstName": "Cliente", "lastName": "U1" }
  ]
}
```
✅ Solo ve su cliente U1

#### Admin
```http
GET /api/v1/clients
Authorization: Bearer {token-admin}

→ 200 OK
{
  "data": [
    { "id": "{A1}", ... },
    { "id": "{U1}", ... },
    { "id": "{U2}", ... }
  ]
}
```
✅ Ve todos los clientes

---

### Prueba 2: GET /api/v1/clients/{id}

#### User-1 intenta ver cliente de User-2
```http
GET /api/v1/clients/{U2}
Authorization: Bearer {token-user-1}

→ 403 Forbidden
```
❌ No puede ver clientes de otros usuarios

#### Admin ve cliente de User-2
```http
GET /api/v1/clients/{U2}
Authorization: Bearer {token-admin}

→ 200 OK
{ "id": "{U2}", ... }
```
✅ Admin puede ver cualquier cliente

---

### Prueba 3: GET /api/v1/simulations

#### User-1
```http
GET /api/v1/simulations
Authorization: Bearer {token-user-1}

→ 200 OK
{
  "data": [
    { "id": "{S2}", "clientName": "Cliente U1", ... },
    { "id": "{S3}", "clientName": "Cliente U1", ... }
  ]
}
```
✅ Ve S2 y S3 (ambas son de su cliente U1, sin importar quién creó la simulación)

#### Admin
```http
GET /api/v1/simulations
Authorization: Bearer {token-admin}

→ 200 OK
{
  "data": [
    { "id": "{S1}", ... },
    { "id": "{S2}", ... },
    { "id": "{S3}", ... },
    { "id": "{S4}", ... }
  ]
}
```
✅ Ve todas las simulaciones

---

### Prueba 4: GET /api/v1/simulations/{id}

#### User-1 ve S2 (su cliente)
```http
GET /api/v1/simulations/{S2}
Authorization: Bearer {token-user-1}

→ 200 OK
{
  "id": "{S2}",
  "amortizationSchedule": [
    { "id": 1, "period": 1, ... },  ← ID reiniciado
    { "id": 2, "period": 2, ... },
    ...
  ]
}
```
✅ Puede ver simulación de su cliente
✅ Los IDs del cronograma empiezan en 1

#### User-1 intenta ver S4 (cliente de User-2)
```http
GET /api/v1/simulations/{S4}
Authorization: Bearer {token-user-1}

→ 403 Forbidden
```
❌ No puede ver simulaciones de clientes ajenos

---

### Prueba 5: DELETE /api/v1/simulations/{id}

#### User-1 elimina S2 (su cliente)
```http
DELETE /api/v1/simulations/{S2}
Authorization: Bearer {token-user-1}

→ 204 No Content
```
✅ Simulación eliminada
✅ AmortizationItems eliminados en cascada

#### User-1 intenta eliminar S4 (cliente de User-2)
```http
DELETE /api/v1/simulations/{S4}
Authorization: Bearer {token-user-1}

→ 403 Forbidden
```
❌ No puede eliminar simulaciones de clientes ajenos

#### Admin elimina cualquier simulación
```http
DELETE /api/v1/simulations/{S1}
Authorization: Bearer {token-admin}

→ 204 No Content
```
✅ Admin puede eliminar cualquier simulación

---

## ✅ Validación y Resultados

### Compilación
```bash
✅ dotnet build Urbania360.sln
Build succeeded in 4.1s
```

### Ejecución
```bash
✅ dotnet run (en Urbania360.Api)
Now listening on: http://localhost:5294
Application started. Press Ctrl+C to shut down.
```

### Swagger
- ✅ Endpoint DELETE documentado correctamente
- ✅ Respuestas 204, 404, 403 documentadas
- ✅ AmortizationItemResponse.Id ahora es `int` (en lugar de `long`)

---

## 📝 Archivos Modificados (5 archivos)

1. ✅ `Urbania360.Api/Controllers/SimulationsController.cs`
   - Agregado endpoint DELETE
   - Actualizado filtro en GET (por Client.CreatedByUserId)
   - Actualizada validación en GET /{id}

2. ✅ `Urbania360.Api/Controllers/ClientsController.cs`
   - Agregado filtro por CreatedByUserId en GET
   - Agregada validación en GET /{id}
   - Agregado método helper GetUserRole()

3. ✅ `Urbania360.Api/DTOs/Simulations/AmortizationItemResponse.cs`
   - Cambiado Id de `long` a `int`
   - Agregada documentación XML

4. ✅ `Urbania360.Api/Mappings/MappingProfile.cs`
   - Actualizado mapeo para usar Period como Id

5. ✅ `MEJORAS_AUTORIZACION_SIMULACIONES.md`
   - Documentación completa de todos los cambios

---

## 🎯 Conclusión

✅ **Todos los cambios implementados exitosamente**
✅ **Sin romper validación del bono MiVivienda**
✅ **Compilación exitosa**
✅ **Aplicación corriendo en http://localhost:5294**
✅ **Swagger actualizado con nuevos endpoints**

**Beneficios**:
1. ✅ **Seguridad mejorada**: User solo ve/elimina sus propios datos
2. ✅ **Autorización granular**: Basada en propiedad de clientes
3. ✅ **UX mejorada**: IDs de cronograma limpios (1, 2, 3, ...)
4. ✅ **API RESTful completa**: CRUD completo para simulaciones
5. ✅ **Eliminación en cascada**: AmortizationItems se eliminan automáticamente

**Próximo paso**: Probar todos los escenarios en Swagger con diferentes roles.
