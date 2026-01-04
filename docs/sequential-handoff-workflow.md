# Sequential Multi-Agent Workflow Documentation

## Overview

This repository implements a **sequential multi-agent workflow** using GitHub Actions. The workflow models a controlled, stage-gated development pipeline where each stage represents a specialized "agent" with specific responsibilities. All stages execute **strictly sequentially** with manual approval gates between them.

## Quick Links

- 🚀 **[Quick Start Guide](workflow-quick-start.md)** - Run the workflow for the first time
- 🔧 **[Environment Setup Guide](github-environments-setup.md)** - Configure approval gates
- 🤖 **[Integrating Custom Agents](integrating-custom-agents.md)** - Use GitHub Copilot agents with workflow stages
- 📖 **Complete documentation below** - Detailed reference

## Table of Contents

- [Workflow Architecture](#workflow-architecture)
- [Stage Definitions](#stage-definitions)
- [GitHub Environment Configuration](#github-environment-configuration)
- [Running the Workflow](#running-the-workflow)
- [Artifacts and Outputs](#artifacts-and-outputs)
- [Troubleshooting](#troubleshooting)

## Workflow Architecture

### Sequential Execution Model

```
┌─────────────────────────────────────────────────────────────────────┐
│                    Sequential Multi-Agent Pipeline                   │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
                    ┌───────────────────────────┐
                    │  Stage 1: Azure Architect │
                    │  (Architecture Validation)│
                    └───────────────────────────┘
                                    │
                                    ▼ [Manual Approval: handoff-azure-architect]
                    ┌───────────────────────────┐
                    │ Stage 2: .NET Senior Dev  │
                    │  (Build & Analysis)       │
                    └───────────────────────────┘
                                    │
                                    ▼ [Manual Approval: handoff-dotnet-senior]
                    ┌───────────────────────────┐
                    │ Stage 3: DevOps Engineer  │
                    │  (CI/CD & Artifacts)      │
                    └───────────────────────────┘
                                    │
                                    ▼ [Manual Approval: handoff-devops]
                    ┌───────────────────────────┐
                    │ Stage 4: Blazor/Fluent UI │
                    │  (Frontend Validation)    │
                    └───────────────────────────┘
                                    │
                                    ▼ [Manual Approval: handoff-blazor-fluentui]
                    ┌───────────────────────────┐
                    │  Stage 5: Test Specialist │
                    │  (Integration/E2E Tests)  │
                    └───────────────────────────┘
                                    │
                                    ▼ [Manual Approval: handoff-test-specialist]
                    ┌───────────────────────────┐
                    │ Stage 6: Unit Test Spec.  │
                    │  (Unit Tests + Coverage)  │
                    └───────────────────────────┘
                                    │
                                    ▼ [Manual Approval: handoff-unit-test-specialist]
                    ┌───────────────────────────┐
                    │  Stage 7: Orchestrator    │
                    │  (Final Coordination)     │
                    └───────────────────────────┘
                                    │
                                    ▼
                            ✅ Pipeline Complete
```

### Key Features

- **Strict Sequential Execution**: Each stage depends on the previous stage's successful completion
- **Manual Approval Gates**: Between each stage, a manual approval is required via GitHub Environments
- **Specialized Responsibilities**: Each stage focuses on a specific aspect of the development process
- **Artifact Passing**: Artifacts from earlier stages are available to later stages
- **Comprehensive Reporting**: Each stage generates detailed reports in GitHub Actions summaries

## Stage Definitions

### Stage 1: Azure Architect

**Responsibility**: Architecture validation and structure verification

**Actions**:
- Validates repository structure (src/, tests/, docs/)
- Checks for Fabric manifest files and validates JSON structure
- Verifies solution and project file existence
- Generates architecture report with project metrics

**Success Criteria**:
- Solution file exists
- Required directory structure is present
- Fabric manifest (if present) is valid JSON

**Handoff**: → `handoff-azure-architect` → .NET Senior Developer

---

### Stage 2: .NET Senior Developer

**Responsibility**: Build the solution and perform code quality analysis

**Actions**:
- Restores NuGet packages (with caching)
- Builds solution in Release configuration
- Runs code analyzers with high warning level
- Checks code formatting with `dotnet format`
- Analyzes package dependencies for outdated packages
- Uploads build artifacts

**Success Criteria**:
- Solution builds successfully
- No critical analyzer warnings
- Build artifacts generated

**Artifacts Generated**:
- `build-output`: Compiled binaries (7 days retention)

**Handoff**: → `handoff-dotnet-senior` → DevOps Engineer

---

### Stage 3: DevOps Engineer

**Responsibility**: CI/CD configuration validation and deployment artifact creation

**Actions**:
- Validates GitHub Actions workflow files exist
- Lints YAML workflow files with yamllint
- Creates deployment packages with published outputs
- Validates PowerShell build scripts syntax
- Packages deployment artifacts with manifests and scripts

**Success Criteria**:
- Workflow files are valid YAML
- Deployment package created successfully
- Build scripts (if any) are syntactically valid

**Artifacts Generated**:
- `deployment-package`: Ready-to-deploy package with published binaries, scripts, and manifests (30 days retention)

**Handoff**: → `handoff-devops` → Blazor Fluent UI Specialist

---

### Stage 4: Blazor Fluent UI Specialist

**Responsibility**: Frontend validation and build (React/Blazor/Fluent UI)

**Actions**:
- Detects frontend technology (React, Blazor, or both)
- For React frontend:
  - Validates Fluent UI dependencies in package.json
  - Installs npm dependencies (with caching)
  - Runs linting (if configured)
  - Builds frontend production bundle
- For Blazor projects:
  - Detects Blazor component projects
  - Builds Blazor projects
- Checks for UI documentation

**Success Criteria**:
- Frontend builds successfully (if present)
- Fluent UI packages detected (for React)
- No critical linting errors

**Artifacts Generated**:
- `frontend-build`: Built frontend assets (7 days retention)

**Handoff**: → `handoff-blazor-fluentui` → Test Specialist

---

### Stage 5: Test Specialist

**Responsibility**: Integration, E2E, and functional testing

**Actions**:
- Detects integration test projects (*.IntegrationTests.csproj)
- Detects E2E test projects (*.E2E.Tests.csproj)
- Detects functional test projects (*.FunctionalTests.csproj)
- Runs all detected non-unit test projects
- If no integration tests exist, provides actionable guidance

**Success Criteria**:
- All integration/E2E tests pass (if present)
- If no tests present, stage passes with warning and guidance

**Artifacts Generated**:
- `integration-test-results`: Test result TRX files (7 days retention)

**Handoff**: → `handoff-test-specialist` → Unit Test Specialist

---

### Stage 6: Unit Test Specialist

**Responsibility**: Execute unit tests with code coverage collection

**Actions**:
- Detects unit test projects (*.Tests.csproj, excluding integration tests)
- Runs unit tests with Coverlet for coverage collection
- Generates coverage reports in multiple formats (OpenCover, Cobertura, JSON)
- Creates HTML coverage report using ReportGenerator
- Extracts and displays line/branch coverage percentages

**Success Criteria**:
- All unit tests pass
- Coverage data collected successfully

**Artifacts Generated**:
- `unit-test-results`: Test result TRX files (7 days retention)
- `coverage-report`: HTML coverage report and raw coverage data (30 days retention)

**Handoff**: → `handoff-unit-test-specialist` → Orchestrator

---

### Stage 7: Orchestrator

**Responsibility**: Final coordination, artifact aggregation, and merge readiness verification

**Actions**:
- Downloads all artifacts from previous stages
- Verifies presence and integrity of:
  - Build artifacts
  - Deployment package
  - Frontend build
  - Test results (unit and integration)
  - Coverage reports
- Aggregates pipeline metrics
- Performs merge readiness checklist
- Generates comprehensive final report

**Success Criteria**:
- All critical artifacts present
- Minimum 6 of 7 readiness checks pass

**Final Output**:
- Comprehensive pipeline summary in GitHub Actions Summary
- Complete artifact inventory
- Next steps for PR review

---

## GitHub Environment Configuration

To enable manual approval gates, you must configure **GitHub Environments** in your repository settings.

**📖 For detailed setup instructions, see: [GitHub Environments Setup Guide](github-environments-setup.md)**

### Quick Summary

Create the following environments in your repository:

1. `handoff-azure-architect`
2. `handoff-dotnet-senior`
3. `handoff-devops`
4. `handoff-blazor-fluentui`
5. `handoff-test-specialist`
6. `handoff-unit-test-specialist`

### Configuration Steps

1. Navigate to your repository on GitHub
2. Go to **Settings** → **Environments**
3. Click **New environment**
4. For each environment listed above:
   - **Environment name**: Use exact names from the list above
   - **Protection rules**: 
     - ✅ Enable "Required reviewers"
     - Add one or more reviewers who must approve before the workflow continues
   - **Optional**: Set deployment branches (e.g., restrict to `main` branch)
5. Click **Save protection rules**

### How Approvals Work

When a stage completes successfully, the workflow pauses at the handoff gate and waits for approval:

1. A notification is sent to designated reviewers
2. Reviewers inspect the stage's output in the GitHub Actions summary
3. Reviewers either **Approve** or **Reject** the deployment
4. Upon approval, the next stage begins execution
5. If rejected, the workflow fails and does not proceed

### Running Without Approvals

If environments are not configured, the workflow will still run but **will not pause** for approvals. Handoff jobs will simply log a message and immediately proceed to the next stage. This is useful for:
- Initial testing of the workflow
- Non-production branches
- Automated CI runs on pull requests

## Running the Workflow

### Triggering the Workflow

#### 1. Manual Trigger (workflow_dispatch)

1. Navigate to **Actions** tab in GitHub
2. Select "Sequential Multi-Agent Workflow"
3. Click **Run workflow**
4. Choose branch (default: main)
5. Optional: Specify .NET version (default: 8.0.x)
6. Click **Run workflow**

#### 2. Pull Request Trigger

The workflow automatically runs on pull requests to `main` or `develop` branches when:
- PR is opened
- New commits are pushed to the PR branch
- PR is reopened

### Monitoring Progress

1. Go to **Actions** tab
2. Click on the running workflow
3. View the workflow graph showing:
   - Completed stages (green checkmark)
   - Current/pending stage (yellow spinner)
   - Waiting for approval (orange pause icon)
   - Failed stages (red X)

### Approving Handoffs

When a handoff gate is reached:

1. You'll receive a notification (if configured)
2. Navigate to the workflow run
3. Click the **Review deployments** button
4. Review the previous stage's output
5. Select the environment to approve
6. Add optional comment
7. Click **Approve and deploy** or **Reject**

## Artifacts and Outputs

### Artifact Retention

| Artifact Name | Contents | Retention | Generated By |
|---------------|----------|-----------|--------------|
| `build-output` | Compiled binaries | 7 days | .NET Senior Developer |
| `deployment-package` | Complete deployment bundle | 30 days | DevOps Engineer |
| `frontend-build` | Frontend production build | 7 days | Blazor Fluent UI Specialist |
| `integration-test-results` | Integration test TRX files | 7 days | Test Specialist |
| `unit-test-results` | Unit test TRX files | 7 days | Unit Test Specialist |
| `coverage-report` | HTML coverage report + XML | 30 days | Unit Test Specialist |

### Downloading Artifacts

1. Navigate to the completed workflow run
2. Scroll to the **Artifacts** section at the bottom
3. Click on the artifact name to download a ZIP file

### Stage Reports

Each stage generates a detailed report in the **GitHub Actions Summary**:

- Navigate to the workflow run
- Each job has a summary visible in the job output
- The Orchestrator stage provides a comprehensive final report

## Troubleshooting

### Common Issues

#### Issue: Workflow stuck at handoff gate

**Symptom**: Workflow shows "Waiting for approval" indefinitely

**Solution**:
1. Ensure GitHub Environments are configured (see [Configuration](#github-environment-configuration))
2. Check that reviewers are specified for the environment
3. Verify reviewers have been notified
4. An authorized reviewer must approve the deployment

---

#### Issue: .NET build fails

**Symptom**: Stage 2 (.NET Senior Developer) fails with build errors

**Solution**:
1. Check the build logs in the job output
2. Verify all dependencies are in NuGet repositories
3. Ensure .NET version is compatible (default: 8.0.x)
4. Check for project/solution file corruption
5. Test build locally: `dotnet build`

---

#### Issue: Unit tests fail

**Symptom**: Stage 6 (Unit Test Specialist) fails

**Solution**:
1. Review test failure details in the job output
2. Download `unit-test-results` artifact for detailed TRX files
3. Run tests locally: `dotnet test`
4. Fix failing tests before re-running workflow

---

#### Issue: Coverage report not generated

**Symptom**: No coverage report in artifacts

**Solution**:
1. Ensure test projects include Coverlet: `<PackageReference Include="coverlet.collector" />`
2. Verify tests ran successfully
3. Check that `coverlet.runsettings` file exists (created automatically if missing)

---

#### Issue: Frontend build fails

**Symptom**: Stage 4 (Blazor Fluent UI Specialist) fails

**Solution**:
1. Check if `package-lock.json` is committed (required for reproducible builds)
2. Verify Node.js version compatibility (workflow uses Node 18)
3. Test locally: `cd src/FabricMappingService.Frontend && npm ci && npm run build`
4. Check for missing environment variables

---

#### Issue: Workflow not triggered on PR

**Symptom**: Workflow doesn't start when PR is created

**Solution**:
1. Verify PR targets `main` or `develop` branch
2. Check that workflow file is committed to the target branch
3. Ensure workflow file syntax is valid (GitHub validates on push)

---

#### Issue: Missing artifacts in Orchestrator stage

**Symptom**: Orchestrator reports missing artifacts

**Solution**:
1. Check if previous stages completed successfully
2. Verify artifact upload steps succeeded in earlier stages
3. Note: Some artifacts are optional (e.g., frontend-build if no frontend exists)

---

### Getting Help

If you encounter issues not covered here:

1. Check the [GitHub Actions documentation](https://docs.github.com/actions)
2. Review workflow logs for specific error messages
3. Consult repository maintainers
4. Open an issue with:
   - Workflow run URL
   - Stage that failed
   - Relevant log excerpts

## Concurrency Control

The workflow uses concurrency groups to prevent overlapping runs:

```yaml
concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: false
```

This means:
- Only one workflow run per branch at a time
- New runs wait for previous runs to complete
- Runs are not cancelled if a new push occurs

## Caching Strategy

The workflow implements caching to speed up execution:

- **NuGet packages**: Cached by `actions/setup-dotnet@v4` and `actions/cache@v4`
- **npm packages**: Cached by `actions/setup-node@v4` with `cache: 'npm'`

Caches are scoped per branch and per OS.

## Workflow Inputs

The workflow accepts the following inputs (via `workflow_dispatch`):

| Input | Description | Default | Required |
|-------|-------------|---------|----------|
| `dotnet-version` | .NET SDK version to use | `8.0.x` | No |

Example:
```yaml
dotnet-version: '9.0.x'  # Use .NET 9 instead
```

## Best Practices

1. **Review Stage Outputs**: Always review the summary of each stage before approving handoffs
2. **Monitor Coverage**: Check that code coverage meets project standards (visible in Unit Test Specialist report)
3. **Update Dependencies**: Address outdated package warnings from .NET Senior Developer stage
4. **Document Changes**: Ensure documentation is updated when adding new features
5. **Test Locally First**: Run builds and tests locally before pushing to reduce workflow failures

## Future Enhancements

Potential improvements to this workflow:

- Integration with code quality tools (SonarQube, CodeQL)
- Automatic deployment to staging environments
- Slack/Teams notifications at handoff gates
- Performance benchmarking in Test Specialist stage
- Security scanning (dependency check, SAST)
- Docker image building in DevOps Engineer stage

---

**For questions or feedback, please consult repository maintainers or open an issue.**
