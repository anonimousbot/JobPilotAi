# AGENTS.md

# JobPilotAi Backend Engineering Guide

This file contains the engineering rules, architecture decisions, coding conventions, and domain knowledge for AI coding agents working on the ApplyAI backend.

All generated code must follow this document unless explicitly overridden.

---

# Overview

ApplyAI is an AI-powered career platform that helps job seekers:

* Analyze resumes
* Improve ATS compatibility
* Generate tailored cover letters
* Track job applications
* Prepare for interviews

Primary goal:

Reduce the time required to submit high-quality job applications while increasing interview conversion rates.

---

# Product Principles

When making implementation decisions prioritize:

1. Simplicity over cleverness
2. Shipping over perfection
3. Readability over abstraction
4. Business value over technical purity
5. Developer velocity over premature scalability

Avoid enterprise architecture unless there is a measurable need.

---

# Tech Stack

## Runtime

* .NET 10
* ASP.NET Core Minimal APIs

## Database

* PostgreSQL
* EF Core 10
* snake_case naming convention

## Authentication

* ASP.NET Core Identity
* JWT Access Tokens
* Refresh Token Rotation

## Validation

* FluentValidation

## Logging

* Serilog
* Structured Logging

## Observability

* OpenTelemetry

## Documentation

* Swagger/OpenAPI

## Containerization

* Docker
* Docker Compose

## Background Processing

* Hosted Services

Do not introduce Hangfire, MassTransit, RabbitMQ, or Azure Service Bus during MVP.

---

# Architecture

## Style

Modular Monolith

## Pattern

Vertical Slice Architecture

## Approach

Feature-first organization.

Code should be grouped by use case, not technical layer.

Example:

Features/
UploadResume/
AnalyzeResume/
GenerateCoverLetter/
RegisterUser/

NOT:

Controllers/
Services/
Repositories/

---

# Project Structure

src/

ApplyAI.Api/

ApplyAI.Domain/

ApplyAI.Infrastructure/

ApplyAI.Tests/

ApplyAI.ArchitectureTests/

ApplyAI.IntegrationTests/

---

# Feature Structure

Features/

UploadResume/

UploadResume.Endpoint.cs

UploadResume.Handler.cs

UploadResume.Validator.cs

UploadResume.Response.cs

UploadResume.Mapping.cs

---

# Code Conventions

## DTOs

Use positional records.

Example:

public record UploadResumeRequest(
IFormFile File
);

---

## Dependency Injection

Use primary constructors.

Example:

internal sealed class UploadResumeHandler(
ApplicationDbContext dbContext,
IResumeParser parser)
{
}

---

## Visibility

Internal by default.

Use public only when required.

---

## Namespaces

Use file-scoped namespaces.

---

## Async

All database and external operations must be asynchronous.

---

# Patterns We Use

## Result Pattern

Business operations return Result<T>.

Never throw exceptions for validation failures.

Example:

Result<ResumeAnalysisResponse>

---

## Direct DbContext Usage

Handlers use EF Core DbContext directly.

No repository pattern.

---

## Manual Mapping

Use extension methods.

Do not use AutoMapper.

---

## Feature-Based Validation

Each feature owns its validator.

---

## Route Constants

Centralize routes.

Example:

RouteConsts.Resume.Upload

---

# Patterns We Do NOT Use

* Repository Pattern
* Unit Of Work Pattern
* AutoMapper
* MediatR
* CQRS Libraries
* Service Locator
* Static Service Classes
* Generic CRUD Services
* Fat Base Controllers
* Exceptions for business logic

---

# Domain Language

Business terminology must remain consistent.

| Business Term   | Code Entity    |
| --------------- | -------------- |
| User            | User           |
| Resume          | Resume         |
| Resume Analysis | ResumeAnalysis |
| ATS Score       | AtsScore       |
| Cover Letter    | CoverLetter    |
| Job Application | JobApplication |
| Subscription    | Subscription   |
| Credit Usage    | UsageRecord    |

Never invent alternate names.

---

# Core Modules

## Identity

Responsibilities:

* Registration
* Authentication
* Authorization
* Refresh Tokens

---

## Resume Module

Responsibilities:

* Upload resumes
* Store resumes
* Parse resumes
* Manage resume history

---

## Analysis Module

Responsibilities:

* ATS scoring
* Resume feedback
* Keyword analysis
* Skill gap detection

---

## Cover Letter Module

Responsibilities:

* Generate cover letters
* Store generation history

---

## Billing Module

Responsibilities:

* Subscription management
* Usage tracking
* Payment integration

---

# AI Integration Rules

All AI requests must go through:

IAiProvider

Never call OpenAI SDK directly from handlers.

Benefits:

* Easier testing
* Easier provider replacement
* Centralized cost tracking

---

# Cost Protection

Every AI operation must:

1. Record usage
2. Record token count
3. Record execution time

Future billing depends on this data.

---

# Security Rules

Never store:

* Plain text passwords
* API keys
* Access tokens

Always:

* Hash sensitive data
* Validate uploads
* Limit file sizes
* Scan file types

---

# Resume Upload Rules

Allowed:

* PDF
* DOCX

Maximum Size:

10 MB

Rejected files must return meaningful errors.

---

# API Design

## Success

{
"success": true,
"data": {}
}

## Failure

{
"success": false,
"errors": []
}

All endpoints follow this structure.

---

# Testing

## Unit Tests

Framework:

* xUnit

Mocking:

* Moq

Naming:

Method_Scenario_ExpectedResult

Example:

GenerateCoverLetter_ValidInput_ReturnsLetter

---

## Integration Tests

Use:

* Testcontainers
* PostgreSQL

Never use InMemoryDatabase for integration tests.

---

# Logging

Log:

* User Id
* Request Duration
* Endpoint
* AI Usage

Do not log:

* Resume contents
* Personal user information
* Access tokens

---

# Future Architecture

When growth requires scaling:

1. Extract AI Module
2. Extract Billing Module
3. Extract Notifications Module

Do not split into microservices before 1000+ active users.

---

# Build Commands

dotnet restore

dotnet build

dotnet test

dotnet ef database update

docker compose up -d

---

# Success Definition

A change is complete when:

* Builds successfully
* Tests pass
* Swagger documentation is updated
* Logging is implemented
* Validation exists
* Result pattern is respected
* No architecture violations are introduced

End of document.
