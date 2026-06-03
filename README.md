# FusionCache Demo

Proyecto plantilla para pruebas de integracion con **FusionCache** usando Clean Architecture, Dapper y Redis.

## Arquitectura

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                            │
│                      Controllers                            │
└─────────────────────────┬───────────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────────┐
│                   Application Layer                         │
│                      Services                               │
└─────────────────────────┬───────────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────────┐
│                  Infrastructure Layer                       │
│  ┌─────────────────────────────────────────────────────┐   │
│  │              CachedRepository (Decorator)            │   │
│  │  ┌─────────────┐         ┌────────────────────┐     │   │
│  │  │ FusionCache │ ──────▶ │  Repository        │     │   │
│  │  │ L1+L2+Backplane│      │  (Dapper)          │     │   │
│  │  └─────────────┘         └────────────────────┘     │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
              │                           │
       ┌──────▼──────┐             ┌──────▼──────┐
       │    Redis    │             │ SQL Server  │
       │ L2 + Pub/Sub│             │             │
       └─────────────┘             └─────────────┘
```

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
│   ├── Interfaces/
│   └── Services/
│
├── FusionCacheDemo.Infrastructure/
│   ├── Cache/
│   │   ├── CachedAccountRepository.cs   <-- Decorator
│   │   └── CachedDriverRepository.cs    <-- Decorator
│   ├── Data/
│   │   ├── AccountRepository.cs         <-- Dapper
│   │   └── DriverRepository.cs          <-- Dapper
│   └── DependencyInjection.cs
│
├── FusionCacheDemo.API/
│   ├── Controllers/
│   ├── Dockerfile
│   └── Program.cs
│
├── Database/
│   └── CreateDatabase.sql
│
└── docker-compose.yml
```

## Stack Tecnologico

- .NET 10
- FusionCache (L1 + L2 + Backplane)
- Redis
- Dapper
- SQL Server
- Docker / K3D Ready

## FusionCache - Capacidades

| Caracteristica | Descripcion |
|----------------|-------------|
| **L1 Cache** | Memoria local por pod (ultra rapido) |
| **L2 Cache** | Redis compartido entre pods |
| **Backplane** | Invalidacion cross-pod via pub/sub |
| **Fail-Safe** | Sirve datos stale si Redis/DB caen |
| **Stampede Protection** | N requests concurrentes = 1 DB call |
| **Eager Refresh** | Renueva cache en background |

## Configuracion FusionCache

```csharp
services.AddFusionCache()
    .WithDefaultEntryOptions(options => options
        .SetDuration(TimeSpan.FromMinutes(5))           // TTL
        .SetFailSafe(true, TimeSpan.FromMinutes(15))    // Stale max 15min
        .SetFactoryTimeouts(
            softTimeout: TimeSpan.FromMilliseconds(100),
            hardTimeout: TimeSpan.FromMilliseconds(500)))
    .WithSerializer(new FusionCacheNewtonsoftJsonSerializer())
    .WithDistributedCache(redisCache)
    .WithBackplane(redisBackplane);
```

## Quick Start

### Con Docker Compose

```bash
# Levantar Redis + SQL Server + 2 Pods API
docker compose up --build

# Pod A: http://localhost:5100
# Pod B: http://localhost:5101
```

### Local (Desarrollo)

```bash
# 1. Levantar Redis y SQL Server
docker compose up redis sqlserver -d

# 2. Crear base de datos
sqlcmd -S localhost,1433 -U sa -P "Welcome123@" -i Database/CreateDatabase.sql

# 3. Ejecutar API
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

## Verificar Cache (Redis CLI)

```bash
# Conectar a Redis
docker exec -it fusioncachedemo-redis-1 redis-cli

# Ver keys
KEYS *

# Ver valor
GET account:1
```

## Para K3D

El proyecto esta preparado para Kubernetes. Los pods comparten:
- Redis como L2 cache
- Backplane para invalidacion automatica

Cuando Pod A actualiza un registro:
1. Actualiza DB
2. Invalida su L1
3. Publica mensaje en Redis pub/sub
4. Pod B/C/D reciben mensaje y invalidan su L1
