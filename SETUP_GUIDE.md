# Cambrian Organization Setup Guide

This guide walks you through setting up the Cambrian organization structure on GitHub with three independent repositories.

## 📋 Prerequisites

- GitHub account with organization creation privileges
- Access to create repositories
- Basic understanding of Git and GitHub

## 🎯 Step 1: Create the Organization

1. Go to [GitHub](https://github.com)
2. Click your **profile photo** in the top-right corner
3. Select **Your organizations**
4. Click the **New organization** button
5. Choose a plan (Free works for public repositories)
6. Enter the organization name:
   - Recommended: **Cambrian**
   - Alternatives: **CambrianLabs**, **CambrianAI**
7. Set the contact email
8. Choose "My personal account" or appropriate ownership
9. Click **Next** and complete the setup

✅ **Your organization is now the source of truth**

## 🔧 Step 2: Create the Three Repositories

### 2.1 Transfer or Create cambrian-api (if not already in org)

**If migrating from personal account:**
1. Go to the existing `loganbryanx/Cambrian-api` repository
2. Click **Settings** → **General**
3. Scroll to **Danger Zone** → **Transfer ownership**
4. Enter the organization name: `Cambrian`
5. Confirm the transfer
6. Repository is now at `Cambrian/cambrian-api`

**If creating new:**
1. Go to your Cambrian organization page
2. Click **New repository**
3. Name: `cambrian-api`
4. Description: "Backend API services for the Cambrian ecosystem"
5. Choose Public or Private
6. Do NOT initialize with README (if transferring existing code)
7. Click **Create repository**

### 2.2 Create cambrian-frontend

1. Go to your Cambrian organization page
2. Click **New repository**
3. Name: `cambrian-frontend`
4. Description: "Frontend application for the Cambrian ecosystem"
5. Choose Public or Private
6. Initialize with README: ✅ (checked)
7. Add .gitignore: **Node** (if using React/Next.js)
8. Choose a license (optional)
9. Click **Create repository**

### 2.3 Create cambrian-infra

1. Go to your Cambrian organization page
2. Click **New repository**
3. Name: `cambrian-infra`
4. Description: "Infrastructure and deployment automation for the Cambrian ecosystem"
5. Choose Public or Private
6. Initialize with README: ✅ (checked)
7. Add .gitignore: **Terraform** (or appropriate for your IaC tool)
8. Choose a license (optional)
9. Click **Create repository**

### 2.4 Update Repository Settings

For each repository, ensure:
- **Description** is clear and describes its domain
- **Topics/Tags** are added (e.g., `api`, `backend`, `aspnet-core` for cambrian-api)
- **About section** includes website URL (if applicable)

## 📌 Step 3: Pin the Repositories

1. Go to your organization homepage: `https://github.com/Cambrian`
2. Click **Customize profile** (or edit pins icon)
3. Click **Pin repositories**
4. Select these three repositories:
   - ✅ cambrian-frontend
   - ✅ cambrian-api
   - ✅ cambrian-infra
5. Arrange them in order (drag and drop)
6. Click **Save pins**

**Result:** Anyone landing on the Cambrian organization page will see these three repositories clearly grouped and intentionally displayed.

## 📝 Step 4: Set Up Repository READMEs

### cambrian-api
✅ Already configured with:
- Clear description of API domain
- Links to other repositories
- Independent deployment instructions

### cambrian-frontend
Create/update README.md with:
```markdown
# Cambrian Frontend

> **Part of the Cambrian Organization** - User interface and client-side logic

React/Next.js application for the Cambrian ecosystem.

## What is Cambrian Frontend?

This repository contains the user interface for Cambrian. It provides:
- User interface components
- Client-side state management
- API consumption from cambrian-api
- Responsive design

This is one of three repositories in the Cambrian organization:
- **cambrian-frontend** (this repo) - User interface and client-side logic
- **cambrian-api** - Backend services and API
- **cambrian-infra** - Infrastructure and deployment automation

For more information, see the [main organization](https://github.com/Cambrian).

## Quick Start
[Add your quick start instructions]

## Deployment
This frontend deploys independently to Vercel/Netlify.
See [DEPLOYMENT.md](DEPLOYMENT.md) for details.
```

### cambrian-infra
Create/update README.md with:
```markdown
# Cambrian Infrastructure

> **Part of the Cambrian Organization** - Infrastructure and deployment automation

Infrastructure as Code for the Cambrian ecosystem.

## What is Cambrian Infra?

This repository contains infrastructure configuration for Cambrian. It provides:
- Infrastructure as Code (Terraform/CloudFormation)
- CI/CD pipeline definitions
- Monitoring and logging setup
- Environment configurations
- Deployment automation

This is one of three repositories in the Cambrian organization:
- **cambrian-frontend** - User interface and client-side logic
- **cambrian-api** - Backend services and API
- **cambrian-infra** (this repo) - Infrastructure and deployment automation

For more information, see the [main organization](https://github.com/Cambrian).

## Structure
[Add your infrastructure structure]

## Usage
[Add usage instructions]
```

## 🔐 Step 5: Configure Repository Access (Optional)

1. Go to organization **Settings** → **Member privileges**
2. Set default repository permissions
3. Create teams if needed (e.g., Frontend Team, Backend Team, DevOps Team)
4. Assign teams to appropriate repositories

## 🎨 Step 6: Customize Organization Profile

1. Go to organization **Settings** → **Profile**
2. Add organization description
3. Add organization avatar/logo
4. Add organization website URL
5. Add social links (Twitter, LinkedIn, etc.)
6. Create an organization README (optional):
   - Create a repository named `.github`
   - Add a `profile/README.md` file
   - This README will appear on your organization homepage

## 🔗 Step 7: Update Cross-Repository Links

After creating all repositories, update any hardcoded links:

1. In **cambrian-api/ORGANIZATION.md**, update links from:
   ```markdown
   [cambrian-frontend](https://github.com/Cambrian/cambrian-frontend)
   ```
   To actual organization name if different

2. Update the same in README.md files across all repos

## ✅ Verification Checklist

- [ ] Organization created with name "Cambrian" (or chosen alternative)
- [ ] Three repositories created and properly named
- [ ] All repositories have clear descriptions
- [ ] cambrian-api has been transferred/created in the organization
- [ ] cambrian-frontend has been created with appropriate README
- [ ] cambrian-infra has been created with appropriate README
- [ ] All three repositories are pinned on the organization homepage
- [ ] Organization profile is customized with description and avatar
- [ ] Repository access and permissions are configured
- [ ] Cross-repository links are updated and working
- [ ] Each repository deploys independently

## 🚀 Next Steps

After setup:
1. **Configure CI/CD** pipelines in each repository
2. **Set up environment variables** for integration between repos
3. **Deploy each service** independently
4. **Test integration** between frontend and API
5. **Document** any additional setup steps in each repository

## 📚 Additional Resources

- [GitHub Organizations Documentation](https://docs.github.com/en/organizations)
- [Repository Pinning](https://docs.github.com/en/account-and-profile/setting-up-and-managing-your-github-profile/customizing-your-profile/pinning-items-to-your-profile)
- [Organization Profile README](https://docs.github.com/en/organizations/collaborating-with-groups-in-organizations/customizing-your-organizations-profile)

## 💡 Tips

- **Keep repositories independent**: Each should work on its own
- **Document everything**: Clear READMEs prevent confusion
- **Use consistent naming**: Follow established patterns
- **Set up branch protection**: Protect main/master branches
- **Enable required reviews**: Ensure code quality
- **Use GitHub Actions**: Automate testing and deployment
- **Monitor dependencies**: Use Dependabot for security updates

## 🆘 Troubleshooting

**Can't create organization?**
- Verify your GitHub account status
- Check if you've reached organization limits
- Contact GitHub support if issues persist

**Can't transfer repository?**
- Ensure you have admin access to the source repository
- Verify the destination organization exists
- Check if repository name conflicts exist

**Pins not showing?**
- Ensure repositories are public (or you're viewing while logged in)
- Try unpinning and re-pinning
- Clear browser cache

---

**Need Help?** Open an issue in the appropriate repository or contact the organization administrators.
