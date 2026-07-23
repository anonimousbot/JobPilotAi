# Observability

JobPilotAi uses Serilog as the application logger. Logs are structured so they can be searched by property instead of only by text.

## Local log destinations

- Console output is optimized for development.
- Rolling file logs are written to `JobPilotAi_Backend/logs/jobpilotai-.log`.
- File logs are retained for 14 days by default.

## Standard log properties

- `Application`: static service name.
- `CorrelationId`: request correlation id from `X-Correlation-ID`, or the ASP.NET trace id when the header is absent.
- `UserId`: authenticated user id when a valid bearer token is supplied.
- `UserRole`: authenticated user role when available.
- `EndpointName`: selected minimal API endpoint name.
- `RequestHost`, `RequestScheme`, and `RemoteIpAddress`: request routing context.

## Sensitive data rules

Do not log resume text, access tokens, refresh tokens, passwords, raw API keys, or full personal profile data. Prefer stable identifiers such as `UserId`, `ResumeId`, and hashed email values.

## Useful local checks

Run the API, call an endpoint, and check the latest log file:

```powershell
Get-Content .\JobPilotAi_Backend\logs\jobpilotai-*.log -Tail 20
```

Swagger is available at `/swagger` in development. XML documentation is generated during build and included in Swagger when present.
