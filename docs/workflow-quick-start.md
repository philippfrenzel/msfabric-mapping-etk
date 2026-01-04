# Quick Start: Running the Sequential Multi-Agent Workflow

This guide helps you run the Sequential Multi-Agent Workflow for the first time.

## Prerequisites

Before running the workflow:

1. ✅ The workflow files are merged to your target branch (`main` or `develop`)
2. ✅ (Optional) [GitHub Environments are configured](github-environments-setup.md) for approval gates
3. ✅ You have **write access** to the repository

## Option 1: Trigger Manually (Recommended for First Run)

### Step 1: Navigate to Actions

1. Go to your repository on GitHub
2. Click the **Actions** tab at the top

### Step 2: Select the Workflow

1. In the left sidebar, find **Sequential Multi-Agent Workflow**
2. Click on it

### Step 3: Run the Workflow

1. On the right side, click **Run workflow**
2. A dropdown appears with options:
   - **Branch**: Select the branch to run on (default: `main`)
   - **.NET SDK version**: Leave default `8.0.x` or change if needed
3. Click the green **Run workflow** button
4. The page refreshes and shows a new workflow run starting

### Step 4: Watch Progress

1. Click on the workflow run (shows as "Sequential Multi-Agent Workflow #1" or similar)
2. You'll see the workflow graph:
   ```
   Stage 1: Azure Architect → Handoff Gate → Stage 2: .NET Senior Developer → ...
   ```
3. Watch as each stage completes (green checkmark)

### Step 5: Approve Handoffs (if configured)

When a handoff gate is reached:

1. The stage shows **Waiting** with an orange pause icon
2. A yellow banner appears: **Review pending deployments**
3. Click **Review deployments**
4. Review the previous stage's output in the job summary
5. Select environment to approve
6. Click **Approve and deploy**
7. The next stage starts immediately

### Step 6: Review Results

After all stages complete:

1. The final **Orchestrator** stage shows a comprehensive summary
2. Click on any stage to see its detailed logs
3. Scroll down to **Artifacts** to download build outputs, test results, coverage reports

## Option 2: Automatic Trigger via Pull Request

### Step 1: Create a Branch

```bash
git checkout -b feature/your-feature-name
```

### Step 2: Make Changes

```bash
# Make your code changes
git add .
git commit -m "Your commit message"
git push origin feature/your-feature-name
```

### Step 3: Open a Pull Request

1. Go to GitHub repository
2. Click **Pull requests** tab
3. Click **New pull request**
4. Select:
   - **base**: `main` or `develop`
   - **compare**: `feature/your-feature-name`
5. Click **Create pull request**
6. Add title and description
7. Click **Create pull request**

### Step 4: Workflow Starts Automatically

- The workflow triggers automatically on PR creation
- Check the **Checks** tab at the bottom of the PR
- Click **Details** next to "Sequential Multi-Agent Workflow"
- Follow steps 4-6 from Option 1 above

## What Each Stage Does

Here's what happens at each stage:

### 1️⃣ Azure Architect (1-2 min)
- ✅ Validates repository structure
- ✅ Checks Fabric manifest
- ✅ Verifies documentation exists
- **Output**: Architecture report

### 2️⃣ .NET Senior Developer (2-5 min)
- ✅ Restores NuGet packages
- ✅ Builds solution in Release mode
- ✅ Runs code analyzers
- ✅ Checks code formatting
- **Output**: Build artifacts

### 3️⃣ DevOps Engineer (3-5 min)
- ✅ Validates GitHub Actions workflows
- ✅ Lints YAML files
- ✅ Creates deployment package
- ✅ Validates build scripts
- **Output**: Deployment package (30 days retention)

### 4️⃣ Blazor Fluent UI Specialist (3-8 min)
- ✅ Detects frontend technology
- ✅ Validates Fluent UI dependencies
- ✅ Builds React frontend
- ✅ Runs linting (if configured)
- **Output**: Frontend build artifacts

### 5️⃣ Test Specialist (2-10 min)
- ✅ Detects integration test projects
- ✅ Runs integration/E2E tests
- ✅ Provides guidance if tests missing
- **Output**: Integration test results

### 6️⃣ Unit Test Specialist (2-5 min)
- ✅ Runs all unit tests
- ✅ Collects code coverage
- ✅ Generates coverage report
- **Output**: Test results + coverage report (30 days retention)

### 7️⃣ Orchestrator (1-2 min)
- ✅ Downloads all artifacts
- ✅ Verifies completeness
- ✅ Aggregates metrics
- ✅ Performs merge readiness check
- **Output**: Final comprehensive report

**Total Time**: ~15-35 minutes (without wait times between approvals)

## Understanding Workflow Output

### Job Summaries

Each stage creates a summary. To view:

1. Click on a completed stage job
2. Scroll to the top of the log output
3. Look for the colored summary box with:
   - ✅ Success indicators
   - ⚠️ Warnings
   - 📊 Metrics and statistics

### Artifacts

Download artifacts:

1. Go to the completed workflow run
2. Scroll to the bottom
3. Click **Artifacts** section
4. Click artifact name to download:
   - `build-output`: Compiled DLLs
   - `deployment-package`: Full deployment bundle
   - `frontend-build`: Built React app
   - `unit-test-results`: TRX test result files
   - `coverage-report`: HTML coverage report
   - `integration-test-results`: Integration test TRX files

### Logs

View detailed logs:

1. Click any stage job name
2. Expand steps on the left sidebar
3. Click a step to see its output
4. Use search (Ctrl+F) to find specific messages

## Common Scenarios

### Scenario: Build Fails

**Stage**: .NET Senior Developer

**What to do**:
1. Click on the failed stage
2. Look for red ❌ error messages
3. Fix the build error locally:
   ```bash
   dotnet build
   ```
4. Push the fix
5. Workflow re-runs automatically (on PR) or re-trigger manually

### Scenario: Tests Fail

**Stage**: Unit Test Specialist or Test Specialist

**What to do**:
1. Download the test results artifact
2. Open TRX file in Visual Studio or convert to readable format
3. Fix failing tests locally:
   ```bash
   dotnet test
   ```
4. Push the fix

### Scenario: Coverage Too Low

**Stage**: Unit Test Specialist

**What to do**:
1. Download coverage report artifact
2. Open `index.html` in the coverage report
3. Identify uncovered code (red highlights)
4. Add tests for uncovered paths
5. Re-run workflow

### Scenario: Frontend Build Fails

**Stage**: Blazor Fluent UI Specialist

**What to do**:
1. Check the error in logs
2. Test locally:
   ```bash
   cd src/FabricMappingService.Frontend
   npm install
   npm run build
   ```
3. Fix TypeScript/build errors
4. Push the fix

## Tips for Faster Iteration

### 1. Run Locally First

Before pushing, run key commands locally:

```bash
# Build
dotnet build

# Test
dotnet test

# Frontend (if you changed UI)
cd src/FabricMappingService.Frontend
npm run build
```

### 2. Fix Multiple Issues at Once

- Review all stage outputs
- Fix all issues in one commit
- Saves multiple workflow runs

### 3. Use Draft PRs

- Create a **Draft PR** for work-in-progress
- Workflow still runs but clearly marked as draft
- Convert to ready when all stages pass

### 4. Skip Approvals for Feature Branches

If environments are configured only for `main`:
- Feature branch PRs run without approval waits
- Faster feedback loop
- Only production merges require approvals

## Troubleshooting

### Workflow Doesn't Start

**Check**:
- Workflow file exists in `.github/workflows/` on the target branch
- PR targets `main` or `develop` (configured in workflow)
- You have Actions enabled (Settings → Actions → General)

### Stage Shows "Skipped"

**Reason**: Previous stage failed or workflow was cancelled

**Solution**: Fix the failing stage and re-run

### Can't Approve Handoff

**Check**:
- You are added as a reviewer in environment settings
- You have write access to the repository
- Environment name matches exactly (case-sensitive)

### Artifacts Missing

**Check**:
- The stage that generates the artifact completed successfully
- Check retention period (7 or 30 days)
- Some artifacts are conditional (e.g., frontend-build only if frontend exists)

## Next Steps

After successful first run:

1. ✅ Configure [GitHub Environments](github-environments-setup.md) for approval gates
2. ✅ Customize .NET version if needed (workflow input)
3. ✅ Review and adjust stage timeouts if needed
4. ✅ Set up [notifications](github-environments-setup.md#notifications) for approvals
5. ✅ Add integration tests (if you see the placeholder warning)
6. ✅ Review coverage report and improve test coverage

## Getting Help

- 📖 [Full Workflow Documentation](sequential-handoff-workflow.md)
- 🔧 [Environment Setup Guide](github-environments-setup.md)
- 🐛 Open an issue for bugs or questions
- 💬 Contact repository maintainers

---

**Ready to run?** Go to **Actions** → **Sequential Multi-Agent Workflow** → **Run workflow** 🚀
