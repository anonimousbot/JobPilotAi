# Backend-Specification.md

# JobPilotAi Backend Specification

Version: 1.0

Status: Architecture Baseline

Author: Product Team

Date: June 2026

---

# 1. System Overview

JobPilotAi is an AI-powered career platform that helps job seekers:

* Upload resumes
* Analyze ATS compatibility
* Generate cover letters
* Track application readiness
* Manage subscriptions

The backend shall expose REST APIs using ASP.NET Core Minimal APIs.

Architecture shall follow:

* Modular Monolith
* Vertical Slice Architecture
* Feature-Based Organization

---

# 2. Core Modules

## Identity Module

Responsibilities:

* Registration
* Login
* Email Verification
* Password Reset
* Refresh Tokens
* Profile Management

---

## Resume Module

Responsibilities:

* Resume Upload
* Resume Storage
* Resume Retrieval
* Resume Deletion
* Resume Parsing

---

## ATS Analysis Module

Responsibilities:

* ATS Scoring
* Resume Feedback
* Keyword Analysis
* Recommendation Generation

---

## Cover Letter Module

Responsibilities:

* Cover Letter Generation
* Cover Letter Editing
* Cover Letter History

---

## Subscription Module

Responsibilities:

* Plan Management
* Billing
* Quota Management

---

## Administration Module

Responsibilities:

* User Management
* Analytics
* Audit Logs

---

# 3. User Roles

## FreeUser

Permissions:

* Upload resumes
* Generate ATS analyses
* Generate cover letters
* View dashboard

Restrictions:

* 5 stored resumes
* 10 ATS analyses per month
* 10 cover letters per month

---

## PremiumUser

Permissions:

* Unlimited resumes
* Unlimited ATS analyses
* Unlimited cover letters

---

## Admin

Permissions:

* Manage users
* View analytics
* View audit logs

---

## SuperAdmin

Permissions:

* Manage pricing
* Manage plans
* Manage admins
* Platform configuration

---

# 4. Database Schema

## Users

Fields:

* Id
* Email
* PasswordHash
* Role
* EmailConfirmed
* CreatedAt
* UpdatedAt
* Status

---

## Profiles

Fields:

* Id
* UserId
* FirstName
* LastName
* PhoneNumber
* Location
* LinkedInUrl
* PortfolioUrl
* TargetRole
* CareerLevel

Relationship:

User 1:1 Profile

---

## Resumes

Fields:

* Id
* UserId
* FileName
* FileType
* FileSize
* StoragePath
* ExtractedText
* SkillsJson
* ExperienceJson
* EducationJson
* UploadedAt

Relationship:

User 1:N Resumes

---

## ResumeAnalyses

Fields:

* Id
* ResumeId
* UserId
* AtsScore
* KeywordMatchScore
* SkillsCoverageScore
* FormattingScore
* ExperienceScore
* EducationScore
* WeaknessesJson
* RecommendationsJson
* CreatedAt

Relationship:

Resume 1:N ResumeAnalyses

---

## CoverLetters

Fields:

* Id
* UserId
* ResumeId (nullable)
* JobTitle
* CompanyName
* JobDescription
* Tone
* OriginalContent
* EditedContent
* CreatedAt
* UpdatedAt

---

## Subscriptions

Fields:

* Id
* UserId
* PlanType
* Status
* StartDate
* EndDate
* BillingCycle

---

## UsageRecords

Fields:

* Id
* UserId
* ActionType
* Quantity
* PeriodStart
* PeriodEnd
* CreatedAt

---

## PaymentEvents

Fields:

* Id
* SubscriptionId
* Amount
* Currency
* Provider
* ProviderReference
* Status
* CreatedAt

---

## RefreshTokens

Fields:

* Id
* UserId
* Token
* ExpiresAt
* RevokedAt

---

## AdminLogs

Fields:

* Id
* AdminId
* Action
* TargetEntity
* TargetId
* MetadataJson
* CreatedAt

---

# 5. ATS Analysis Rules

ATS score range:

0-100

Calculation weights:

| Category             | Weight |
| -------------------- | ------ |
| Keyword Match        | 35%    |
| Skills Coverage      | 25%    |
| Formatting           | 15%    |
| Experience Relevance | 15%    |
| Education Match      | 10%    |

Output:

* Overall Score
* Weaknesses
* Missing Keywords
* Recommendations
* Strengths

---

# 6. Resume Upload Rules

Accepted Types:

* PDF
* DOCX

Maximum Size:

10 MB

Validation:

* File type validation
* File size validation
* Duplicate detection

Duplicate Detection:

SHA256 hash comparison.

Behavior:

Warn user if duplicate exists.

Allow upload.

---

# 7. Cover Letter Rules

Required Inputs:

* Job Title
* Company Name
* Job Description

Optional Inputs:

* Resume
* Tone

Supported Tones:

* Professional
* Friendly
* Confident
* Executive

Regeneration:

Consumes quota.

Editing:

Users may edit generated content.

Store:

* Original AI version
* Latest edited version

---

# 8. Authentication Rules

Authentication Methods:

* Email / Password
* Google OAuth

Password Requirements:

* Minimum 12 characters
* One uppercase
* One lowercase
* One number

Email Verification:

Required before ATS Analysis or Cover Letter Generation.

JWT Lifetime:

15 minutes

Refresh Token Lifetime:

30 days

Refresh Tokens must rotate on use.

---

# 9. API Endpoints

## Auth

POST /api/auth/register

POST /api/auth/login

POST /api/auth/refresh

POST /api/auth/logout

POST /api/auth/verify-email

POST /api/auth/forgot-password

POST /api/auth/reset-password

---

## Profile

GET /api/profile

PUT /api/profile

---

## Resumes

POST /api/resumes

GET /api/resumes

GET /api/resumes/{id}

DELETE /api/resumes/{id}

---

## ATS Analysis

POST /api/analyses

GET /api/analyses

GET /api/analyses/{id}

---

## Cover Letters

POST /api/coverletters

POST /api/coverletters/{id}/regenerate

GET /api/coverletters

GET /api/coverletters/{id}

PUT /api/coverletters/{id}

DELETE /api/coverletters/{id}

---

## Subscriptions

GET /api/subscriptions/current

POST /api/subscriptions/upgrade

POST /api/subscriptions/cancel

GET /api/subscriptions/history

---

## Admin

GET /api/admin/users

GET /api/admin/analytics

GET /api/admin/auditlogs

---

# 10. Quota Rules

Free Plan:

Resume Storage:
5 resumes

ATS Analysis:
10 per month

Cover Letters:
10 per month

Premium:

Unlimited

Quota Validation:

Quota must be checked before any AI operation.

---

# 11. AI Integration

All AI operations must use:

IAiProvider

Supported Operations:

* Analyze Resume
* Generate Cover Letter

Tracked Metrics:

* Tokens Used
* Processing Time
* Estimated Cost

Store all usage events.

---

# 12. Error Handling

Business errors return:

HTTP 400

Authorization errors:

HTTP 403

Authentication errors:

HTTP 401

Not Found:

HTTP 404

Unexpected failures:

HTTP 500

Response Format:

{
"success": false,
"errors": [
"message"
]
}

---

# 13. Logging

Log:

* UserId
* Endpoint
* Request Duration
* AI Usage
* Payment Events

Do Not Log:

* Resume Content
* Passwords
* Tokens
* Personally Sensitive Data

---

# 14. Acceptance Criteria

A feature is complete when:

* Endpoint implemented
* Validator implemented
* Handler implemented
* Unit tests implemented
* Integration tests implemented
* Swagger documented
* Logging implemented
* Authorization implemented
* Build passes

End of Specification.
