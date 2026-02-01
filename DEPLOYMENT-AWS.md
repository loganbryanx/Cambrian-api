# AWS ECS Deployment

This guide explains how to deploy the Cambrian API to AWS ECS using the automated GitHub Actions workflow.

## Prerequisites

Before the GitHub Actions workflow can deploy to AWS, you need to configure the following GitHub secrets in your repository settings (Settings → Secrets and variables → Actions → New repository secret):

### Required Secrets

1. **AWS_REGION** - The AWS region for your ECS cluster (e.g., `us-east-1`)
2. **AWS_ACCESS_KEY_ID** - AWS access key with permissions to push to ECR and deploy to ECS
3. **AWS_SECRET_ACCESS_KEY** - AWS secret access key
4. **ECR_REPOSITORY** - The name of your ECR repository (e.g., `cambrian-api`)
5. **ECS_CLUSTER** - The name of your ECS cluster
6. **ECS_SERVICE** - The name of your ECS service

## Setup Steps

### 1. Create AWS Resources

Before configuring the secrets, you need to create the following AWS resources:

1. **ECR Repository** - Create a repository in Amazon ECR to store Docker images
2. **ECS Cluster** - Create an ECS cluster (Fargate is recommended)
3. **ECS Task Definition** - Update `deploy/ecs-task-def.json` with your values:
   - Replace `REPLACE_WITH_EXECUTION_ROLE_ARN` with your ECS task execution role ARN
   - Replace `REPLACE_WITH_TASK_ROLE_ARN` with your ECS task role ARN
   - Replace `REPLACE_WITH_SECRET_ARN` with your AWS Secrets Manager secret ARN (for database connection string)
   - Replace `REPLACE_WITH_REGION` with your AWS region
4. **ECS Service** - Create an ECS service using the task definition

### 2. Create IAM User

Create an IAM user with the following permissions:
- `AmazonEC2ContainerRegistryPowerUser` (or specific ECR permissions)
- `AmazonECS_FullAccess` (or specific ECS permissions)

Save the access key ID and secret access key for use in GitHub secrets.

### 3. Configure GitHub Secrets

Add all the required secrets listed above to your repository settings.

### 4. Trigger Deployment

The workflow automatically runs when you push changes to the `main` branch that affect:
- `src/**`
- `tests/**`
- `Cambrian.sln`
- `Dockerfile`
- `.github/workflows/deploy-api.yml`
- `deploy/ecs-task-def.json`

## Troubleshooting

### Workflow fails with "Input required and not supplied: aws-region"

This means the `AWS_REGION` secret is not configured. Make sure all required secrets are set in your repository settings.

### Deployment succeeds but service is unhealthy

Check the ECS service logs in CloudWatch Logs. Common issues:
- Database connection string not configured correctly
- Container port mismatch
- Task role permissions

## Alternative Deployment (Render)

For simpler deployment without AWS configuration, see [DEPLOYMENT.md](DEPLOYMENT.md) for Render deployment instructions.
