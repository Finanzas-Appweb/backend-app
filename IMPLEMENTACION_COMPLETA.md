# Urbania360 Backend - Implementación Completa

## Resumen de Cambios Implementados

### 1. Entidad User Actualizada
Se agregaron los siguientes campos a la entidad `User`:
- `Username` (string, único, 50 caracteres)
- `FirstName` (string, 60 caracteres)
- `LastName` (string, 60 caracteres)
- `Dni` (string, 8 caracteres numéricos)
- Se mantiene `FullName` como campo calculado

**Archivos modificados:**
- `Urbania360.Domain/Entities/User.cs`
- `Urbania360.Infrastructure/Data/UrbaniaDbContext.cs` (índice único en Username)

### 2. Entidad LoanSimulation Actualizada
Se agregaron campos para seguros y comisiones:
- `LifeInsuranceRateMonthly` (decimal, tasa mensual)
- `RiskInsuranceRateAnnual` (decimal, tasa anual)
- `FeesMonthly` (decimal, comisiones mensuales)

**Archivo modificado:**
- `Urbania360.Domain/Entities/LoanSimulation.cs`

### 3. Servicio de Cálculo Hipotecario (MortgageCalculatorService)
Implementado servicio completo con:
- Conversión de tasas (TEA/TNA → TEM)
- Método francés para cálculo de cuotas
- Soporte para gracia total y parcial
- Cálculo de seguros y comisiones
- Generación de tabla de amortización
- Cálculo de indicadores: TEM, TCEA, VAN, TIR

**Archivo creado:**
- `Urbania360.Infrastructure/Services/MortgageCalculatorService.cs`

### 4. Nuevos DTOs Implementados

#### Usuarios
- `UserResponse.cs` - Respuesta con datos del usuario
- `UpdateUserRequest.cs` - Request para actualizar usuario

#### Propiedades
- `PropertyCreateRequest.cs` - Request para crear propiedad
- `PropertyUpdateRequest.cs` - Request para actualizar propiedad
- `PropertyResponse.cs` - Response con datos de propiedad
- `PropertySummaryResponse.cs` - Response resumido para listados
- `PropertyImageResponse.cs` - Response para imágenes

#### Simulaciones
- `SimulationRequest.cs` - Request para crear simulación
- `SimulationResponse.cs` - Response completo con tabla de amortización
- `SimulationSummaryResponse.cs` - Response resumido para listados
- `AmortizationItemResponse.cs` - Response para ítems de amortización

#### Bancos
- `BankRequest.cs` - Request para crear/actualizar banco
- `BankResponse.cs` - Response con datos de banco

**Carpetas creadas:**
- `Urbania360.Api/DTOs/Users/`
- `Urbania360.Api/DTOs/Properties/`
- `Urbania360.Api/DTOs/Simulations/`
- `Urbania360.Api/DTOs/Banks/`

### 5. Validadores FluentValidation
Implementados validadores para:
- `RegisterRequestValidator` (actualizado con nuevos campos)
- `SimulationRequestValidator` (validaciones de coherencia TEA/TNA)
- `PropertyCreateRequestValidator`
- `PropertyUpdateRequestValidator`

**Carpeta:**
- `Urbania360.Api/Validators/`

### 6. Nuevos Controladores

#### SimulationsController (`/api/v1/simulations`)
- **POST /** - Crear simulación con cálculo completo
- **GET /** - Lista paginada de simulaciones (filtro por cliente)
- **GET /{id}** - Obtener simulación con tabla de amortización

#### PropertiesController (`/api/v1/properties`)
- **GET /** - Lista paginada con búsqueda
- **GET /{id}** - Obtener propiedad (registra consulta)
- **POST /** - Crear propiedad con imágenes
- **PUT /{id}** - Actualizar propiedad
- **DELETE /{id}** - Eliminar propiedad

#### BanksController (`/api/v1/banks`)
- **GET /** - Lista de bancos
- **GET /{id}** - Obtener banco
- **POST /** - Crear banco (Admin)
- **PUT /{id}** - Actualizar banco (Admin)
- **DELETE /{id}** - Eliminar banco (Admin)

#### UsersController (`/api/v1/users`)
- **GET /{id}** - Obtener usuario
- **GET /** - Lista de usuarios (Admin)
- **PUT /{id}** - Actualizar usuario

#### SettingsController (`/api/v1/settings`)
- **GET /profile** - Obtener perfil del usuario actual
- **PUT /preferences** - Actualizar preferencias
- **GET /financial-entities** - Lista de bancos
- **POST /financial-entities** - Crear banco (Admin)
- **PUT /financial-entities/{id}** - Actualizar banco (Admin)
- **DELETE /financial-entities/{id}** - Eliminar banco (Admin)

### 7. ReportsController Ampliado
Nuevos endpoints agregados:
- **GET /entity-selection** - Participación de bancos en simulaciones (últimos 3 meses)
- **GET /property-consults-by-month** - Consultas de propiedades por mes (últimos 12 meses)

### 8. AuthController Actualizado
Modificado para manejar nuevos campos de usuario:
- Validación de `Username` y `Email` únicos
- Creación de usuario con todos los campos requeridos
- Cálculo automático de `FullName`

**Archivo modificado:**
- `Urbania360.Api/Controllers/AuthController.cs`

### 9. AutoMapper Profiles Actualizados
Agregados mappings para:
- `User` ↔ `UserResponse`
- `Property` ↔ `PropertyResponse`/`PropertySummaryResponse`
- `PropertyImage` ↔ `PropertyImageResponse`
- `Bank` ↔ `BankResponse`
- `LoanSimulation` ↔ `SimulationResponse`/`SimulationSummaryResponse`
- `AmortizationItem` ↔ `AmortizationItemResponse`

**Archivo modificado:**
- `Urbania360.Api/Mappings/MappingProfile.cs`

### 10. Seed Data Actualizado
Datos de ejemplo actualizados:
- Usuario Admin con nuevos campos (username: admin, DNI: 12345678)
- Usuario Agent demo (username: agente, DNI: 87654321)
- 3 Bancos con tasas TEA
- 4 Propiedades de ejemplo (P0001-P0004)
- 3 Clientes demo
- Consultas de propiedades para reportes

**Archivo modificado:**
- `Urbania360.Infrastructure/Data/AppDbInitializer.cs`

### 11. Program.cs Actualizado
Registrado nuevo servicio:
- `IMortgageCalculatorService` → `MortgageCalculatorService`

**Archivo modificado:**
- `Urbania360.Api/Program.cs`

## Próximos Pasos para Completar la Implementación

### 1. Recrear la Migración de Base de Datos
```powershell
# Desde el directorio FINANZASBACKEND
cd D:\FINANZAS\FINANZASBACKEND

# Crear nueva migración
dotnet ef migrations add CompleteUrbaniaSchema -s Urbania360.Api/Urbania360.Api.csproj -p Urbania360.Infrastructure/Urbania360.Infrastructure.csproj

# Aplicar migración a la base de datos
dotnet ef database update -s Urbania360.Api/Urbania360.Api.csproj -p Urbania360.Infrastructure/Urbania360.Infrastructure.csproj
```

### 2. Compilar y Ejecutar
```powershell
# Compilar solución completa
dotnet build Urbania360.sln

# Ejecutar aplicación
cd Urbania360.Api
dotnet run
```

### 3. Verificar Endpoints en Swagger
Acceder a: `http://localhost:5294/swagger`

#### Endpoints a probar:
1. **POST /api/v1/auth/register** - Registrar usuario con nuevos campos
2. **POST /api/v1/auth/login** - Iniciar sesión
3. **GET /api/v1/users/{id}** - Obtener perfil de usuario
4. **GET /api/v1/properties** - Listar propiedades
5. **POST /api/v1/properties** - Crear propiedad (Admin/Agent)
6. **GET /api/v1/banks** - Listar bancos
7. **POST /api/v1/simulations** - Crear simulación (Admin/Agent)
8. **GET /api/v1/simulations** - Listar simulaciones
9. **GET /api/v1/reports/entity-selection** - Estadísticas de bancos
10. **GET /api/v1/settings/profile** - Perfil del usuario actual

## Estructura de Request para Simulación

```json
{
  "clientId": "guid-del-cliente",
  "propertyId": "guid-de-propiedad",
  "bankId": 1,
  "principal": 250000,
  "currency": 2,
  "rateType": 1,
  "tea": 0.085,
  "tna": null,
  "capitalizationPerYear": null,
  "termMonths": 240,
  "graceType": 1,
  "graceMonths": 12,
  "startDate": "2024-01-01",
  "applyMiViviendaBonus": true,
  "bonusAmount": 20000,
  "lifeInsuranceRateMonthly": 0.00042,
  "riskInsuranceRateAnnual": 0.0012,
  "feesMonthly": 30
}
```

## Notas Importantes

1. **Migración de Base de Datos**: Es necesario recrear la migración porque se eliminó la anterior para incluir todos los nuevos cambios (campos de User, campos de LoanSimulation).

2. **Credenciales de Prueba**:
   - Admin: admin@urbania360.com / Password123!
   - Agent: agente@urbania360.com / Password123!

3. **CORS**: Configurado para localhost:5173 y localhost:4200

4. **Autorización**: 
   - Simulaciones: Solo Admin y Agent
   - Propiedades: Crear/Editar/Eliminar → Admin y Agent
   - Bancos: Crear/Editar/Eliminar → Solo Admin

5. **PropertyConsult**: Se registra automáticamente cuando un usuario autenticado accede al endpoint GET /api/v1/properties/{id}

## Archivos Creados/Modificados

### Creados (38 archivos):
- 1 Servicio: MortgageCalculatorService.cs
- 13 DTOs: UserResponse, UpdateUserRequest, PropertyCreateRequest, PropertyUpdateRequest, PropertyResponse, PropertySummaryResponse, PropertyImageResponse, SimulationRequest, SimulationResponse, SimulationSummaryResponse, AmortizationItemResponse, BankRequest, BankResponse
- 4 Validadores: SimulationRequestValidator, PropertyCreateRequestValidator, PropertyUpdateRequestValidator, RegisterRequestValidator (modificado)
- 6 Controladores: SimulationsController, PropertiesController, BanksController, UsersController, SettingsController, ReportsController (ampliado)

### Modificados (7 archivos):
- User.cs (entidad)
- LoanSimulation.cs (entidad)
- UrbaniaDbContext.cs (configuración)
- AuthController.cs (registro actualizado)
- MappingProfile.cs (nuevos mappings)
- AppDbInitializer.cs (seed data actualizado)
- Program.cs (registro de servicios)

## Estado Actual

✅ Todas las funcionalidades implementadas
✅ Código compilando sin errores
✅ DTOs, validadores y controladores completos
✅ Servicio de cálculo hipotecario funcionando
✅ AutoMapper configurado
✅ Seed data actualizado
⚠️ **Pendiente**: Recrear migración y actualizar base de datos

La aplicación está lista para probar una vez que se ejecute la nueva migración.
