# Urbania360 Backend API

Backend mínimo funcional para **Urbania360** desarrollado con **.NET 9 (C#)**, **Entity Framework Core**, **JWT Authentication**, **Swagger**, y **Clean Architecture ligera**.

## 🏗️ Arquitectura

La solución está organizada en 3 proyectos principales:

- **Urbania360.Domain**: Entidades, enums y lógica de dominio
- **Urbania360.Infrastructure**: DbContext, servicios de infraestructura y acceso a datos
- **Urbania360.Api**: Controllers, DTOs, validaciones y configuración de la Web API

## 🚀 Tecnologías

- **.NET 9** (C#, nullable enabled, LangVersion latest)
- **Entity Framework Core 9.0** con SQL Server/Azure SQL
- **JWT Authentication** (HS256)
- **Swagger/OpenAPI** para documentación
- **AutoMapper** para mapeo DTO ↔ Entity
- **FluentValidation** para validaciones
- **BCrypt.Net** para hash de contraseñas

## 📊 Modelo de Datos

### Entidades Principales

- **User**: Usuarios del sistema (Admin, Agent, Client)
- **Client**: Clientes del sistema
- **Property**: Propiedades inmobiliarias
- **LoanSimulation**: Simulaciones de préstamos hipotecarios
- **Bank**: Catálogo de bancos
- **ActivityLog**: Log de actividades (alta volumetría)

### Endpoints Activos

#### 🔐 Authentication (`/api/v1/auth`)
- `POST /register` - Registrar nuevo usuario
- `POST /login` - Iniciar sesión

#### 👥 Clients (`/api/v1/clients`) [Requiere Auth: Admin/Agent]
- `GET /` - Lista paginada de clientes (con búsqueda)
- `GET /{id}` - Obtener cliente por ID
- `POST /` - Crear nuevo cliente
- `PUT /{id}` - Actualizar cliente
- `DELETE /{id}` - Eliminar cliente

#### 📈 Reports (`/api/v1/reports`) [Requiere Auth]
- `GET /summary` - Resumen general del sistema
- `GET /most-consulted-properties` - Propiedades más consultadas
- `GET /simulations-by-month` - Estadísticas mensuales de simulaciones

## ⚙️ Configuración e Instalación

### 1. Prerrequisitos

- **.NET 9 SDK**
- **SQL Server** o **Azure SQL Database**
- **Visual Studio 2022** o **VS Code** (opcional)

### 2. Configuración de la Base de Datos

1. **Reemplazar la cadena de conexión** en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tu-servidor;Database=Urbania360;User Id=tu-usuario;Password=tu-password;TrustServerCertificate=true;"
  }
}
```

2. **Configurar claves JWT** en `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "tu-clave-secreta-minimo-32-caracteres-para-hs256",
    "Issuer": "Urbania360",
    "Audience": "Urbania360"
  }
}
```

### 3. Instalación y Ejecución

```bash
# 1. Restaurar paquetes NuGet
dotnet restore

# 2. Compilar la solución
dotnet build

# 3. Aplicar migraciones a la base de datos
dotnet ef database update -s Urbania360.Api -p Urbania360.Infrastructure

# 4. Ejecutar la aplicación
dotnet run --project Urbania360.Api
```

### 4. Acceso a la API

- **URL Base**: `https://localhost:7164` o `http://localhost:5294`
- **Swagger UI**: `https://localhost:7164/swagger` (se abre automáticamente)
- **Redirección**: La raíz `/` redirige automáticamente a `/swagger`

## 🔑 Datos de Prueba (Seed Data)

El sistema incluye datos iniciales para pruebas:

### Usuario Administrador
- **Email**: `admin@urbania360.com`
- **Password**: `Password123!`
- **Role**: Admin

### Bancos Precargados
- BCP (TEA: 8.50%)
- Interbank (TEA: 8.90%)
- Scotiabank (TEA: 9.20%)

### Propiedades Demo
- 4 propiedades de ejemplo (P0001-P0004)
- 3 clientes de ejemplo
- Actividades y consultas de prueba

## 🧪 Cómo Probar la API

### 1. Autenticación

1. **Registrar un nuevo usuario** o usar el admin seed:
   ```
   POST /api/v1/auth/login
   {
     "email": "admin@urbania360.com",
     "password": "Password123!"
   }
   ```

2. **Copiar el token** de la respuesta

3. **En Swagger**: Hacer clic en **"Authorize"** y pegar: `Bearer tu-token-aqui`

### 2. Gestión de Clientes

Con el token configurado, probar los endpoints de clientes:

```json
POST /api/v1/clients
{
  "firstName": "Juan",
  "lastName": "Pérez",
  "email": "juan@email.com",
  "phone": "+51987654321",
  "annualIncome": 60000.00
}
```

### 3. Reportes

Consultar estadísticas del sistema:
- `GET /api/v1/reports/summary`
- `GET /api/v1/reports/most-consulted-properties`

## 📝 Estructura de Carpetas

```
Urbania360/
├── Urbania360.Domain/
│   ├── Entities/           # Entidades del dominio
│   └── Enums/             # Enumeraciones
├── Urbania360.Infrastructure/
│   ├── Data/              # DbContext y configuraciones
│   └── Services/          # Servicios de infraestructura
├── Urbania360.Api/
│   ├── Controllers/       # API Controllers
│   ├── DTOs/             # Data Transfer Objects
│   ├── Mappings/         # AutoMapper Profiles
│   └── Validators/       # FluentValidation Rules
└── publish/
    └── sql/              # Scripts SQL de migración
```

## 🚀 Despliegue

### Script SQL para Producción

Se genera automáticamente un script idempotente en:
```
publish/sql/Init.sql
```

### Comandos de Migración

```bash
# Crear nueva migración
dotnet ef migrations add NombreMigracion -s Urbania360.Api -p Urbania360.Infrastructure

# Aplicar migraciones
dotnet ef database update -s Urbania360.Api -p Urbania360.Infrastructure

# Generar script SQL
dotnet ef migrations script --idempotent -s Urbania360.Api -p Urbania360.Infrastructure -o publish/sql/Deploy.sql
```

## 🔮 Próximos Pasos

Las siguientes entidades están modeladas pero **sin endpoints**:

- **Property**: Gestión de propiedades inmobiliarias
- **LoanSimulation**: Simulador de préstamos hipotecarios
- **AmortizationItem**: Tabla de amortización detallada
- **PropertyImage**: Imágenes de propiedades
- **PropertyConsult**: Seguimiento de consultas

## 🛡️ Seguridad

- ✅ **JWT Authentication** con expiración de 12 horas
- ✅ **Role-based Authorization** (Admin, Agent, User)
- ✅ **Password Hashing** con BCrypt
- ✅ **Input Validation** con FluentValidation
- ✅ **CORS** configurado para frontend
- ✅ **Activity Logging** para auditoría

## 📧 Soporte

Para consultas técnicas, contacta al equipo de desarrollo en `admin@urbania360.com`.

---

**Urbania360** - Sistema Financiero Inmobiliario 🏠💰