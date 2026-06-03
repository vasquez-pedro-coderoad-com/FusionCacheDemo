# FusionCache Demo

Proyecto plantilla para pruebas de integración con **FusionCache** usando Clean Architecture y Dapper.

## Estructura del Proyecto

```
FusionCacheDemo/
├── FusionCacheDemo.Domain/
│   ├── Entities/
│   │   ├── Account.cs
│   │   └── Driver.cs
│   └── Interfaces/
│       ├── IRepository.cs
│       ├── IAccountRepository.cs
│       └── IDriverRepository.cs
│
├── FusionCacheDemo.Application/
│   ├── DTOs/
│   │   ├── AccountDto.cs
│   │   └── DriverDto.cs
│   ├── Interfaces/
│   │   ├── IAccountService.cs
│   │   └── IDriverService.cs
│   └── Services/
│       ├── AccountService.cs
│       └── DriverService.cs
│
├── FusionCacheDemo.Infrastructure/
│   ├── Data/
│   │   ├── SqlConnectionFactory.cs
│   │   ├── AccountRepository.cs
│   │   └── DriverRepository.cs
│   └── DependencyInjection.cs
│
├── FusionCacheDemo.API/
│   ├── Controllers/
│   │   ├── AccountsController.cs
│   │   └── DriversController.cs
│   └── Program.cs
│
└── Database/
    └── CreateDatabase.sql
```

## Stack Tecnologico

- .NET 10
- Dapper
- SQL Server
- Clean Architecture

## Configuracion

1. Ejecutar el script `Database/CreateDatabase.sql` en SQL Server
2. Ajustar connection string en `appsettings.json`
3. Ejecutar el proyecto

```bash
dotnet run --project FusionCacheDemo.API
```

## Endpoints

| Metodo | Endpoint | Descripcion |
|--------|----------|-------------|
| GET | /api/accounts | Listar cuentas |
| GET | /api/accounts/{id} | Obtener cuenta |
| POST | /api/accounts | Crear cuenta |
| PUT | /api/accounts/{id} | Actualizar cuenta |
| DELETE | /api/accounts/{id} | Eliminar cuenta |
| GET | /api/drivers | Listar drivers |
| GET | /api/drivers/active | Listar drivers activos |
| GET | /api/drivers/{id} | Obtener driver |
| POST | /api/drivers | Crear driver |
| PUT | /api/drivers/{id} | Actualizar driver |
| DELETE | /api/drivers/{id} | Eliminar driver |
