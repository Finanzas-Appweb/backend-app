# Corrección de Validación del Bono MiVivienda

## 🐛 Problema Original

Al enviar una solicitud con:
```json
{
  "applyMiViviendaBonus": false,
  "bonusAmount": 0
}
```

El backend respondía con error `400 Bad Request`:
```json
{
  "BONUSAMOUNT": "El monto del bono debe ser mayor a cero"
}
```

**Causa**: La validación se aplicaba siempre que `bonusAmount` tuviera valor (incluso `0`), sin importar el valor de `applyMiViviendaBonus`.

---

## ✅ Solución Implementada

### 1. Validación Corregida

**Archivo**: `Urbania360.Api/Validators/SimulationRequestValidator.cs`

```csharp
// ANTES - Validaba cuando bonusAmount tenía valor, sin importar applyMiViviendaBonus
RuleFor(x => x.BonusAmount)
    .NotNull().When(x => x.ApplyMiViviendaBonus)
    .WithMessage("El monto del bono es requerido cuando se aplica el bono Mi Vivienda")
    .GreaterThan(0).When(x => x.BonusAmount.HasValue)  // ❌ PROBLEMA AQUÍ
    .WithMessage("El monto del bono debe ser mayor a cero")
    .LessThan(x => x.Principal).When(x => x.BonusAmount.HasValue)  // ❌ PROBLEMA AQUÍ
    .WithMessage("El monto del bono no puede ser mayor al préstamo principal");

// DESPUÉS - Solo valida cuando applyMiViviendaBonus es true
RuleFor(x => x.BonusAmount)
    .NotNull().When(x => x.ApplyMiViviendaBonus)
    .WithMessage("El monto del bono es requerido cuando se aplica el bono Mi Vivienda")
    .GreaterThan(0).When(x => x.ApplyMiViviendaBonus && x.BonusAmount.HasValue)  // ✅ CORREGIDO
    .WithMessage("El monto del bono debe ser mayor a cero")
    .LessThan(x => x.Principal).When(x => x.ApplyMiViviendaBonus && x.BonusAmount.HasValue)  // ✅ CORREGIDO
    .WithMessage("El monto del bono no puede ser mayor al préstamo principal");
```

### 2. Normalización del Bono en el Controlador

**Archivo**: `Urbania360.Api/Controllers/SimulationsController.cs`

```csharp
// Normalizar bonusAmount: si no se aplica el bono MiVivienda, forzar a 0
var bonusAmount = request.ApplyMiViviendaBonus ? request.BonusAmount : 0;

// Usar bonusAmount normalizado en el cálculo
var input = new SimulationInput
{
    // ... otros campos ...
    ApplyMiViviendaBonus = request.ApplyMiViviendaBonus,
    BonusAmount = bonusAmount,  // ✅ Usar bonusAmount normalizado
    // ... otros campos ...
};

// Usar bonusAmount normalizado en la entidad
var simulation = new LoanSimulation
{
    // ... otros campos ...
    ApplyMiViviendaBonus = request.ApplyMiViviendaBonus,
    BonusAmount = bonusAmount,  // ✅ Usar bonusAmount normalizado
    // ... otros campos ...
};
```

---

## 🧪 Casos de Prueba

### ✅ Caso 1: Sin Bono (applyMiViviendaBonus = false, bonusAmount = 0)

**Request**:
```http
POST /api/v1/simulations
Authorization: Bearer {token}
Content-Type: application/json

{
  "clientId": "guid-cliente",
  "principal": 300000,
  "currency": 1,
  "rateType": 1,
  "tea": 8.5,
  "termMonths": 240,
  "graceType": 0,
  "graceMonths": 0,
  "startDate": "2025-01-01",
  "applyMiViviendaBonus": false,
  "bonusAmount": 0,
  "lifeInsuranceRateMonthly": 0.00054,
  "riskInsuranceRateAnnual": 0.0015,
  "feesMonthly": 20
}
```

**Resultado Esperado**:
- ✅ Status: `201 Created`
- ✅ Sin errores de validación
- ✅ `simulation.BonusAmount` guardado como `0`
- ✅ Cálculo sin considerar bono: `montoFinanciado = principal`

---

### ✅ Caso 2: Sin Bono (applyMiViviendaBonus = false, bonusAmount = null)

**Request**:
```http
POST /api/v1/simulations
...
{
  ...
  "applyMiViviendaBonus": false,
  "bonusAmount": null
}
```

**Resultado Esperado**:
- ✅ Status: `201 Created`
- ✅ Sin errores de validación
- ✅ `simulation.BonusAmount` guardado como `0`
- ✅ Backend tolerante a `null` cuando no se aplica bono

---

### ✅ Caso 3: Sin Bono pero con Monto Mayor (applyMiViviendaBonus = false, bonusAmount = 20000)

**Request**:
```http
POST /api/v1/simulations
...
{
  ...
  "applyMiViviendaBonus": false,
  "bonusAmount": 20000  // ⚠️ Cliente envía monto pero no aplica bono
}
```

**Resultado Esperado**:
- ✅ Status: `201 Created`
- ✅ Sin errores de validación
- ✅ `simulation.BonusAmount` guardado como `0` (ignorado)
- ✅ Backend tolerante: ignora el monto enviado si `applyMiViviendaBonus` es `false`

**Nota**: Este caso demuestra que el backend es **tolerante a errores del frontend**.

---

### ✅ Caso 4: Con Bono Válido (applyMiViviendaBonus = true, bonusAmount = 20000)

**Request**:
```http
POST /api/v1/simulations
...
{
  ...
  "applyMiViviendaBonus": true,
  "bonusAmount": 20000
}
```

**Resultado Esperado**:
- ✅ Status: `201 Created`
- ✅ `simulation.BonusAmount` guardado como `20000`
- ✅ Cálculo con bono: `montoFinanciado = principal - bonusAmount`

---

### ❌ Caso 5: Con Bono pero Monto Cero (applyMiViviendaBonus = true, bonusAmount = 0)

**Request**:
```http
POST /api/v1/simulations
...
{
  ...
  "applyMiViviendaBonus": true,
  "bonusAmount": 0
}
```

**Resultado Esperado**:
- ❌ Status: `400 Bad Request`
- ❌ Error: `"El monto del bono debe ser mayor a cero"`
- ✅ Validación correcta: no permite bono de 0 cuando se indica que se aplica

---

### ❌ Caso 6: Con Bono pero Sin Monto (applyMiViviendaBonus = true, bonusAmount = null)

**Request**:
```http
POST /api/v1/simulations
...
{
  ...
  "applyMiViviendaBonus": true,
  "bonusAmount": null
}
```

**Resultado Esperado**:
- ❌ Status: `400 Bad Request`
- ❌ Error: `"El monto del bono es requerido cuando se aplica el bono Mi Vivienda"`
- ✅ Validación correcta: exige monto cuando se indica que se aplica bono

---

### ❌ Caso 7: Con Bono Mayor al Principal (applyMiViviendaBonus = true, bonusAmount = 400000)

**Request**:
```http
POST /api/v1/simulations
...
{
  ...
  "principal": 300000,
  "applyMiViviendaBonus": true,
  "bonusAmount": 400000  // > principal
}
```

**Resultado Esperado**:
- ❌ Status: `400 Bad Request`
- ❌ Error: `"El monto del bono no puede ser mayor al préstamo principal"`
- ✅ Validación correcta: protege contra datos inconsistentes

---

## 📊 Tabla Resumen de Comportamiento

| applyMiViviendaBonus | bonusAmount | Validación | bonusAmount Guardado | Cálculo |
|----------------------|-------------|------------|---------------------|---------|
| `false` | `0` | ✅ OK | `0` | Sin bono |
| `false` | `null` | ✅ OK | `0` | Sin bono |
| `false` | `20000` | ✅ OK (ignorado) | `0` | Sin bono |
| `true` | `20000` | ✅ OK | `20000` | Con bono |
| `true` | `0` | ❌ Error | - | - |
| `true` | `null` | ❌ Error | - | - |
| `true` | `> principal` | ❌ Error | - | - |

---

## 🔍 Lógica del Cálculo

### MortgageCalculatorService

El servicio de cálculo ya maneja correctamente el bono:

```csharp
// En MortgageCalculatorService.Calculate()
decimal principal = input.Principal;

// Solo aplica el bono si ApplyMiViviendaBonus es true y tiene valor
if (input.ApplyMiViviendaBonus && input.BonusAmount.HasValue)
{
    principal -= input.BonusAmount.Value;
}
```

Con la normalización implementada:
- Si `applyMiViviendaBonus = false` → `bonusAmount = 0` → Condición es `false` → No resta nada
- Si `applyMiViviendaBonus = true` → `bonusAmount = valor` → Condición es `true` → Resta el bono

---

## ✅ Beneficios de la Solución

1. **✅ Backend Tolerante**: Acepta `bonusAmount = 0` o `null` cuando `applyMiViviendaBonus = false`
2. **✅ Validación Condicional**: Solo valida el bono cuando realmente se aplica
3. **✅ Normalización Automática**: Fuerza `bonusAmount = 0` internamente si no se aplica
4. **✅ Cálculos Correctos**: El servicio de cálculo usa el valor normalizado
5. **✅ Datos Consistentes**: La base de datos guarda `0` cuando no hay bono
6. **✅ Sin Romper Funcionalidad**: Los casos válidos con bono siguen funcionando igual

---

## 🎯 Conclusión

✅ **Problema resuelto**: Ya no se dispara el error "El monto del bono debe ser mayor a cero" cuando `applyMiViviendaBonus = false`

✅ **Compilación exitosa**: `Build succeeded in 4.1s`

✅ **Listo para probar**: Swagger disponible en `http://localhost:5294/swagger`

**Próximo paso**: Probar los casos de prueba documentados arriba en Swagger para validar el comportamiento.
