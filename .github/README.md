# GitHub Actions Workflows

This directory contains CI/CD workflows for the Veritheia project.

## Workflows

### test.yml - Quick Test Runner
- **Trigger**: Push/PR to main, master, develop branches
- **Purpose**: Fast feedback on test status
- **Features**:
  - Runs all tests using Testcontainers
  - Generates test reports
  - Uploads test results as artifacts

### ci.yml - Complete CI/CD Pipeline
- **Trigger**: Push/PR to main, master branches, and version tags
- **Purpose**: Full validation and release pipeline
- **Features**:
  - Multi-OS testing (Ubuntu, Windows, macOS)
  - Code quality checks
  - Test coverage reporting
  - Docker image building
  - Automated releases for version tags

## Test Infrastructure

### Why Testcontainers?
Our tests use Testcontainers to spin up PostgreSQL with pgvector automatically:
- **No configuration needed** - Works identically locally and in CI
- **Isolation** - Each test run gets a fresh database
- **Real PostgreSQL** - Tests run against actual PostgreSQL 17 with pgvector
- **Fast cleanup** - Respawn resets database between tests (~50ms)

### SSL Certificate Handling
WebTests are configured to accept any certificate in test environments:
```csharp
ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
```
This ensures tests work reliably across all CI environments without certificate setup.

## Required Secrets
None required for basic CI. For release automation, you may want to add:
- `DOCKER_USERNAME` and `DOCKER_PASSWORD` for Docker Hub publishing
- `NUGET_API_KEY` for NuGet package publishing

## Running Locally
To run the same tests locally:
```bash
dotnet test --configuration Release --logger "trx"
```

## Troubleshooting

### Tests fail with "Docker not found"
Ensure Docker is installed and running. GitHub Actions runners have Docker pre-installed.

### PostgreSQL connection errors
Testcontainers handles all PostgreSQL setup. If issues persist:
1. Check Docker is running
2. Ensure no firewall blocking Docker containers
3. Set `TESTCONTAINERS_RYUK_DISABLED=true` for corporate environments

### SSL certificate errors
Already handled in WebTests.cs with certificate validation bypass for tests.

## Best Practices
1. **All tests must be idempotent** - Respawn ensures clean state
2. **Use Testcontainers for databases** - Ensures reproducibility
3. **Mock external services** - Don't depend on external APIs in tests
4. **Keep tests fast** - Parallel execution where possible