# ApplyAI Production Requirements & Deployment Specification

## 1. Overview
This document outlines the production requirements for deploying ApplyAI — a resume ATS analysis and cover letter generation SaaS platform. It is derived from the System Design Specification (SDS v1.0) generated via generate_sds.js.

**Target Environment**: Containerized, cloud-agnostic deployment (AWS, GCP, Azure, or alternatives like Railway, Render, Fly.io).

**Version**: 1.0 (aligned with SDS)

---

## 2. Non-Functional Requirements

### Performance
- **API Response Time**: p95 < 2 seconds for synchronous endpoints
- **Async Job Completion**: p95 < 45 seconds for ATS analysis/cover letter generation
- **Scalability**: Horizontal scaling of API and Worker services
- **Uptime**: 99.5% monthly

### Security & Compliance
- TLS 1.2+ everywhere
- GDPR compliant (data residency configurable)
- SOC2-ready audit logging
- OWASP Top 10 mitigations applied
- Rate limiting and input validation enforced

### Reliability
- Zero-downtime deployments
- Automated backups (daily DB + S3 versioning)
- Disaster recovery RPO < 1h, RTO < 4h

---

## 3. Infrastructure Requirements

### Core Services
| Component              | Technology                  | Minimum Specs                  | Scaling Strategy          |
|------------------------|-----------------------------|--------------------------------|---------------------------|
| API Server             | ASP.NET Core 8 (Docker)     | 2 vCPU, 4GB RAM                | Auto-scale (CPU 60%)      |
| Background Worker      | ASP.NET Core Worker         | 2 vCPU, 4GB RAM                | Fixed 2-5 instances       |
| Database               | PostgreSQL 16               | 2 vCPU, 8GB RAM, 100GB SSD     | Vertical + Read replicas  |
| Cache & Queue          | Redis 7                     | 2 vCPU, 4GB RAM                | Cluster mode (if >50 RPS) |
| Object Storage         | S3-compatible (R2/S3)       | —                              | —                         |
| CDN / Edge             | Cloudflare                  | —                              | —                         |

### Managed Services
- **Email**: SendGrid / Resend
- **Payments**: Paystack (NG) + Stripe (intl)
- **AI**: Anthropic Claude API (Sonnet)
- **Monitoring**: Sentry + Prometheus + Grafana
- **Secrets**: Doppler / AWS Secrets Manager

---

## 4. Environment Variables (Production)

```env
# Core
ASPNETCORE_ENVIRONMENT=Production
CONNECTIONSTRINGS__DEFAULT=Host=...;Database=applyai;...
REDIS__CONNECTION=...
ANTHROPIC__APIKEY=sk-...
STORAGE__TYPE=s3
STORAGE__BUCKET=applyai-resumes-prod
STORAGE__REGION=auto
STORAGE__ENDPOINT=https://...

# Auth
JWT__KEY=...
JWT__ISSUER=...
GOOGLE__CLIENTID=...
SENDGRID__APIKEY=...

# Billing
PAYSTACK__SECRETKEY=...
STRIPE__SECRETKEY=...

# Limits
MAX_CONCURRENT_AI_JOBS=5
FREE_ANALYSIS_LIMIT=10
FREE_COVER_LETTER_LIMIT=10
```

---

## 5. Docker Configuration

### Dockerfile (Multi-stage)
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore && dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ApplyAI.Api.dll"]
```

**Worker Entry Point**: Same image, different command via `DOTNET_RUNNING_IN_CONTAINER` + args.

---

## 6. Docker Compose (for Staging / Local Prod-like)

```yaml
version: '3.9'
services:
  api:
    build: .
    ports: ["8080:8080"]
    environment:
      - ASPNETCORE_ENVIRONMENT=Staging
    depends_on: [db, redis]

  worker:
    build: .
    command: ["dotnet", "ApplyAI.Worker.dll"]
    depends_on: [db, redis]

  db:
    image: postgres:16
    environment:
      POSTGRES_DB: applyai
      POSTGRES_USER: postgres
    volumes: ["postgres_data:/var/lib/postgresql/data"]

  redis:
    image: redis:7-alpine
    command: redis-server --appendonly yes

volumes:
  postgres_data:
```

---

## 7. CI/CD Pipeline Requirements
- **GitHub Actions** or GitLab CI
- Stages: test → build → scan (Trivy) → deploy to staging → manual approval → production
- Blue/Green or Canary deployment strategy
- Automated migration running on deploy

---

## 8. Monitoring & Alerting
- Health checks: `/health`, `/health/ready`
- Key metrics: queue depth, AI latency, error rate, quota usage
- Alerts: PagerDuty for critical issues

---

## 9. Cost Optimization
- Reserved instances / Savings Plans
- Redis + DB connection pooling
- AI job deduplication & caching
- S3 Intelligent-Tiering

---

**Next Steps**:
1. Finalize cloud provider selection (OI-01)
2. Provision infrastructure
3. Run security audit
4. Load test with 500 concurrent users

**Approval**:
- [ ] Architecture Review
- [ ] Security Review
- [ ] Product Sign-off