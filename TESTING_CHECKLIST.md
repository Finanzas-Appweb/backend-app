# Urbania360 Backend - Checklist de Pruebas

## Pre-requisitos
- [ ] Base de datos SQL Server disponible (urbania360.database.windows.net)
- [ ] Connection string configurado en `appsettings.json`
- [ ] Migraciones aplicadas correctamente
- [ ] Aplicación corriendo en http://localhost:5294

## Credenciales de Prueba
```
Admin:
- Email: admin@urbania360.com
- Password: Password123!
- Username: admin
- DNI: 12345678

Agent:
- Email: agente@urbania360.com
- Password: Password123!
- Username: agente
- DNI: 87654321
```

## 1. Autenticación y Usuarios

### Registro de Usuario
- [ ] **POST /api/v1/auth/register**
  - Cuerpo:
    ```json
    {
      "username": "testuser",
      "firstName": "Test",
      "lastName": "User",
      "dni": "99999999",
      "email": "test@example.com",
      "phone": "+51999999999",
      "password": "Password123!",
      "role": 3
    }
    ```
  - Verificar: Token JWT devuelto, usuario en BD

### Login
- [ ] **POST /api/v1/auth/login**
  - Cuerpo:
    ```json
    {
      "email": "admin@urbania360.com",
      "password": "Password123!"
    }
    ```
  - Guardar: Token JWT para siguientes requests

### Perfil de Usuario
- [ ] **GET /api/v1/settings/profile**
  - Headers: `Authorization: Bearer {token}`
  - Verificar: Datos completos del usuario incluyendo DNI, Username

### Actualizar Usuario
- [ ] **PUT /api/v1/users/{userId}**
  - Cuerpo:
    ```json
    {
      "username": "admin_updated",
      "firstName": "Admin",
      "lastName": "Updated",
      "dni": "12345678",
      "email": "admin@urbania360.com",
      "phone": "+51999999999",
      "defaultCurrency": 1,
      "defaultRateType": 1
    }
    ```

## 2. Bancos

### Listar Bancos
- [ ] **GET /api/v1/banks**
  - Verificar: 3 bancos seed (BCP, Interbank, Scotiabank)

### Crear Banco (Admin only)
- [ ] **POST /api/v1/banks**
  - Cuerpo:
    ```json
    {
      "name": "BBVA",
      "annualRateTea": 0.0875,
      "effectiveFrom": "2024-01-01T00:00:00Z"
    }
    ```

### Actualizar Banco (Admin only)
- [ ] **PUT /api/v1/banks/{bankId}**
  - Modificar tasa anual

### Eliminar Banco (Admin only)
- [ ] **DELETE /api/v1/banks/{bankId}**
  - Solo si no tiene simulaciones asociadas

## 3. Propiedades

### Listar Propiedades
- [ ] **GET /api/v1/properties**
  - Parámetros: `?page=1&pageSize=10&search=san`
  - Verificar: Paginación y búsqueda funcionando

### Ver Detalle de Propiedad
- [ ] **GET /api/v1/properties/{propertyId}**
  - Verificar: Se registra PropertyConsult automáticamente

### Crear Propiedad (Admin/Agent)
- [ ] **POST /api/v1/properties**
  - Cuerpo:
    ```json
    {
      "code": "P0005",
      "title": "Casa en Surco",
      "address": "Av. Principal 123",
      "district": "Surco",
      "province": "Lima",
      "type": 1,
      "areaM2": 150.5,
      "price": 320000,
      "currency": 2,
      "imagesUrl": [
        "https://example.com/img1.jpg",
        "https://example.com/img2.jpg"
      ]
    }
    ```

### Actualizar Propiedad (Admin/Agent)
- [ ] **PUT /api/v1/properties/{propertyId}**
  - Modificar precio y agregar/quitar imágenes

### Eliminar Propiedad (Admin/Agent)
- [ ] **DELETE /api/v1/properties/{propertyId}**
  - Solo si no tiene simulaciones

## 4. Clientes

### Listar Clientes
- [ ] **GET /api/v1/clients**
  - Verificar: 3 clientes seed

### Crear Cliente
- [ ] **POST /api/v1/clients**
  - Cuerpo:
    ```json
    {
      "firstName": "Pedro",
      "lastName": "Gomez Torres",
      "email": "pedro.gomez@email.com",
      "phone": "+51987654321",
      "annualIncome": 55000
    }
    ```

## 5. Simulaciones de Préstamos

### Crear Simulación con TEA (Admin/Agent)
- [ ] **POST /api/v1/simulations**
  - Cuerpo:
    ```json
    {
      "clientId": "{clientId-from-db}",
      "propertyId": "{propertyId-from-db}",
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
  - Verificar:
    - Tabla de amortización generada (240 períodos)
    - TEM calculado correctamente
    - Cuota mensual razonable
    - TCEA, VAN, TIR calculados
    - Total de intereses y costo total

### Crear Simulación con TNA (Admin/Agent)
- [ ] **POST /api/v1/simulations**
  - Cambiar: `"rateType": 2`, `"tea": null`, `"tna": 0.082`, `"capitalizationPerYear": 12`

### Crear Simulación sin Gracia
- [ ] **POST /api/v1/simulations**
  - Cambiar: `"graceType": 0`, `"graceMonths": 0`

### Crear Simulación con Gracia Total
- [ ] **POST /api/v1/simulations**
  - Cambiar: `"graceType": 2`, `"graceMonths": 12`
  - Verificar: Capitalización de intereses en período de gracia

### Listar Simulaciones
- [ ] **GET /api/v1/simulations**
  - Parámetros: `?clientId={clientId}&page=1&pageSize=10`

### Ver Detalle de Simulación
- [ ] **GET /api/v1/simulations/{simulationId}**
  - Verificar: Tabla completa de amortización incluida

## 6. Reportes

### Resumen del Sistema
- [ ] **GET /api/v1/reports/summary**
  - Verificar: Estadísticas generales y últimas 5 actividades

### Propiedades Más Consultadas
- [ ] **GET /api/v1/reports/most-consulted-properties**
  - Parámetros: `?limit=10`
  - Verificar: Propiedades ordenadas por consultas

### Simulaciones por Mes
- [ ] **GET /api/v1/reports/simulations-by-month**
  - Parámetros: `?months=6`

### Selección de Entidades Financieras
- [ ] **GET /api/v1/reports/entity-selection**
  - Verificar: Participación de bancos últimos 3 meses

### Consultas de Propiedades por Mes
- [ ] **GET /api/v1/reports/property-consults-by-month**
  - Verificar: Últimos 12 meses

## 7. Settings

### Actualizar Preferencias
- [ ] **PUT /api/v1/settings/preferences**
  - Cuerpo:
    ```json
    {
      "defaultCurrency": 2,
      "defaultRateType": 2
    }
    ```

### Gestión de Entidades Financieras
- [ ] **GET /api/v1/settings/financial-entities**
- [ ] **POST /api/v1/settings/financial-entities** (Admin)
- [ ] **PUT /api/v1/settings/financial-entities/{id}** (Admin)
- [ ] **DELETE /api/v1/settings/financial-entities/{id}** (Admin)

## 8. Validaciones y Casos de Error

### Validación de Simulaciones
- [ ] Intentar crear simulación con TEA y TNA al mismo tiempo → Error
- [ ] Intentar crear simulación con TNA sin capitalizationPerYear → Error
- [ ] Intentar crear simulación con graceMonths >= termMonths → Error
- [ ] Intentar crear simulación con bonusAmount > principal → Error

### Validación de Usuarios
- [ ] Intentar registrar con DNI no numérico → Error
- [ ] Intentar registrar con DNI != 8 caracteres → Error
- [ ] Intentar registrar con email ya existente → 409 Conflict
- [ ] Intentar registrar con username ya existente → 409 Conflict

### Validación de Propiedades
- [ ] Intentar crear propiedad con código duplicado → 409 Conflict
- [ ] Intentar eliminar propiedad con simulaciones → 409 Conflict

### Validación de Bancos
- [ ] Intentar crear banco con nombre duplicado → 409 Conflict
- [ ] Intentar eliminar banco con simulaciones → 409 Conflict

### Autorización
- [ ] Intentar POST /api/v1/simulations sin token → 401 Unauthorized
- [ ] Con rol User: POST /api/v1/simulations debe funcionar → 201 Created
- [ ] Con rol User: GET /api/v1/simulations solo debe mostrar simulaciones propias
- [ ] Con rol Admin/Agent: GET /api/v1/simulations debe mostrar todas las simulaciones
- [ ] Intentar POST /api/v1/banks con rol Agent → 403 Forbidden

## 9. Cálculos Financieros

### Verificar Cálculos Manuales
Para una simulación de ejemplo:
- Principal: $250,000
- TEA: 8.5%
- Plazo: 240 meses (20 años)
- Gracia parcial: 12 meses
- Bono Mi Vivienda: $20,000

Cálculos esperados:
- [ ] TEM = (1 + 0.085)^(1/12) - 1 ≈ 0.00681 (0.681%)
- [ ] Principal efectivo = $250,000 - $20,000 = $230,000
- [ ] Cuota mensual (después de gracia) ≈ $1,970
- [ ] Primera cuota de gracia (solo intereses) ≈ $1,566
- [ ] Saldo final del último período = 0

### Verificar Tabla de Amortización
- [ ] Número de períodos correcto (240)
- [ ] Saldo inicial primer período = Principal efectivo
- [ ] Durante gracia: saldo constante (gracia parcial) o creciente (gracia total)
- [ ] Después de gracia: saldo decreciente
- [ ] Saldo final último período = 0 o muy cercano a 0
- [ ] Suma de Principal en todos los períodos = Principal efectivo
- [ ] Intereses decrecientes a lo largo del tiempo
- [ ] Seguros e intereses calculados correctamente cada período

## 10. Performance y Límites

- [ ] Simulación con 600 meses (máximo) se genera correctamente
- [ ] Lista con 50 elementos por página funciona bien
- [ ] Búsqueda de propiedades responde rápido
- [ ] Reportes con muchos datos no timeout

## Resultado Final

- [ ] **Todas las funcionalidades implementadas**
- [ ] **Todos los endpoints respondiendo correctamente**
- [ ] **Validaciones funcionando**
- [ ] **Autorización por roles correcta**
- [ ] **Cálculos financieros precisos**
- [ ] **Seed data cargado correctamente**
- [ ] **Sin errores de compilación**
- [ ] **Swagger documentado y funcional**

---

## Notas
- Registrar cualquier bug o comportamiento inesperado
- Verificar logs de actividad en la base de datos
- Probar con diferentes roles (Admin, Agent, User)
- Usuario con rol User solo puede ver sus propias simulaciones (filtradas por CreatedByUserId)
- Validar respuestas de error con mensajes claros
