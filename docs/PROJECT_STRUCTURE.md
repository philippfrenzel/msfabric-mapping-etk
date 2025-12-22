# Project Structure

This document describes the organization of the Fabric Reference Table & Data Mapping Service repository.

## Overview

This project follows Microsoft Fabric Extensibility Toolkit design principles, organized into clear functional areas for development, deployment, and documentation.

## Directory Structure

```
msfabric-mapping-etk/
├── .github/                          # GitHub configuration
│   └── agents/                       # Custom GitHub Copilot agents
├── docs/                             # Documentation
│   ├── API.md                        # API endpoint documentation
│   └── FABRIC-INTEGRATION.md         # Fabric integration guide
├── fabric-manifest/                  # Fabric workload manifest
│   └── workload-manifest.json        # Workload definition for Fabric
├── scripts/                          # Automation scripts
│   ├── Setup/                        # Setup and configuration scripts
│   ├── Run/                          # Scripts to run development environment
│   ├── Build/                        # Build automation scripts
│   └── Deploy/                       # Deployment scripts
├── src/                              # Source code
│   ├── FabricMappingService.Api/     # REST API (Backend)
│   │   ├── Controllers/              # API controllers
│   │   ├── Dtos/                     # Data transfer objects
│   │   ├── Program.cs                # Application entry point
│   │   └── appsettings.json          # Configuration
│   └── FabricMappingService.Core/    # Core library
│       ├── Attributes/               # Custom mapping attributes
│       ├── Converters/               # Type converters
│       ├── Exceptions/               # Custom exceptions
│       ├── Models/                   # Domain models
│       ├── Services/                 # Business logic
│       └── Examples/                 # Example models
├── tests/                            # Test projects
│   └── FabricMappingService.Tests/   # Unit tests
├── CODE_OF_CONDUCT.md                # Community code of conduct
├── SECURITY.md                       # Security policy
├── SUPPORT.md                        # Support information
├── README.md                         # Project overview
├── SUMMARY.md                        # Project summary
└── FabricMappingService.sln          # Solution file

```

## Project Components

### Source Code (`src/`)

#### FabricMappingService.Core
The core library containing the business logic and domain models:

- **Attributes/**: Custom attributes for declarative mapping configuration
  - `MapToAttribute`: Maps source property to target property
  - `MappingProfileAttribute`: Class-level mapping configuration
  - `IgnoreMappingAttribute`: Excludes properties from mapping

- **Converters/**: Type conversion implementations
  - `IPropertyConverter`: Interface for custom converters
  - `DefaultPropertyConverter`: Built-in type conversions

- **Services/**: Business logic implementation
  - `AttributeMappingService`: Attribute-based mapping service
  - `MappingIO`: Reference table (KeyMapping) operations

- **Models/**: Configuration and domain models
  - `MappingConfiguration`: Service configuration
  - `MappingResult`: Operation results with metadata
  - `ReferenceTableColumn`: Reference table schema definition

- **Exceptions/**: Custom exception types
  - `MappingException`: Base mapping exception

#### FabricMappingService.Api
ASP.NET Core Web API providing REST endpoints:

- **Controllers/**: API endpoint implementations
  - `ReferenceTableController`: Reference table CRUD operations
  - `MappingController`: Attribute mapping operations

- **Dtos/**: Request/response data transfer objects
  - Separated from domain models for API versioning

### Tests (`tests/`)

Unit tests using xUnit framework:

- `AttributeMappingServiceTests`: Tests for attribute-based mapping
- `DefaultPropertyConverterTests`: Tests for type conversion
- `MappingIOTests`: Tests for reference table operations

### Documentation (`docs/`)

Comprehensive documentation:

- **API.md**: REST API endpoint reference
- **FABRIC-INTEGRATION.md**: Integration with Microsoft Fabric
- **PROJECT_SETUP.md**: Development environment setup guide (to be added)

### Scripts (`scripts/`)

Automation scripts for common tasks:

- **Setup/**: Environment setup and configuration
- **Run/**: Start development servers and services
- **Build/**: Build and packaging automation
- **Deploy/**: Deployment to Azure and Fabric

### Fabric Manifest (`fabric-manifest/`)

Microsoft Fabric workload definition:

- **workload-manifest.json**: Workload metadata, item types, and API endpoints

## Key Design Principles

### 1. Separation of Concerns
- Core business logic in `FabricMappingService.Core`
- REST API in `FabricMappingService.Api`
- Tests in separate project

### 2. Microsoft Fabric Integration
- Manifest-driven workload definition
- Support for KeyMapping outports (reference tables)
- OneLake integration for storage

### 3. Extensibility
- Custom attributes for declarative configuration
- Plugin architecture for custom converters
- Configurable mapping behavior

### 4. Developer Experience
- Comprehensive documentation
- Automation scripts for common tasks
- Example models and usage patterns
- Development environment setup guides

### 5. Production Ready
- Error handling and validation
- Structured logging
- Health check endpoints
- Security best practices

## Technology Stack

- **.NET 10.0**: Target framework
- **C# 14**: Programming language
- **ASP.NET Core**: Web API framework
- **xUnit**: Testing framework
- **Microsoft Fabric**: Platform integration

## Getting Started

1. Review [README.md](../README.md) for project overview
2. Follow [PROJECT_SETUP.md](PROJECT_SETUP.md) for environment setup
3. Explore [API.md](API.md) for API documentation
4. Read [FABRIC-INTEGRATION.md](FABRIC-INTEGRATION.md) for Fabric integration

## Naming Conventions

### Projects
- Format: `FabricMappingService.[ComponentName]`
- Examples: `FabricMappingService.Core`, `FabricMappingService.Api`

### Namespaces
- Follow project structure
- Example: `FabricMappingService.Core.Services`

### Files
- Pascal case for C# files: `AttributeMappingService.cs`
- Match type name: Class `AttributeMappingService` in `AttributeMappingService.cs`

### Tests
- Format: `[ClassName]Tests.cs`
- Test method format: `[Method]_[Scenario]_[Expected]`

## Build and Test

```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Run API locally
cd src/FabricMappingService.Api
dotnet run
```

## Contributing

When adding new features:

1. Add implementation in appropriate `src/` project
2. Add tests in `tests/FabricMappingService.Tests/`
3. Update relevant documentation in `docs/`
4. Update API manifest if adding new endpoints

## Additional Resources

- [Microsoft Fabric Documentation](https://learn.microsoft.com/fabric/)
- [Fabric Extensibility Toolkit](https://learn.microsoft.com/fabric/extensibility-toolkit/)
- [Microsoft Fabric Tools Workload Sample](https://github.com/microsoft/Microsoft-Fabric-tools-workload)
