# GitHub Environments Setup Guide

This guide explains how to configure GitHub Environments for the Sequential Multi-Agent Workflow. Environments enable manual approval gates between workflow stages.

## Overview

The workflow uses **6 environment gates** to pause execution between stages and require manual approval before proceeding. This ensures each stage's work is reviewed before the next stage begins.

## Required Environments

You need to create the following environments:

1. `handoff-azure-architect`
2. `handoff-dotnet-senior`
3. `handoff-devops`
4. `handoff-blazor-fluentui`
5. `handoff-test-specialist`
6. `handoff-unit-test-specialist`

## Configuration Steps

### Step 1: Navigate to Repository Settings

1. Go to your repository on GitHub
2. Click **Settings** (requires admin/write access)
3. In the left sidebar, click **Environments**

### Step 2: Create Each Environment

For each of the 6 environments listed above:

1. Click **New environment**
2. Enter the **Environment name** exactly as shown above (case-sensitive)
3. Click **Configure environment**

### Step 3: Configure Protection Rules

For each environment, configure the following protection rules:

#### Required Reviewers

1. ✅ Check **Required reviewers**
2. Click **Add reviewers**
3. Add one or more users/teams who must approve:
   - Repository maintainers
   - Tech leads
   - Senior developers
   - Or specific team members for each stage (e.g., DevOps team for `handoff-devops`)
4. Set **Number of required reviewers**: 1 (or more for critical stages)

#### Deployment Branches (Optional)

1. Select **Selected branches**
2. Add branch rules:
   - `main` (production deployments)
   - `develop` (staging deployments)
   - Or use a pattern like `release/*`

#### Wait Timer (Optional)

1. Set a **Wait timer** if you want a minimum delay before approval can occur
2. Example: 5 minutes to ensure reviewers have time to review logs

### Step 4: Save Configuration

1. Click **Save protection rules** for each environment
2. Repeat for all 6 environments

## Environment Details

### handoff-azure-architect

- **Purpose**: Approve transition from architecture validation to .NET development
- **Recommended Reviewers**: Architects, tech leads
- **What to Check**: Architecture report, manifest validation, project structure

### handoff-dotnet-senior

- **Purpose**: Approve transition from build/analysis to DevOps
- **Recommended Reviewers**: Senior developers, tech leads
- **What to Check**: Build success, analyzer warnings, code formatting

### handoff-devops

- **Purpose**: Approve transition from CI/CD setup to frontend validation
- **Recommended Reviewers**: DevOps engineers, infrastructure team
- **What to Check**: Workflow validation, artifact creation, deployment package

### handoff-blazor-fluentui

- **Purpose**: Approve transition from frontend work to testing
- **Recommended Reviewers**: Frontend developers, UI/UX team
- **What to Check**: Frontend build success, Fluent UI integration, linting results

### handoff-test-specialist

- **Purpose**: Approve transition from integration tests to unit tests
- **Recommended Reviewers**: QA engineers, test specialists
- **What to Check**: Integration test results, E2E test coverage

### handoff-unit-test-specialist

- **Purpose**: Approve final transition to orchestrator
- **Recommended Reviewers**: Senior developers, tech leads
- **What to Check**: Unit test results, code coverage percentage, test quality

## Approval Workflow

### When a Handoff Gate is Reached

1. The workflow pauses at the handoff job
2. Designated reviewers receive a notification (if configured)
3. The job shows status: **Waiting** with an orange pause icon

### How to Approve

1. Navigate to the workflow run in **Actions** tab
2. You'll see a yellow banner: **Review pending deployments**
3. Click **Review deployments**
4. A modal appears showing:
   - Environment name
   - Previous stage results (in job summary)
   - Deployment details
5. Select the environment(s) to approve
6. Add an optional comment
7. Choose:
   - **Approve and deploy** (continue to next stage)
   - **Reject** (fail the workflow)

### After Approval

- The handoff job completes
- The next stage immediately begins execution
- The workflow continues sequentially

### After Rejection

- The workflow fails and stops
- No further stages execute
- The PR/branch status is updated to failed

## Running Without Approvals

If environments are **not configured**:

- The workflow still runs normally
- Handoff jobs execute but don't wait for approval
- All stages run sequentially without pauses
- Useful for:
  - Testing the workflow
  - Non-protected branches (e.g., feature branches)
  - CI validation on pull requests

To enable this behavior:
- Simply don't create the environments
- Or don't configure protection rules

## Best Practices

### 1. Different Reviewers Per Stage

Assign stage-appropriate reviewers:
- **Azure Architect**: Cloud architects
- **.NET Senior**: Lead developers
- **DevOps**: Infrastructure/platform team
- **Blazor/UI**: Frontend team
- **Tests**: QA team

### 2. Require Multiple Approvals for Production

For `main` branch deployments:
- Set required reviewers count to 2 or more
- Include at least one tech lead or architect

### 3. Use Teams Instead of Individuals

- Assign GitHub Teams as reviewers instead of individuals
- Ensures approvals even when someone is unavailable
- Example: `@myorg/devops-team`, `@myorg/frontend-team`

### 4. Document Approval Criteria

Create a checklist for each stage. Example for `handoff-dotnet-senior`:

```markdown
Before approving, verify:
- [ ] Build completed successfully
- [ ] No critical analyzer warnings
- [ ] Code formatting checks passed
- [ ] No new compiler errors introduced
```

### 5. Set Wait Timers for Critical Stages

For production-critical stages:
- Set a 5-10 minute wait timer
- Ensures reviewers have time to properly review logs
- Prevents rushed approvals

### 6. Use Deployment Branch Protection

Limit approvals to specific branches:
- `main`: Always require approval
- `develop`: Require approval for specific stages
- Feature branches: Run without approval (faster feedback)

## Notifications

### Email Notifications

Reviewers receive email notifications when:
- A deployment is waiting for their review
- They are mentioned in approval comments
- A deployment they reviewed completes or fails

### In-App Notifications

Reviewers see notifications:
- In the GitHub UI (bell icon)
- On the Actions page
- On the repository main page

### Slack/Teams Integration

Configure webhooks for real-time notifications:
1. Go to **Settings** → **Webhooks**
2. Add webhook URL for Slack/Teams
3. Configure events: `deployment`, `deployment_status`
4. Workflow status will post to your channel

## Troubleshooting

### Issue: No approval button appears

**Solution:**
- Ensure you have write/admin access to the repository
- Check that the environment is properly configured
- Verify the environment name matches exactly (case-sensitive)

### Issue: Can't add reviewers

**Solution:**
- Ensure users/teams have read access to the repository
- For organization repositories, check team visibility settings
- Users must have GitHub accounts

### Issue: Workflow bypasses approvals

**Possible Causes:**
- Environment not created
- Environment name mismatch
- Branch not in deployment branches list
- No protection rules configured

**Solution:**
- Verify environment configuration
- Check environment name spelling
- Ensure protection rules are saved

### Issue: Multiple people can approve same deployment

**Expected Behavior:**
- By default, any designated reviewer can approve
- To require specific approvals, add multiple reviewers and set count > 1

## Environment Configuration as Code (Advanced)

While environments must be created via UI, you can document your configuration:

```yaml
# .github/environment-config.yml (documentation only, not used by GitHub)
environments:
  - name: handoff-azure-architect
    protection_rules:
      required_reviewers:
        - @architects-team
      wait_timer: 0
      deployment_branches:
        - main
        - develop
  
  - name: handoff-dotnet-senior
    protection_rules:
      required_reviewers:
        - @senior-devs
        - @tech-leads
      wait_timer: 0
      deployment_branches:
        - main
```

This serves as documentation for your team.

## Testing Your Configuration

After configuring environments:

1. Create a test branch
2. Make a small change
3. Open a PR to `main`
4. Watch the workflow execution
5. Verify handoff gates appear
6. Test the approval flow
7. Confirm next stage starts after approval

## Security Considerations

1. **Limit Environment Access**: Only repository admins should manage environments
2. **Reviewer Permissions**: Reviewers need write access but don't need admin
3. **Audit Logs**: Check **Settings** → **Audit log** to see approval history
4. **Secrets in Environments**: You can add environment-specific secrets (not used in this workflow)

## Further Reading

- [GitHub Environments Documentation](https://docs.github.com/en/actions/deployment/targeting-different-environments/using-environments-for-deployment)
- [Required Reviewers](https://docs.github.com/en/actions/deployment/targeting-different-environments/using-environments-for-deployment#required-reviewers)
- [Deployment Branches](https://docs.github.com/en/actions/deployment/targeting-different-environments/using-environments-for-deployment#deployment-branches)

---

**Need Help?** Open an issue or contact repository maintainers.
