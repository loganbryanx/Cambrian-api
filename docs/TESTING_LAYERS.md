# Testing Layers

This document captures the required testing layers and completion criteria for Cambrian.

## Layer 4: E2E Tests (6 complete user flows documented)

**Status:** Pending details

Document six complete user flows. Each flow should include:
- Preconditions
- Steps (UI + API)
- Expected results
- Test data
- Cleanup

**Flows (to fill in):**
1. 
2. 
3. 
4. 
5. 
6. 

## Layer 3: Integration Tests (APIs, databases, AWS resources)

**Status:** Partially implemented

Scope:
- API contracts
- Database integrations
- AWS resources (e.g., S3, SES, SNS, SQS, EventBridge)

**Owned scripts / entry points:**
- scripts/integration-test.sh

**Environment requirements:**
- API base URL
- Valid auth token
- Database connectivity
- Required AWS credentials/roles

## Layer 2: Service Health Tests (7 microservices)

**Status:** Pending details

Each service must expose a health endpoint with clear status codes and optional dependency checks.

**Services (to fill in):**
1. 
2. 
3. 
4. 
5. 
6. 
7. 

**Owned scripts / entry points:**
- scripts/health-check.sh

## Layer 1: Infrastructure Tests (Terraform, ECS, ALB, RDS)

**Status:** Partially implemented

Scope:
- Terraform plan/apply validation
- ECS service health
- ALB listener/target group health
- RDS connectivity

**Owned scripts / entry points:**
- scripts/validate-infra.sh
