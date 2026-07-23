# ApplyAI Non-Functional Requirements (NFR)

Version: 1.0
Status: Architecture Baseline
Date: June 2026

---

# 1. Availability

## Business Context

ApplyAI is an MVP SaaS product targeting job seekers globally.

The platform is not mission-critical (unlike banking or healthcare systems), therefore a moderate SLA is acceptable during early growth stages.

---

## Availability Requirements

### NFR-AV-001

The platform shall maintain a minimum uptime of:

99.5% monthly uptime

### Target

* Monthly Uptime: 99.5%
* Maximum Downtime: 3.65 hours per month

### Reasoning

This aligns with startup-stage SaaS expectations while avoiding the infrastructure costs associated with 99.9%+ availability.

---

### NFR-AV-002

Planned maintenance windows shall occur during low-traffic periods.

### Target

* Maximum planned maintenance window: 2 hours
* Maintenance notice: Minimum 24 hours before outage

### Reasoning

Provides operational flexibility without significantly affecting users.

---

### NFR-AV-003

Critical incidents shall trigger automated alerts.

### Target

* Alert detection: < 5 minutes
* Initial response time: < 30 minutes

### Reasoning

Fast detection minimizes user impact and reputation damage.

---

# 2. Performance

## Business Context

Users expect a responsive experience.

The most expensive operation is AI analysis and cover letter generation.

---

## API Response Targets

### NFR-PERF-001

Standard API endpoints shall respond within the following limits.

| Metric | Target     |
| ------ | ---------- |
| P50    | < 200 ms   |
| P95    | < 500 ms   |
| P99    | < 1 second |

Applies to:

* Authentication
* Dashboard
* Resume retrieval
* User profile
* Subscription endpoints

---

### NFR-PERF-002

AI-powered endpoints shall complete within:

| Operation               | Target       |
| ----------------------- | ------------ |
| ATS Analysis            | < 30 seconds |
| Cover Letter Generation | < 20 seconds |

### Reasoning

Consistent with BRD requirements while accounting for LLM latency.

---

## Page Load Performance

### NFR-PERF-003

Frontend pages shall load within:

| Metric       | Target      |
| ------------ | ----------- |
| Landing Page | < 2 seconds |
| Dashboard    | < 3 seconds |
| ATS Results  | < 3 seconds |

Measured on average broadband connections.

---

## Database Performance

### NFR-PERF-004

Database queries shall complete within:

| Metric         | Target   |
| -------------- | -------- |
| P95 Query Time | < 100 ms |
| P99 Query Time | < 250 ms |

Excluding AI processing operations.

---

# 3. Scalability

## Business Context

ApplyAI is expected to launch with low traffic and grow gradually.

Architecture must support scaling without requiring microservices.

---

## User Growth Targets

### NFR-SCALE-001

System shall support:

| Stage         | Registered Users |
| ------------- | ---------------- |
| MVP Launch    | 1,000            |
| Year 1        | 10,000           |
| Growth Target | 100,000          |

---

## Concurrent User Targets

### NFR-SCALE-002

Platform shall support:

| Scenario     | Concurrent Users |
| ------------ | ---------------- |
| MVP          | 50               |
| Year 1       | 500              |
| Scale Target | 2,000            |

Without architectural redesign.

---

## Data Growth

### NFR-SCALE-003

Expected growth:

| Asset         | Monthly Growth |
| ------------- | -------------- |
| Resumes       | 5,000–20,000   |
| ATS Reports   | 10,000–50,000  |
| Cover Letters | 10,000–50,000  |

Storage architecture must support growth to:

1 TB of user content without migration.

---

## Peak Load Scenario

### NFR-SCALE-004

The system shall handle:

* 100 simultaneous resume uploads
* 50 simultaneous ATS analyses
* 50 simultaneous cover letter generations

Without service degradation.

---

# 4. Security

## Authentication

### NFR-SEC-001

Supported authentication methods:

* Email + Password
* Google OAuth

---

### NFR-SEC-002

Password requirements:

* Minimum 12 characters
* At least one uppercase character
* At least one lowercase character
* At least one number

Passwords shall be hashed using ASP.NET Core Identity defaults.

---

## Authorization

### NFR-SEC-003

Role-Based Access Control (RBAC)

Supported roles:

* Free User
* Premium User
* Administrator
* Super Administrator

Authorization shall be enforced at API level.

---

## Data Encryption

### NFR-SEC-004

All data in transit shall use:

TLS 1.3

Minimum acceptable:

TLS 1.2

---

### NFR-SEC-005

Sensitive data shall be encrypted at rest using:

AES-256

Applies to:

* Resume files
* User personal information
* Refresh tokens

---

## File Security

### NFR-SEC-006

Uploaded files shall:

* Be virus scanned
* Be type validated
* Be size validated
* Be stored outside the web root

Allowed file types:

* PDF
* DOCX

Maximum size:

10 MB

---

## Compliance

### NFR-SEC-007

Platform shall comply with:

* GDPR
* UK GDPR
* Basic privacy requirements for US users

Users shall be able to:

* Request account deletion
* Export personal data
* Withdraw consent

---

## Audit Logging

### NFR-SEC-008

Security events shall be logged.

Events include:

* Login attempts
* Password resets
* Subscription changes
* Admin actions

Logs retained for:

12 months

---

# 5. Cost Constraints

## Business Context

ApplyAI is a bootstrapped startup.

Infrastructure must prioritize low operational costs.

---

## Infrastructure Budget

### NFR-COST-001

Target infrastructure budget:

| Stage        | Monthly Budget |
| ------------ | -------------- |
| MVP          | <$50           |
| Early Growth | <$200          |
| Scale Stage  | <$1000         |

Excluding AI API costs.

---

## Hosting Strategy

### NFR-COST-002

Preferred hosting architecture:

Backend:

* ASP.NET Core
* Docker Container

Database:

* PostgreSQL

Storage:

* S3-Compatible Object Storage

Deployment:

* Single VPS during MVP

Preferred providers:

* Hetzner
* Contabo
* DigitalOcean

---

## AI Cost Controls

### NFR-COST-003

Every AI request shall record:

* User ID
* Tokens Used
* Processing Time
* Estimated Cost

---

### NFR-COST-004

Caching shall be implemented.

ATS analyses generated for identical resume content shall be cacheable for 30 days.

---

### NFR-COST-005

Usage limits shall be enforced before AI requests are executed.

This prevents unnecessary AI spending.

---

# Monitoring & Observability

### NFR-OBS-001

Application shall expose:

* Request metrics
* Error rates
* AI request duration
* AI request cost

via OpenTelemetry.

---

### NFR-OBS-002

Critical dashboards shall include:

* Daily active users
* Resume uploads
* ATS analyses
* Cover letter generations
* Revenue metrics
* AI spending

---

# Acceptance Criteria

The platform satisfies NFR requirements when:

* Monthly uptime ≥ 99.5%
* Standard API p95 < 500ms
* ATS analysis < 30 seconds
* Supports 500 concurrent users
* All sensitive data encrypted
* Infrastructure cost remains within defined budget
* AI usage is fully auditable
