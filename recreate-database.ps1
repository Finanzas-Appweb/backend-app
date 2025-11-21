# Script para recrear la base de datos de Urbania360
# Ejecutar desde el directorio FINANZASBACKEND

Write-Host "=== Urbania360 - Recrear Base de Datos ===" -ForegroundColor Cyan
Write-Host ""

# Verificar que estamos en el directorio correcto
if (-not (Test-Path "Urbania360.sln")) {
    Write-Host "Error: Este script debe ejecutarse desde el directorio FINANZASBACKEND" -ForegroundColor Red
    exit 1
}

# Paso 1: Limpiar migraciones anteriores
Write-Host "Paso 1: Limpiando migraciones anteriores..." -ForegroundColor Yellow
if (Test-Path "Urbania360.Infrastructure\Migrations") {
    Remove-Item -Path "Urbania360.Infrastructure\Migrations\*" -Recurse -Force
    Write-Host "✓ Migraciones anteriores eliminadas" -ForegroundColor Green
}

# Paso 2: Crear nueva migración
Write-Host ""
Write-Host "Paso 2: Creando nueva migración..." -ForegroundColor Yellow
$migrationResult = dotnet ef migrations add CompleteUrbaniaSchema `
    -s Urbania360.Api/Urbania360.Api.csproj `
    -p Urbania360.Infrastructure/Urbania360.Infrastructure.csproj `
    2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Migración creada exitosamente" -ForegroundColor Green
} else {
    Write-Host "✗ Error al crear la migración:" -ForegroundColor Red
    Write-Host $migrationResult
    exit 1
}

# Paso 3: Aplicar migración (opcional - descomentar si se desea)
Write-Host ""
Write-Host "Paso 3: ¿Desea aplicar la migración a la base de datos ahora? (S/N)" -ForegroundColor Yellow
$response = Read-Host

if ($response -eq "S" -or $response -eq "s") {
    Write-Host "Aplicando migración a la base de datos..." -ForegroundColor Yellow
    
    $updateResult = dotnet ef database update `
        -s Urbania360.Api/Urbania360.Api.csproj `
        -p Urbania360.Infrastructure/Urbania360.Infrastructure.csproj `
        2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Base de datos actualizada exitosamente" -ForegroundColor Green
    } else {
        Write-Host "✗ Error al actualizar la base de datos:" -ForegroundColor Red
        Write-Host $updateResult
        exit 1
    }
} else {
    Write-Host "Migración creada pero no aplicada. Ejecute manualmente:" -ForegroundColor Cyan
    Write-Host "dotnet ef database update -s Urbania360.Api/Urbania360.Api.csproj -p Urbania360.Infrastructure/Urbania360.Infrastructure.csproj" -ForegroundColor White
}

Write-Host ""
Write-Host "=== Proceso completado ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para ejecutar la aplicación:" -ForegroundColor Yellow
Write-Host "  cd Urbania360.Api" -ForegroundColor White
Write-Host "  dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "Swagger estará disponible en:" -ForegroundColor Yellow
Write-Host "  http://localhost:5294/swagger" -ForegroundColor White
Write-Host ""
