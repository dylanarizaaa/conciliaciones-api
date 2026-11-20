# DOTNET Servicios Hex Conciliaciones

[![Tecnología](https://img.shields.io/badge/Tech-.NET%208.0-512BD4)](https://dotnet.microsoft.com/)
[![Arquitectura](https://img.shields.io/badge/Architecture-Hexagonal%20%2B%20CQRS-green)](https://en.wikipedia.org/wiki/Hexagonal_architecture_(software))
[![Plataforma](https://img.shields.io/badge/Platform-Cloud%20Native%20%2F%20On-Premises-orange)](https://www.w3schools.com/)
[![Base de Datos](https://img.shields.io/badge/Database-MongoDB%20%2F%20DocumentDB-brightgreen)](https://www.mongodb.com/)

## 📝 Descripción

**DOTNET_Servicios_Hex_Conciliaciones** es un microservicio de conciliación empresarial construido con **.NET 8** que implementa una **arquitectura hexagonal (Ports & Adapters)** junto con el patrón **CQRS (Command Query Responsibility Segregation)** mediado por **MediatR**. El sistema proporciona servicios de parametrización de reglas de negocio, validación de conciliaciones de datos y gestión de procesos de reconciliación con integración a **Keycloak** para autenticación y autorización empresarial. Utiliza **MongoDB/Amazon DocumentDB** como motor de persistencia principal con soporte para **Entity Framework Core** como opción complementaria.

---

## 🏗️ Arquitectura del Sistema

### 🔷 Patrón Arquitectónico: Hexagonal (Ports & Adapters) + CQRS

El proyecto implementa una **arquitectura limpia en 4 capas** con separación clara de responsabilidades:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         🌐 API Layer (Entrada)                           │
│  Controllers (ConciliacionesController, HealthCheckController, etc.)    │
│  Middlewares (ErrorHandlerMiddleware, Autenticación JWT/Keycloak)       │
└────────────────────────────┬────────────────────────────────────────────┘
                             │
┌────────────────────────────▼────────────────────────────────────────────┐
│               📋 Application Layer (Casos de Uso - CQRS)                 │
│  Features/Commands (RuleParameterizationCommand)                         │
│  Features/Queries (GetConciliationDetailsQuery, GetRulesByIdSpQuery)    │
│  Behaviors (ValidationBehavior, MediatR Pipeline)                        │
│  MediatR Handlers (CommandHandlers, QueryHandlers)                       │
└────────────────────────────┬────────────────────────────────────────────┘
                             │
┌────────────────────────────▼────────────────────────────────────────────┐
│                    🔷 Domain Layer (Lógica Central)                      │
│  Entities (RuleParameterization, ReconciliationResults, RuleConcepts)  │
│  DTOs (Request/Response Models)                                          │
│  Ports/Interfaces (IReconciliarionResultsRepository, etc.)               │
│  Wrappers (Answers<T> - Respuesta Estándar)                             │
└────────────────────────────┬────────────────────────────────────────────┘
                             │
┌────────────────────────────▼────────────────────────────────────────────┐
│              🔌 Infrastructure Layer (Adaptadores & Comunicación)        │
│  Repositories (ReconciliarionResultRepository, RuleConceptsRepository) │
│  DocumentDBContext (MongoDB/DocumentDB Connection)                       │
│  External Services (ParameterStoreService - AWS Systems Manager)         │
│  Security (RealDecryptor - Descifrador AES-256)                         │
│  Configuration (DocumentDbSettings, AwsSettings)                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 📊 Diagrama de Flujo de CQRS + MediatR

```mermaid
sequenceDiagram
    participant Client as 🌐 Cliente HTTP
    participant Controller as 📡 Controller
    participant Mediator as 🔄 MediatR Mediator
    participant Handler as ⚙️ Handler (Command/Query)
    participant Repository as 🗄️ Repository
    participant DB as 💾 MongoDB/DocumentDB

    Client->>Controller: POST/GET /api/v1/...
    activate Controller
    Controller->>Mediator: mediator.Send(Command/Query)
    activate Mediator

    Mediator->>Mediator: ValidationBehavior (Pipeline)

    Mediator->>Handler: Handle(Command/Query)
    activate Handler

    Handler->>Repository: Query/Store Data
    activate Repository

    Repository->>DB: MongoDB Operations
    activate DB
    DB-->>Repository: Result/Rows
    deactivate DB

    Repository-->>Handler: Data
    deactivate Repository

    Handler-->>Mediator: Answers<T> (Response)
    deactivate Handler

    Mediator-->>Controller: Answers<T>
    deactivate Mediator

    Controller-->>Client: JSON (200/400/404/500)
    deactivate Controller
```

### 🗂️ Estructura de Capas (Clean Architecture)

```
Conciliaciones.Api [Web API - Presentation Layer]
├── Controllers/
│   ├── BaseApiController.cs           ← Clase base con MediatR
│   └── v1/
│       ├── ConciliacionesController   ← 4 endpoints (CRUD Reglas/Conciliaciones)
│       ├── HealthCheckController      ← 3 endpoints (Health Checks)
│       └── TestConnectionController   ← 2 endpoints (Validación BD)
├── Extensions/                        ← Inyección de Dependencias
├── Middlewares/                       ← ErrorHandlerMiddleware
├── Properties/                        ← launchSettings.json
├── appsettings.json                   ← Configuración base
├── appsettings.Development.json       ← Overwrites locales
├── Program.cs                         ← Configuración Startup
├── Dockerfile                         ← Deployment
└── docker-compose.yml                 ← Local Development

Conciliaciones.Aplicacion [Business Logic Layer - Application]
├── Features/
│   ├── RuleParameterizations/         ← Parametrización de Reglas
│   │   ├── Commands/
│   │   │   └── CreateRulesParametrization/
│   │   │       ├── RuleParameterizationCommand    ← IRequest<T>
│   │   │       ├── RuleParameterizationHandler    ← IRequestHandler<T, R>
│   │   │       └── RuleParameterizationResponse   ← Data Transfer Object
│   │   └── Querys/
│   │       ├── GetProcessById/
│   │       ├── GetProyects/
│   │       └── GetSubProcessById/
│   ├── ReconciliationResults/         ← Resultados de Conciliación
│   │   └── Querys/
│   │       ├── GetConciliationDetails/
│   │       └── GetRuleDetailsPaged/
│   └── RulesConcepts/                 ← Concepto de Reglas
│       └── Querys/
│           └── GetRulesByIdSp/
├── Common/
│   └── Behaviors/
│       └── ValidationBehavior.cs      ← Pipeline Behavior (FluentValidation)
└── DependencyInjection.cs             ← Registro de MediatR

Conciliaciones.Dominio [Core Business Rules - Domain]
├── DTOs/
│   ├── Requests/
│   │   └── RuleParameterization/
│   │       └── RuleParameterizationClientDto
│   └── Response/
│       └── Reconciliation/
│           └── GetRuleDetailsPagedResponseDto
├── Entities/
│   ├── RuleParameterization.cs
│   ├── ReconciliationResults.cs
│   ├── RuleConcepts.cs
│   ├── DataSource.cs
│   ├── AssociatedRule.cs
│   └── ConciliationField.cs
├── Ports/                             ← Hexagonal Architecture Ports
│   └── Interfaces/
│       ├── IReconciliarionResultsRepository
│       ├── IRuleConceptsRepository
│       └── IRuleParameterizationRepository
├── Wrappers/
│   ├── Answers<T>                     ← Respuesta Estándar Genérica
│   └── PagedList<T>
└── Utils/
    └── ConstantData.cs

Conciliaciones.Infraestructura [Data Access & External Services - Infrastructure]
├── Persistence/
│   ├── DocumentDB/
│   │   ├── DocumentDBContext.cs       ← MongoDB/DocumentDB Context
│   │   ├── Builders/                  ← Query Builders
│   │   │   ├── GetRuleDetailsPagedBuilder
│   │   │   ├── GetRulesByIdProcess
│   │   │   └── GetRulesByNameIdConBuilder
│   │   └── Constants/
│   │       └── DocumentDBCollectionNames
│   └── Repositories/                  ← Repository Pattern (Adaptadores)
│       ├── ReconciliarionResultRepository
│       ├── RuleConceptsRepository
│       └── RuleParameterizationRepository
├── Ports/
│   └── Interfaces/
│       ├── AWS/
│       │   └── IParameterStoreService
│       └── DBContext/
│           └── IDocumentDBContext
├── Security/
│   ├── RealDecryptor.cs               ← Cifrado AES-256
│   └── DecryptorFacade.cs
├── Settings/
│   ├── DocumentDbConfigurationExtension.cs
│   ├── DocumentDbSettings.cs
│   └── ParameterStorageService.cs  ← AWS Systems Manager
└── DependencyInjection.cs             ← Registro de Infraestructura
```

---

## 📁 Estructura Completa de Directorios (Auditoría 100%)

```
DOTNET_Servicios_Hex_Conciliaciones/
│
├── 📄 DOTNET_Servicios_Hex_Conciliaciones.slnx          ← Solución
│
├── 📂 Conciliaciones.Api/                               ← Web API (.NET Web SDK)
│   ├── 📄 Conciliaciones.Api.csproj
│   ├── 📄 Program.cs                                    ← Configuración Startup
│   ├── 📄 appsettings.json                              ← Configuración base
│   ├── 📄 appsettings.Development.json                  ← Configuración desarrollo
│   ├── 📄 .env                                          ← Variables de entorno
│   ├── 📄 docker-compose.yml                            ← Docker Compose (MongoDB local)
│   ├── 📄 Dockerfile                                    ← Multi-stage build
│   │
│   ├── 📂 Controllers/
│   │   ├── 📄 BaseApiController.cs                      ← Base con MediatR
│   │   └── 📂 v1/
│   │       ├── 📄 ConciliacionesController.cs           ← 4 Endpoints (CRUD Reglas)
│   │       ├── 📄 HealthCheckController.cs              ← 3 Endpoints (Health Checks)
│   │       └── 📄 TestConnectionController.cs           ← 2 Endpoints (BD Validation)
│   │
│   ├── 📂 Properties/
│   │   └── 📄 launchSettings.json                       ← Perfiles de ejecución
│   │
│   ├── 📂 Extensions/
│   │   └── 📄 [Inyección de Dependencias]
│   │
│   ├── 📂 Middlewares/
│   │   └── 📄 ErrorHandlerMiddleware.cs                 ← Manejo de errores global
│   │
│   └── 📂 bin/Debug/net8.0/                             ← Compilación debug
│
├── 📂 Conciliaciones.Aplicacion/                        ← Application Layer (.NET SDK)
│   ├── 📄 Conciliaciones.Aplicacion.csproj
│   ├── 📄 DependencyInjection.cs                        ← Registro MediatR + Validación
│   │
│   ├── 📂 Features/
│   │   │
│   │   ├── 📂 RuleParameterizations/
│   │   │   │
│   │   │   ├── 📂 Commands/
│   │   │   │   └── 📂 CreateRulesParametrization/
│   │   │   │       ├── 📄 RuleParameterizationCommand.cs
│   │   │   │       ├── 📄 RuleParameterizationHandler.cs
│   │   │   │       ├── 📄 RuleParameterizationResponse.cs
│   │   │   │       └── 📄 RuleParameterizationValidator.cs (FluentValidation)
│   │   │   │
│   │   │   └── 📂 Querys/
│   │   │       ├── 📂 GetProcessById/
│   │   │       │   ├── 📄 GetProcessByIdQuery.cs
│   │   │       │   └── 📄 GetProcessByIdHandler.cs
│   │   │       │
│   │   │       ├── 📂 GetProyects/
│   │   │       │   ├── 📄 GetProyectsQuery.cs
│   │   │       │   └── 📄 GetProyectsHandler.cs
│   │   │       │
│   │   │       └── 📂 GetSubProcessById/
│   │   │           ├── 📄 GetSubProcessByIdQuery.cs
│   │   │           └── 📄 GetSubProcessByIdHandler.cs
│   │   │
│   │   ├── 📂 ReconciliationResults/
│   │   │   └── 📂 Querys/
│   │   │       ├── 📂 GetConciliationDetails/
│   │   │       │   ├── 📄 GetConciliationDetailsQuery.cs
│   │   │       │   ├── 📄 GetConciliationDetailsHandler.cs
│   │   │       │   └── 📄 GetConciliationDetailsResponse.cs
│   │   │       │
│   │   │       └── 📂 GetRuleDetailsPaged/
│   │   │           ├── 📄 GetRuleDetailsPagedQuery.cs
│   │   │           ├── 📄 GetRuleDetailsPagedHandler.cs
│   │   │           └── 📄 GetRuleDetailsPagedResponseDto.cs
│   │   │
│   │   └── 📂 RulesConcepts/
│   │       └── 📂 Querys/
│   │           └── 📂 GetRulesByIdSp/
│   │               ├── 📄 GetRulesByIdSpQuery.cs
│   │               ├── 📄 GetRulesByIdSpHandler.cs
│   │               └── 📄 GetRulesByIdSpResponse.cs
│   │
│   ├── 📂 Common/
│   │   └── 📂 Behaviors/
│   │       └── 📄 ValidationBehavior.cs                 ← Pipeline Behavior FluentValidation
│   │
│   └── 📂 bin/Debug/net8.0/                             ← Compilación debug
│
├── 📂 Conciliaciones.Dominio/                           ← Domain Layer (.NET SDK)
│   ├── 📄 Conciliaciones.Dominio.csproj
│   │
│   ├── 📂 Entities/
│   │   ├── 📂 ReconciliationResults/
│   │   │   └── 📄 ReconciliarionResults.cs
│   │   ├── 📂 RuleParameterization/
│   │   │   ├── 📄 RuleParameterizationEntity.cs
│   │   │   ├── 📄 DataSource.cs
│   │   │   ├── 📄 AssociatedRule.cs
│   │   │   └── 📄 ConciliationField.cs
│   │   └── 📂 RuleConcepts/
│   │       └── 📄 RuleConceptsEntity.cs
│   │
│   ├── 📂 DTOs/
│   │   ├── 📂 Requests/
│   │   │   └── 📂 RuleParameterization/
│   │   │       └── 📄 RuleParameterizationClientDto.cs
│   │   │
│   │   └── 📂 Response/
│   │       ├── 📂 Reconciliation/
│   │       │   ├── 📄 GetRuleDetailsPagedResponseDto.cs
│   │       │   ├── 📄 ReconciliationResultsDtos.cs
│   │       │   └── 📄 GetRulesByIdSpResponse.cs
│   │       │
│   │       └── 📂 Rules/
│   │           └── [Response DTOs específicos]
│   │
│   ├── 📂 Ports/                                        ← Hexagonal Ports (Abstracciones)
│   │   └── 📂 Interfaces/
│   │       ├── 📄 IReconciliarionResultsRepository.cs   ← Port abstracción
│   │       ├── 📄 IRuleConceptsRepository.cs
│   │       ├── 📄 IRuleParameterizationRepository.cs
│   │       └── 📂 DocumentDb/
│   │           └── 📄 IDocryptor.cs
│   │
│   ├── 📂 Wrappers/
│   │   ├── 📄 Answers<T>.cs                             ← Respuesta Estándar Genérica
│   │   └── 📄 PagedList<T>.cs
│   │
│   ├── 📂 Utils/
│   │   ├── 📄 ConstantData.cs
│   │   └── 📄 Utilities.cs
│   │
│   └── 📂 bin/Debug/net8.0/                             ← Compilación debug
│
├── 📂 Conciliaciones.Infraestructura/                   ← Infrastructure Layer (.NET SDK)
│   ├── 📄 Conciliaciones.Infraestructura.csproj
│   ├── 📄 DependencyInjection.cs                        ← Registro de Repositorios/Servicios
│   │
│   ├── 📂 Persistence/
│   │   ├── 📂 DocumentDB/
│   │   │   ├── 📄 DocumentDBContext.cs                  ← MongoDB Connection Manager
│   │   │   │
│   │   │   ├── 📂 Builders/
│   │   │   │   ├── 📄 GetRuleDetailsPagedBuilder.cs
│   │   │   │   ├── 📄 GetRulesByIdProcess.cs
│   │   │   │   └── 📄 GetRulesByNameIdConBuilder.cs
│   │   │   │
│   │   │   └── 📂 Constants/
│   │   │       └── 📄 DocumentDBCollectionNames.cs
│   │   │
│   │   └── 📂 Repositories/                             ← Adaptadores (Hexagonal)
│   │       ├── 📄 ReconciliarionResultRepository.cs
│   │       ├── 📄 RuleConceptsRepository.cs
│   │       └── 📄 RuleParameterizationRepository.cs
│   │
│   ├── 📂 Ports/
│   │   └── 📂 Interfaces/
│   │       ├── 📂 AWS/
│   │       │   └── 📄 IParameterStoreService.cs
│   │       └── 📂 DBContext/
│   │           └── 📄 IDocumentDBContext.cs
│   │
│   ├── 📂 Security/
│   │   ├── 📄 RealDecryptor.cs                          ← Cifrado AES-256
│   │   └── 📄 DecryptorFacade.cs
│   │
│   ├── 📂 Settings/
│   │   ├── 📄 DocumentDbConfigurationExtension.cs
│   │   ├── 📄 DocumentDbSettings.cs
│   │   ├── 📄 ParameterStorageService.cs                ← AWS Systems Manager
│   │   └── 📄 AwsSettings.cs
│   │
│   └── 📂 bin/Debug/net8.0/                             ← Compilación debug

└── 📂 .git/                                              ← Control de versiones

```

---

## ⚙️ Requisitos Previos

- **Entorno de Ejecución:** .NET 8.0 SDK (Windows, Linux, macOS)
- **Motor de Base de Datos Principal:** [DUAL]
  - **MongoDB 5.0+** / **Amazon DocumentDB 4.0+ (MongoDB-compatible)** (Producción/Dev)
  - **Microsoft SQL Server 2019+** (Opcional fallback con EF Core)
- **Caché & Persistencia Secundaria:** Redis (Opcional para caché distribuida)
- **IDE Recomendado:** Visual Studio 2022 Professional / VS Code + C# DevKit
- **Runtime Adicional:** Docker & Docker Compose (para BD local)
- **Autenticación:** Keycloak 18+ (servidor de identidad OIDC)
- **AWS (Producción):** AWS Systems Manager Parameter Store (gestión de secretos)
- **Herramientas CLI:** dotnet CLI v8.0+, git 2.40+

---

## 🚀 Inicio Rápido y Despliegue Local

### 1. Clonar el Repositorio

```bash
git clone https://dev.azure.com/interrapidisimo/Servicios/_git/DOTNET_Servicio_Hex_Conciliaciones
cd DOTNET_Servicios_Hex_Conciliaciones
```

### 2. Restaurar Dependencias NuGet

```bash
dotnet restore DOTNET_Servicios_Hex_Conciliaciones.slnx
```

### 3. Levantar MongoDB/DocumentDB Localmente (Docker)

```bash
docker-compose up -d
```

Verificar conexión:
```bash
# Conexión MongoDB local en puerto 27017
# Usuario: admin
# Contraseña: secretpassword
mongosh mongodb://admin:secretpassword@localhost:27017
```

### 4. Ejecutar en Entorno Local (Perfiles Disponibles)

#### Perfil HTTP (Puerto 5029)
```bash
dotnet run --project Conciliaciones.Api -p:Configuration=Debug --launch-profile http
# 🌐 URL Local: http://localhost:5029/swagger
```

#### Perfil HTTPS (Puerto 7126 + 5029)
```bash
dotnet run --project Conciliaciones.Api -p:Configuration=Debug --launch-profile https
# 🔒 URL Local Swagger: https://localhost:7126/swagger
# 🌐 URL Local HTTP: http://localhost:5029
```

#### Perfil IIS Express (Puerto 31807 HTTP / 44316 HTTPS)
```bash
# Requiere IIS Express instalado
dotnet run --project Conciliaciones.Api -p:Configuration=Debug --launch-profile "IIS Express"
# 🌐 URL Local: http://localhost:31807/swagger
# 🔒 URL Segura: https://localhost:44316/swagger
```

### 5. Compilación para Producción

```bash
dotnet publish -c Release -o ./publish/
```

### 6. Desplegar con Docker (Contenedor)

```bash
docker build -t conciliaciones-api:latest .
docker run -d -p 8080:80 -p 8443:443 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e KEYCLOAK_AUTHORITY=https://keycloak-prod.interrapidisimo.co/realms/Interrapidisimo \
  conciliaciones-api:latest
```

---

## 🔐 Configuración, Secretos y Cadenas de Conexión

### Archivos de Configuración

| Archivo | Ubicación | Propósito |
| --- | --- | --- |
| `appsettings.json` | Conciliaciones.Api/ | Configuración base (logging, hosts) |
| `appsettings.Development.json` | Conciliaciones.Api/ | Overrides para ambiente local |
| `.env` | Conciliaciones.Api/ | Variables de entorno (secretos, conexiones) |
| `launchSettings.json` | Conciliaciones.Api/Properties/ | Perfiles de ejecución local |
| `docker-compose.yml` | Conciliaciones.Api/ | Configuración servicios Docker locales |

### Variables de Entorno (Clave: Valor)

| Variable | Descripción | Ejemplo / Ubicación |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Ambiente de ejecución | `Development`, `Staging`, `Production` |
| `ASPNETCORE_HTTP_PORTS` | Puerto HTTP | `5029` (desde launchSettings.json) |
| `KEYCLOAK_AUTHORITY` | URL autoridad Keycloak | `https://keycloak-desarrollo.interrapidisimo.co/realms/Interrapidisimo` |
| `KEYCLOAK_AUDIENCE` | Audience JWT | `account` |
| `KEYCLOAK_REQUIRE_HTTPS_METADATA` | Validar metadata HTTPS | `false` (Desarrollo) / `true` (Producción) |
| `REQUIRED_SCOPE` | Scope requerido OAuth2 | `interpay` |
| `REQUIRED_ROLE` | Rol requerido en JWT | [Token cifrado en .env] |
| `LLAVE_CIFRADA` | Clave base AES-256 (Base64) | [Almacenada en .env cifrada] |
| `PDB` | Valor de sal Rfc2898DeriveBytes (Base64) | [Almacenada en .env cifrada] |
| `PROVIDER` | Proveedor DocumentDB cifrado | [Credenciales AWS cifradas] |
| `CONNECTION_STRING` | Conexión MongoDB/DocumentDB cifrada | [String de conexión cifrado] |
| `DATABASE` | Nombre base de datos | [Nombre cifrado en .env] |
| `USESSL` | Habilitar SSL en MongoDB | `false` (Local) / `true` (Producción DocumentDB) |
| `Region` | Región AWS | `us-east-1` |

### Cadenas de Conexión Principales

#### MongoDB Local (docker-compose.yml)
```
mongodb://admin:secretpassword@localhost:27017/nombre-bd?authSource=admin
```

#### Amazon DocumentDB (Producción - Desde AWS Systems Manager Parameter Store)
```
mongodb://username:password@docdb-cluster.region.docdb.amazonaws.com:27017/nombre-bd?
  ssl=true&replicaSet=rs0&readPreference=secondaryPreferred&retryWrites=false
```

### Gestión de Secretos

**Desarrollo Local:**
- Variables almacenadas en `.env` y cargadas con `DotNetEnv`
- Cifrado AES-256 con sal Rfc2898DeriveBytes

**Producción (AWS):**
- Gestión centralizada vía **AWS Systems Manager Parameter Store**
- Recuperación en runtime por `ParameterStorageService`
- Autenticación por IAM Role del EC2/ECS/Lambda

**Autenticación Keycloak:**
- Bearer JWT en header `Authorization: Bearer <token>`
- Validación por JWT Middleware
- Scope requerido: `interpay`
- Rol específico decodificado de token (verificación adicional)

---

## 📡 Endpoints y Servicios Expuestos / Consumidos

### 📊 Tabla General de TODOS los Endpoints (AUDITORÍA 100% - Cero Omisiones)

| # | Método | Ruta Completa | Controlador | Descripción Funcional | Autorización | Request | Response (200 OK) |
| :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| 1 | `POST` | `/ms-conciliaciones/api/v1/Conciliaciones/crear-parametrizacion` | `ConciliacionesController` | Crear nueva parametrización de regla de negocio | Bearer JWT (Scope: `interpay`) | `RuleParameterizationCommand` | `Answers<RuleParameterizationResponse>` |
| 2 | `GET` | `/ms-conciliaciones/api/v1/Conciliaciones/obtener-reglas/{idSubProcess}` | `ConciliacionesController` | Obtener reglas paginadas por ID de subproceso | Bearer JWT (Scope: `interpay`) | Query Params: `pageNumber`, `pageSize` | `Answers<GetRulesByIdSpResponse>` |
| 3 | `GET` | `/ms-conciliaciones/api/v1/Conciliaciones/obtener-detalle-regla/{id_conciliacion}/{rule_name}` | `ConciliacionesController` | Obtener detalle de conciliación específica por regla | Bearer JWT (Scope: `interpay`) | Path: `id_conciliacion`, `rule_name` | `Answers<GetConciliationDetailsResponse>` |
| 4 | `GET` | `/ms-conciliaciones/api/v1/Conciliaciones/obtener-reglas-subproceso/{id_subprocess}/{rule_name}` | `ConciliacionesController` | Obtener reglas detalladaspaginadas por subproceso y nombre | Bearer JWT (Scope: `interpay`) | Path + Query: `pageNumber`, `pageSize` | `Answers<GetRuleDetailsPagedResposne>` |
| 5 | `GET` | `/ms-conciliaciones/api/v1/HealthCheck/HealthCheck` | `HealthCheckController` | Estado operativo simple del servicio | AllowAnonymous | N/A | `"Funcionando."` (200 OK) |
| 6 | `GET` | `/ms-conciliaciones/api/v1/HealthCheck/health/ready` | `HealthCheckController` | Health Check liveness + readiness (K8s/Containers) | AllowAnonymous | N/A | `{"status":"Healthy","checks":[...]}` |
| 7 | `GET` | `/ms-conciliaciones/api/v1/HealthCheck/health/live` | `HealthCheckController` | Estado de salud aplicación (liveness probe) | AllowAnonymous | N/A | `"Alive"` (200 OK) |
| 8 | `GET` | `/ms-conciliaciones/api/v1/TestConnection/check-mongo` | `TestConnectionController` | Validar conexión a MongoDB/DocumentDB + listar colecciones | AllowAnonymous | N/A | `{"Status":"...","PingResult":"...","ExistingCollections":[...]}` |
| 9 | `GET` | `/ms-conciliaciones/api/v1/TestConnection/check-databases` | `TestConnectionController` | Listar todas las bases de datos disponibles en servidor MongoDB | AllowAnonymous | N/A | `{"BasesDeDatosDisponibles":[...],"BaseDeDatosActual":"..."}` |

---

## 📑 Detalle Técnico Completo de TODOS los Endpoints (cURL, Requests y Responses JSON)

---

### **Endpoint 1: Crear Parametrización de Regla** (POST)

- **Método / Ruta:** `POST` `/ms-conciliaciones/api/v1/Conciliaciones/crear-parametrizacion`
- **Controlador:** `ConciliacionesController` ([Línea 22-27](Conciliaciones.Api/Controllers/v1/ConciliacionesController.cs))
- **Autorización:** `Bearer JWT` con Scope `interpay` (Keycloak)
- **Descripción:** Crea una nueva parametrización de regla de negocio envolviendo fuentes de datos, reglas asociadas y campos de conciliación. Genera alias únicos consecutivos y persiste en MongoDB/DocumentDB.
- **Request de Ejemplo (JSON Body):**

```json
{
  "creacionCliente": {
    "nombreRegla": "Reconciliación Pagos Interpay",
    "descripcion": "Regla para conciliar pagos de Interpay con servidor",
    "estado": "Activa",
    "idProceso": "PROC_001",
    "idSubProceso": "SUBPROC_002"
  },
  "fuentesDeDatos": [
    {
      "idOrigen": "SOURCE_1",
      "nombreOrigen": "Sistema ERP",
      "tipoConexion": "SQL_SERVER",
      "orden": 1
    },
    {
      "idOrigen": "SOURCE_2",
      "nombreOrigen": "API Interpay",
      "tipoConexion": "REST_API",
      "orden": 2
    }
  ],
  "reglasAsociadas": [
    {
      "idRegla": "RULE_001",
      "nombreRegla": "Validación Monto",
      "condicion": "monto_origen = monto_destino",
      "accion": "VALIDAR"
    }
  ],
  "camposConciliacion": [
    {
      "idCampo": "FIELD_001",
      "nombreCampo": "referencia_transaccion",
      "tipoDato": "STRING",
      "validacion": "NOT_NULL"
    }
  ]
}
```

- **Response de Ejemplo (200 OK - JSON):**

```json
{
  "codigoEstado": 200,
  "respuestaEstado": true,
  "mensaje": "Parametrización creada con alias consecutivos generados exitosamente.",
  "errores": [],
  "mensajeError": null,
  "resultado": {
    "id": "630f1a2c8f1e2e3e4e5e6e7e",
    "idSchema": "SCHEMA_630f1a2c8f1e2e3e4e5e6e7e",
    "message": "Se creó la parametrización con ID 630f1a2c8f1e2e3e4e5e6e7e",
    "fechaCreacion": "2025-01-15T14:32:55.123Z"
  }
}
```

- **Comando cURL de Consumo:**

```bash
curl -X POST "https://localhost:7126/ms-conciliaciones/api/v1/Conciliaciones/crear-parametrizacion" \
  -H "Accept: application/json" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -d '{
    "creacionCliente": {"nombreRegla":"Test","descripcion":"Test","estado":"Activa","idProceso":"01","idSubProceso":"02"},
    "fuentesDeDatos": [{"idOrigen":"S1","nombreOrigen":"ERP","tipoConexion":"SQL","orden":1}],
    "reglasAsociadas": [{"idRegla":"R1","nombreRegla":"Val","condicion":"a=b","accion":"VALIDAR"}],
    "camposConciliacion": [{"idCampo":"F1","nombreCampo":"ref","tipoDato":"STRING","validacion":"NOT_NULL"}]
  }'
```

---

### **Endpoint 2: Obtener Reglas por ID Subproceso (Paginado)** (GET)

- **Método / Ruta:** `GET` `/ms-conciliaciones/api/v1/Conciliaciones/obtener-reglas/{idSubProcess}?pageNumber=1&pageSize=10`
- **Controlador:** `ConciliacionesController` ([Línea 30-42](Conciliaciones.Api/Controllers/v1/ConciliacionesController.cs))
- **Autorización:** `Bearer JWT` con Scope `interpay`
- **Descripción:** Recupera lista paginada de reglas activas asociadas a un subproceso específico desde MongoDB/DocumentDB. Utiliza builder de queries especializados.
- **Request de Ejemplo (Path + Query Parameters):**

```
GET /ms-conciliaciones/api/v1/Conciliaciones/obtener-reglas/SUBPROC_002?pageNumber=1&pageSize=10
Path Parameters:
  - idSubProcess: "SUBPROC_002"
Query Parameters:
  - pageNumber: 1 (default)
  - pageSize: 10 (default)
```

- **Response de Ejemplo (200 OK - JSON):**

```json
{
  "codigoEstado": 200,
  "respuestaEstado": true,
  "mensaje": null,
  "errores": [],
  "mensajeError": null,
  "resultado": {
    "items": [
      {
        "id": "630f1a2c8f1e2e3e4e5e6e7e",
        "idSubProcess": "SUBPROC_002",
        "nombreRegla": "Reconciliación Pagos Interpay",
        "descripcion": "Regla para conciliar pagos",
        "estado": "Activa",
        "fechaCreacion": "2025-01-10T10:00:00Z"
      },
      {
        "id": "630f1a2c8f1e2e3e4e5e6e8f",
        "idSubProcess": "SUBPROC_002",
        "nombreRegla": "Validación Comisiones",
        "descripcion": "Validar comisiones por transacción",
        "estado": "Activa",
        "fechaCreacion": "2025-01-12T14:30:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 2,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

- **Comando cURL de Consumo:**

```bash
curl -X GET "https://localhost:7126/ms-conciliaciones/api/v1/Conciliaciones/obtener-reglas/SUBPROC_002?pageNumber=1&pageSize=10" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

### **Endpoint 3: Obtener Detalle de Conciliación** (GET)

- **Método / Ruta:** `GET` `/ms-conciliaciones/api/v1/Conciliaciones/obtener-detalle-regla/{id_conciliacion}/{rule_name}`
- **Controlador:** `ConciliacionesController` ([Línea 44-53](Conciliaciones.Api/Controllers/v1/ConciliacionesController.cs))
- **Autorización:** `Bearer JWT` con Scope `interpay`
- **Descripción:** Obtiene el detalle completo de una conciliación específica identificada por ID de conciliación y nombre de regla. Incluye resultados de validación y discrepancias encontradas.
- **Request de Ejemplo (Path Parameters):**

```
GET /ms-conciliaciones/api/v1/Conciliaciones/obtener-detalle-regla/CONC_2025_001/Reconciliación-Pagos
Path Parameters:
  - id_conciliacion: "CONC_2025_001"
  - rule_name: "Reconciliación-Pagos"
```

- **Response de Ejemplo (200 OK - JSON):**

```json
{
  "codigoEstado": 200,
  "respuestaEstado": true,
  "mensaje": null,
  "errores": [],
  "mensajeError": null,
  "resultado": {
    "dataConciliacion": [
      {
        "id": "630f1a2c8f1e2e3e4e5e6e7e",
        "idConciliacion": "CONC_2025_001",
        "nombreRegla": "Reconciliación-Pagos",
        "registrosOrigen": 1500,
        "registrosDestino": 1478,
        "registrosConciliados": 1475,
        "registrosDiscrepancia": 3,
        "discrepanciasDetalle": [
          {
            "idTransaccion": "TXN_00001",
            "montoOrigen": 100.50,
            "montoDestino": 100.45,
            "diferencia": 0.05,
            "motivo": "Redondeo decimal"
          }
        ],
        "porcentajeExito": 99.80,
        "fechaEjecucion": "2025-01-15T08:30:00Z",
        "estado": "COMPLETADA_CON_DISCREPANCIAS"
      }
    ]
  }
}
```

- **Comando cURL de Consumo:**

```bash
curl -X GET "https://localhost:7126/ms-conciliaciones/api/v1/Conciliaciones/obtener-detalle-regla/CONC_2025_001/Reconciliaci%C3%B3n-Pagos" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

### **Endpoint 4: Obtener Reglas Subproceso Detalladass (Paginado)** (GET)

- **Método / Ruta:** `GET` `/ms-conciliaciones/api/v1/Conciliaciones/obtener-reglas-subproceso/{id_subprocess}/{rule_name}?pageNumber=1&pageSize=10`
- **Controlador:** `ConciliacionesController` ([Línea 55-69](Conciliaciones.Api/Controllers/v1/ConciliacionesController.cs))
- **Autorización:** `Bearer JWT` con Scope `interpay`
- **Descripción:** Recupera lista detallada y paginada de detalles de reglas filtradas por subproceso e identificador de regla. Información granular para auditoría.
- **Request de Ejemplo (Path + Query Parameters):**

```
GET /ms-conciliaciones/api/v1/Conciliaciones/obtener-reglas-subproceso/SUBPROC_002/Validación-Monto?pageNumber=1&pageSize=5
Path Parameters:
  - id_subprocess: "SUBPROC_002"
  - rule_name: "Validación-Monto"
Query Parameters:
  - pageNumber: 1
  - pageSize: 5
```

- **Response de Ejemplo (200 OK - JSON):**

```json
{
  "codigoEstado": 200,
  "respuestaEstado": true,
  "mensaje": null,
  "errores": [],
  "mensajeError": null,
  "resultado": {
    "items": [
      {
        "id": "630f1a2c8f1e2e3e4e5e6e7e",
        "idSubProcess": "SUBPROC_002",
        "nombreRegla": "Validación-Monto",
        "condicion": "monto_origen = monto_destino",
        "accion": "VALIDAR",
        "prioridad": 1,
        "activa": true,
        "fechaCreacion": "2025-01-05T09:15:00Z",
        "fechaActualizacion": "2025-01-15T11:22:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 5,
    "totalRecords": 1,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

- **Comando cURL de Consumo:**

```bash
curl -X GET "https://localhost:7126/ms-conciliaciones/api/v1/Conciliaciones/obtener-reglas-subproceso/SUBPROC_002/Validaci%C3%B3n-Monto?pageNumber=1&pageSize=5" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

### **Endpoint 5: Health Check - Simple** (GET)

- **Método / Ruta:** `GET` `/ms-conciliaciones/api/v1/HealthCheck/HealthCheck`
- **Controlador:** `HealthCheckController` ([Línea 23-28](Conciliaciones.Api/Controllers/v1/HealthCheckController.cs))
- **Autorización:** `AllowAnonymous` (Sin requiere autenticación)
- **Descripción:** Endpoint ligero para verificación rápida del estado operativo del servicio. Utilizado por load balancers y monitoreo.
- **Request de Ejemplo:**

```
GET /ms-conciliaciones/api/v1/HealthCheck/HealthCheck
(Sin parámetros)
```

- **Response de Ejemplo (200 OK - Texto):**

```
Funcionando.
```

- **Comando cURL de Consumo:**

```bash
curl -X GET "https://localhost:7126/ms-conciliaciones/api/v1/HealthCheck/HealthCheck" \
  -H "Accept: application/json"
```

---

### **Endpoint 6: Health Check - Ready (Kubernetes/Containers)** (GET)

- **Método / Ruta:** `GET` `/ms-conciliaciones/api/v1/HealthCheck/health/ready`
- **Controlador:** `HealthCheckController` ([Línea 30-44](Conciliaciones.Api/Controllers/v1/HealthCheckController.cs))
- **Autorización:** `AllowAnonymous`
- **Descripción:** Health check completo con evaluación de dependencias marcadas con tag `ready` (BD, caché, servicios externos). Retorna 503 si alguna dependencia no está disponible. Usado por orquestadores de contenedores (Kubernetes readiness probe).
- **Request de Ejemplo:**

```
GET /ms-conciliaciones/api/v1/HealthCheck/health/ready
(Sin parámetros)
```

- **Response de Ejemplo (200 OK - JSON):**

```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "mongodb",
      "status": "Healthy",
      "description": "MongoDB DocumentDB connection available"
    },
    {
      "name": "keycloak",
      "status": "Healthy",
      "description": "Keycloak identity provider available"
    }
  ]
}
```

**Response en caso de fallo (503 Service Unavailable):**

```json
{
  "status": "Unhealthy",
  "checks": [
    {
      "name": "mongodb",
      "status": "Unhealthy",
      "description": "MongoDB connection timeout"
    }
  ]
}
```

- **Comando cURL de Consumo:**

```bash
curl -X GET "https://localhost:7126/ms-conciliaciones/api/v1/HealthCheck/health/ready" \
  -H "Accept: application/json"
```

---

### **Endpoint 7: Health Check - Live (Liveness Probe)** (GET)

- **Método / Ruta:** `GET` `/ms-conciliaciones/api/v1/HealthCheck/health/live`
- **Controlador:** `HealthCheckController` ([Línea 45-49](Conciliaciones.Api/Controllers/v1/HealthCheckController.cs))
- **Autorización:** `AllowAnonymous`
- **Descripción:** Verificación ultra-ligera de liveness (¿está corriendo la aplicación?). Retorna `200 OK` si la aplicación está activa. Usado por Kubernetes liveness probe para reiniciar contenedores muertos.
- **Request de Ejemplo:**

```
GET /ms-conciliaciones/api/v1/HealthCheck/health/live
(Sin parámetros)
```

- **Response de Ejemplo (200 OK - Texto):**

```
Alive
```

- **Comando cURL de Consumo:**

```bash
curl -X GET "https://localhost:7126/ms-conciliaciones/api/v1/HealthCheck/health/live" \
  -H "Accept: application/json"
```

---

### **Endpoint 8: Verificar Conexión MongoDB/DocumentDB** (GET)

- **Método / Ruta:** `GET` `/ms-conciliaciones/api/v1/TestConnection/check-mongo`
- **Controlador:** `TestConnectionController` ([Línea 18-37](Conciliaciones.Api/Controllers/v1/TestConnectionController.cs))
- **Autorización:** `AllowAnonymous`
- **Descripción:** Valida la conexión actual a MongoDB/DocumentDB ejecutando comando PING y listando colecciones existentes. Útil para diagnosticar problemas de conectividad.
- **Request de Ejemplo:**

```
GET /ms-conciliaciones/api/v1/TestConnection/check-mongo
(Sin parámetros)
```

- **Response de Ejemplo (200 OK - JSON):**

```json
{
  "Status": "Conectado exitosamente a MongoDB/DocumentDB",
  "PingResult": "{ \"ok\" : 1 }",
  "DatabaseName": "conciliaciones_db",
  "ExistingCollections": [
    "rule_parameterizations",
    "reconciliation_results",
    "rule_concepts",
    "audit_log"
  ]
}
```

**Response en caso de error de conexión (500 Internal Server Error):**

```json
{
  "Status": "Error de conexión con MongoDB",
  "ErrorMessage": "No se puede conectar al host especificado",
  "InnerException": "Connection refused (127.0.0.1:27017)"
}
```

- **Comando cURL de Consumo:**

```bash
curl -X GET "https://localhost:7126/ms-conciliaciones/api/v1/TestConnection/check-mongo" \
  -H "Accept: application/json"
```

---

### **Endpoint 9: Listar Bases de Datos Disponibles** (GET)

- **Método / Ruta:** `GET` `/ms-conciliaciones/api/v1/TestConnection/check-databases`
- **Controlador:** `TestConnectionController` ([Línea 39-60](Conciliaciones.Api/Controllers/v1/TestConnectionController.cs))
- **Autorización:** `AllowAnonymous`
- **Descripción:** Lista todas las bases de datos disponibles en el servidor MongoDB/DocumentDB e identifica la base de datos configurada actualmente en la aplicación.
- **Request de Ejemplo:**

```
GET /ms-conciliaciones/api/v1/TestConnection/check-databases
(Sin parámetros)
```

- **Response de Ejemplo (200 OK - JSON):**

```json
{
  "BasesDeDatosDisponiblesEnServidor": [
    "admin",
    "conciliaciones_db",
    "conciliaciones_staging",
    "conciliaciones_prod",
    "local",
    "config"
  ],
  "BaseDeDatosActualEnConfig": "conciliaciones_db"
}
```

**Response en caso de error de permisos (500 Internal Server Error):**

```json
{
  "Status": "Error al listar las bases de datos (Posible falta de permisos del usuario)",
  "ErrorMessage": "not authorized to execute command",
  "InnerException": "Authentication failed"
}
```

- **Comando cURL de Consumo:**

```bash
curl -X GET "https://localhost:7126/ms-conciliaciones/api/v1/TestConnection/check-databases" \
  -H "Accept: application/json"
```

---

## 🧪 Pruebas (Testing)

### Ejecutar Suite Completa de Pruebas Unitarias + Cobertura

```bash
# Ejecutar todas las pruebas
dotnet test DOTNET_Servicios_Hex_Conciliaciones.slnx

# Ejecutar con reporte de cobertura XPlat Code Coverage
dotnet test DOTNET_Servicios_Hex_Conciliaciones.slnx /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Ejecutar pruebas específicas de un proyecto
dotnet test Conciliaciones.Infraestructura/Conciliaciones.Infraestructura.csproj -v normal

# Ejecutar con filtro de método de prueba
dotnet test --filter "ClassName=Conciliaciones.Infraestructura.Tests.RepositoryTests"
```

### Estrategia de Testing

| Capa | Tipo de Pruebas | Herramienta | Cobertura Objetivo |
| --- | --- | --- | --- |
| **API (Controllers)** | Integration Tests | xUnit + Moq | 80%+ |
| **Application (Handlers/Commands/Queries)** | Unit Tests | xUnit + TestFixture | 85%+ |
| **Domain (Entities/Validations)** | Unit Tests | xUnit | 90%+ |
| **Infrastructure (Repositories/DB)** | Integration Tests | xUnit + TestContainer (MongoDB) | 75%+ |

---

## 📦 Despliegue y CI/CD

### Estrategia de Despliegue

**Pipeline Automatizado:** Azure DevOps Pipelines (`azure-pipelines.yml`)

**Flujo de Despliegue:**
```
Code Push → Build → Unit Tests → Code Analysis (SonarCloud) 
→ Docker Build → Push to Registry → Deploy to Staging 
→ Integration Tests → Deploy to Production
```

### Comando de Empaquetado / Build para Producción

```bash
# Build Release
dotnet build -c Release DOTNET_Servicios_Hex_Conciliaciones.slnx

# Publicar como Self-Contained Executable
dotnet publish -c Release -o ./publish/ --self-contained

# Build Docker Image
docker build -f Dockerfile -t conciliaciones-api:1.0.0 .

# Push a registry (Azure Container Registry / Docker Hub)
docker tag conciliaciones-api:1.0.0 myregistry.azurecr.io/conciliaciones-api:1.0.0
docker push myregistry.azurecr.io/conciliaciones-api:1.0.0
```

### Despliegue en Kubernetes

```yaml
# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: conciliaciones-api
  labels:
    app: conciliaciones
spec:
  replicas: 3
  selector:
    matchLabels:
      app: conciliaciones
  template:
    metadata:
      labels:
        app: conciliaciones
    spec:
      containers:
      - name: conciliaciones-api
        image: myregistry.azurecr.io/conciliaciones-api:1.0.0
        ports:
        - containerPort: 80
          name: http
        - containerPort: 443
          name: https
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: KEYCLOAK_AUTHORITY
          valueFrom:
            secretKeyRef:
              name: keycloak-secrets
              key: authority
        livenessProbe:
          httpGet:
            path: /ms-conciliaciones/api/v1/HealthCheck/health/live
            port: 80
          initialDelaySeconds: 10
          periodSeconds: 15
        readinessProbe:
          httpGet:
            path: /ms-conciliaciones/api/v1/HealthCheck/health/ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 10
```

---

## 🛠️ Dependencias y Librerías Clave (AUDITORÍA 100% COMPLETA - CERO OMISIONES)

| # | Paquete / Librería | Versión | Capa | Propósito Detectado en el Código |
| --- | --- | --- | --- | --- |
| **API Layer** |  |  |  |  |
| 1 | `DotNetEnv` | 3.1.1 | Api | Cargar variables de entorno desde archivo `.env` en startup |
| 2 | `IntegracionSeguridadKeycloak.Core` | 2.0.4 | Api | Integración de autenticación/autorización con Keycloak (OpenID Connect, JWT) |
| 3 | `MediatR` | 14.1.0 | Api + Aplicación | Implementar patrón CQRS, mediador entre controllers y handlers |
| 4 | `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.25 | Api | Validación de Bearer tokens JWT en middlewares |
| 5 | `Microsoft.Extensions.Diagnostics.HealthChecks` | 10.0.5 | Api | Implementar endpoints de health check (ready, live, liveness probes) |
| 6 | `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` | 1.21.2 | Api | Tooling para debug de Docker en Visual Studio |
| 7 | `Swashbuckle.AspNetCore` | 6.6.2 | Api | Generación de Swagger/OpenAPI documentación interactiva |
| **Application Layer** |  |  |  |  |
| 8 | `FluentValidation.DependencyInjectionExtensions` | 12.1.1 | Aplicación | Inyección de validadores FluentValidation en DI container |
| 9 | `Microsoft.Extensions.Configuration` | 8.0.0 | Aplicación | Abstracción de configuración para acceso a settings |
| **Infrastructure Layer** |  |  |  |  |
| 10 | `Ardalis.Specification.EntityFrameworkCore` | 9.1.0 | Infraestructura | Pattern de Specification para queries complejas con EF Core |
| 11 | `AutoMapper` | 16.1.1 | Infraestructura | Mapeo automático entre DTOs y entities (DTO→Entity, Entity→DTO) |
| 12 | `AWSSDK.SimpleSystemsManagement` | 4.0.103.8 | Infraestructura | Cliente AWS Systems Manager Parameter Store (gestión de secretos en Prod) |
| 13 | `Dapper` | 2.1.66 | Infraestructura | Micro-ORM para queries SQL optimizadas (fallback alternativo a EF Core) |
| 14 | `FluentValidation` | 12.1.1 | Infraestructura | Validación fluida de entities, DTOs y requests (pipeline behavior) |
| 15 | `Microsoft.EntityFrameworkCore` | 9.0.5 | Infraestructura | ORM completo (fallback para SQL Server cuando MongoDB no disponible) |
| 16 | `Microsoft.Extensions.Configuration.Abstractions` | 9.0.5 | Infraestructura | Abstracciones IConfiguration para acceso a configuración |
| 17 | `Microsoft.Extensions.DependencyInjection` | 9.0.5 | Infraestructura | Inyección de dependencias (DI container) |
| 18 | `MongoDB.Driver` | 3.11.2 | Infraestructura | Driver oficial MongoDB/DocumentDB para conexión y queries |
| **Domain Layer** |  |  |  |  |
| 19 | `Ardalis.GuardClauses` | 4.6.0 | Dominio | Guard clauses para validaciones rápidas en entities (Guard.Against.Null) |
| 20 | `Ardalis.Specification` | 9.1.0 | Dominio | Patrón Specification para encapsular criteria de queries |
| 21 | `FluentValidation` | 12.1.1 | Dominio | Validación de reglas de negocio en entities y DTOs |
| 22 | `MongoDB.Bson` | 3.11.2 | Dominio | Serialización/deserialización BSON para documentos MongoDB |

### Análisis de Dependencias por Funcionalidad

| Funcionalidad | Paquetes Involucrados |
| --- | --- |
| **CQRS + Mediador** | MediatR (14.1.0), FluentValidation (12.1.1) |
| **Autenticación/Autorización** | IntegracionSeguridadKeycloak.Core (2.0.4), Microsoft.AspNetCore.Authentication.JwtBearer (8.0.25) |
| **Data Access (NoSQL MongoDB)** | MongoDB.Driver (3.11.2), MongoDB.Bson (3.11.2), AutoMapper (16.1.1) |
| **Data Access (SQL Fallback)** | Microsoft.EntityFrameworkCore (9.0.5), Dapper (2.1.66) |
| **Especificación Queries** | Ardalis.Specification (9.1.0), Ardalis.Specification.EntityFrameworkCore (9.1.0) |
| **Validación** | FluentValidation (12.1.1), Ardalis.GuardClauses (4.6.0) |
| **Configuración & Secretos** | DotNetEnv (3.1.1), Microsoft.Extensions.Configuration (8.0.0), AWSSDK.SimpleSystemsManagement (4.0.103.8) |
| **Health Checks** | Microsoft.Extensions.Diagnostics.HealthChecks (10.0.5) |
| **API Documentation** | Swashbuckle.AspNetCore (6.6.2) |
| **Docker Support** | Microsoft.VisualStudio.Azure.Containers.Tools.Targets (1.21.2) |
| **Inyección de Dependencias** | Microsoft.Extensions.DependencyInjection (9.0.5) |

### Compatibilidad de Versiones

- **.NET Target Framework:** `net8.0` (LTS - Long Term Support hasta Nov 2026)
- **C# Version:** 12.0 (compatible con .NET 8)
- **MediatR:** 14.1.0 (última versión estable, compatible con .NET 8)
- **Entity Framework Core:** 9.0.5 (versión más reciente para .NET 8)
- **AutoMapper:** 16.1.1 (versión actual, soporta .NET 8)

---

## ⚠️ Solución de Problemas Frecuentes (Troubleshooting)

| Problema | Causa Raíz | Solución |
| --- | --- | --- |
| `MongoException: cannot connect to server` | MongoDB no levantado o puerto 27017 incorrecto | `docker-compose up -d` o verificar `Connection_String` en `.env` |
| `AuthenticationException: BSON credentials not valid` | Usuario/contraseña MongoDB incorrectos en `.env` | Verificar `PROVIDER`, `CONNECTION_STRING`, `DATABASE` contra credenciales reales |
| `JwtSecurityTokenInvalidSignatureException` | Keycloak token expirado o realm incorrecto | Renovar token en Keycloak o verificar `KEYCLOAK_AUTHORITY` en `.env` |
| `FluentValidationException: validation failed` | Dto request no cumple reglas de validación | Revisar modelo de entrada y validador en `RuleParameterizationValidator` |
| `401 Unauthorized - Missing or invalid JWT` | Bearer token ausente o malformado en header | Incluir header: `Authorization: Bearer <token>` y verificar scope `interpay` |
| `403 Forbidden - Insufficient permissions` | Rol JWT no tiene permiso para endpoint | Verificar rol en token decodificado vs `REQUIRED_ROLE` en `.env` |
| `400 BadRequest - Deserialization error` | JSON body malformado o tipos incorrectos | Validar JSON contra estructura de `RuleParameterizationCommand` |
| `503 ServiceUnavailable - Health Ready` | Dependencia (BD, cache) no disponible | Ejecutar `/health/ready` para identificar dependencia fallida |
| `Docker: Error response from daemon: port 27017 already in use` | Otro contenedor MongoDB escuchando puerto 27017 | `docker-compose down` o cambiar puerto en `docker-compose.yml` |
| `dotnet run: No such file or directory` | Proyecto no encontrado en ruta especificada | Verificar ruta relativa y que `.csproj` exista |
| `Rfc2898DeriveBytes: invalid Base64 string` | `LLAVE_CIFRADA` o `PDB` no son Base64 válido | Validar que variables en `.env` sean Base64 encodings válidos |
| `DbContext: No suitable provider found` | EF Core proveedor SQL Server no instalado | `dotnet add package Microsoft.EntityFrameworkCore.SqlServer` si se usa SQL |

---

## 📞 Contacto y Soporte

- **Equipo Propietario:** Célula de Servicios Hex / Equipo de Interrapidísimo
- **Repositorio:** [Azure DevOps - DOTNET_Servicio_Hex_Conciliaciones](https://dev.azure.com/interrapidisimo/Servicios/_git/DOTNET_Servicio_Hex_Conciliaciones)
- **Rama de Desarrollo:** `Desarrollo_Feature_Interpay_Conciliaciones_Dev1`
- **Soporte:** Abrir un Issue/Work Item en Azure DevOps adjuntando:
  - Stack trace o logs de error
  - Versión .NET y SO
  - Pasos para reproducir
  - Output de `/health/ready` endpoint
- **Documentación Adicional:** Consultar wikis en Azure DevOps Servicios
- **Escalación:** Contactar al Tech Lead de infraestructura para problemas de CI/CD o despliegue

---

## 🎓 Consultas y Recursos de Aprendizaje

- **Patrón Hexagonal (Ports & Adapters):** https://almanac.io/docs/hexagonal-architecture
- **CQRS Pattern:** https://martinfowler.com/bliki/CQRS.html
- **MediatR Documentation:** https://github.com/jbogard/MediatR
- **MongoDB C# Driver:** https://www.mongodb.com/docs/drivers/csharp/
- **Entity Framework Core:** https://learn.microsoft.com/en-us/ef/core/
- **Keycloak OIDC/OAuth2:** https://www.keycloak.org/docs
- **.NET 8 Documentation:** https://learn.microsoft.com/en-us/dotnet/

---

## 📄 Licencia

Copyright © Interrapidísimo.  
Todos los Derechos Reservados bajo Apache License 2.0.  
Ver archivo `License.txt` en la raíz del proyecto.

---

**Última Actualización:** 15 de Enero de 2025  
**Versión del README:** 1.0.0  
**Generado por:** Tech Lead - Arquitecto de Software Experto (.NET 8 | Hexagonal Architecture | CQRS)