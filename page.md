# JobPilotAI UI/UX Prompt For Stitch

Create a modern, polished web application UI for JobPilotAI, an AI-powered career platform that helps job seekers upload resumes, improve ATS compatibility, generate tailored cover letters, manage their profile, and manage subscriptions.

Important: preserve the current frontend direction. The product should feel focused, practical, trustworthy, and quietly premium. Avoid generic SaaS purple dashboards. Use a confident career-tech visual direction: deep ink, warm ivory, electric teal or signal green accents, document textures, score rings, soft shadows, and clear progress states.

Design for desktop first with excellent responsive mobile layouts. The authenticated app should use a clean sidebar/dashboard shell, strong cards, quota and plan messaging, and clear empty/loading/error states.

## Product Summary

JobPilotAI helps users:

- Create an account.
- Complete a career profile and onboarding details.
- Upload PDF or DOCX resumes.
- Run ATS analysis against a resume and optional job description.
- Review ATS score, strengths, weaknesses, recommendations, and missing keywords.
- Generate tailored cover letters from a resume, role, company, job description, and tone.
- Edit, regenerate, copy, save, and delete cover letters.
- Manage plan/subscription information.

## Current Frontend Pages

These are the pages currently available in the frontend and should be treated as the source of truth for the MVP UI.

### Landing Page `/`

Goal: convert job seekers.

Sections:

- Hero: "Apply smarter. Land interviews faster."
- Subheadline explaining resume analysis, cover letters, and application support.
- Primary CTA to `/register`.
- Secondary CTA for product walkthrough.
- Product preview using ATS score cards, resume upload tile, cover letter preview, quota pill, and animated floating document elements.
- Benefits: ATS scoring, tailored cover letters, resume history, application readiness.
- Workflow: Upload resume -> Analyze fit -> Improve resume -> Generate cover letter -> Apply with confidence.
- Trust section: privacy, secure uploads, no public resume sharing.

Motion direction:

- Use tasteful 3D floating resume/document cards.
- Use reveal-on-load animation, card lift, subtle sheen, and score movement.
- Keep motion helpful and premium, not noisy.

### Register Page `/register`

Goal: create a user account and start onboarding.

Fields:

- Full name.
- Email.
- Password.
- Terms checkbox.

Behavior:

- Submit to `POST /api/auth/register`.
- Automatically login with `POST /api/auth/login`.
- Save session locally for MVP.
- Save initial name to `PUT /api/profile`.
- Redirect to `/onboarding/career-goals`.

States:

- Loading state while account is being created.
- Password guidance.
- Validation error alert.
- Link to future `/login` page.

### Login Page `/login`

Goal: sign users back into their workspace.

Connected endpoints:

- `POST /api/auth/login`

UI:

- AI credit balance bar.
- Editorial value-prop panel.
- Social login visual buttons, marked as coming soon until OAuth exists.
- Email/password login form.
- Remember-me checkbox for MVP session preference.
- Link to future `/forgot-password`.
- Link to `/register`.

Behavior:

- Saves access token and refresh token to the MVP local session store.
- Redirects to `/dashboard` after successful login.

### Dashboard `/dashboard`

Goal: show the user's career command center.

Current status:

- UI exists.
- Backend dashboard summary endpoint does not exist yet.
- Keep dashboard data as mock/demo or derive from future list endpoints.

Suggested cards:

- ATS score trend.
- Resume count.
- Cover letter count.
- Plan/quota status.
- Recent activity.
- Next recommended action.

### Onboarding Career Goals `/onboarding/career-goals`

Goal: collect career goal and target role.

Current status:

- UI exists.
- Should connect to `PUT /api/profile/onboarding`.

Fields:

- Career goal.
- Target role.
- Career level if included in flow.

### Onboarding Experience `/onboarding/experience`

Goal: collect career level and experience details.

Current status:

- UI exists.
- Should connect to `PUT /api/profile/onboarding`.

Fields:

- Career level.
- Experience summary or selected level.
- Optional target role confirmation.

### Onboarding Resume Upload `/onboarding/resume-upload`

Goal: introduce the user to uploading their first resume.

Current status:

- UI exists.
- The fully connected resume upload is currently on `/resume/analyze`.
- This page should either call `POST /api/resumes` directly or link users into `/resume/analyze`.

Rules:

- Allow PDF and DOCX only.
- Max size 10 MB.
- Show clear upload progress and file validation messages.

### Resume Analyze `/resume/analyze`

Goal: manage resumes and start an ATS analysis.

Connected endpoints:

- `GET /api/resumes`
- `POST /api/resumes`
- `DELETE /api/resumes/{id}`
- `POST /api/analyses`

UI:

- Sidebar app shell.
- Upload dropzone.
- Resume list/table.
- Optional job description textarea.
- Analyze CTA.
- Empty state when no resume exists.
- Error and success alerts.

### Resume Results `/resume/results/[analysisId]`

Goal: show ATS analysis results.

Connected endpoints:

- `GET /api/analyses/{id}`

UI:

- ATS score hero.
- Score breakdown cards.
- Strengths.
- Weaknesses.
- Missing keywords.
- Recommendations.
- CTA to generate a cover letter at `/cover-letter/new`.
- CTA to open the detailed deep-dive at `/resume/deep-dive/[analysisId]`.

Special behavior:

- `demo` analysis id may render demo content.

### ATS Deep-Dive `/resume/deep-dive/[analysisId]`

Goal: provide an advanced ATS analysis workspace for users who want more detail than the summary results page.

Connected endpoints:

- `GET /api/analyses/{id}`

UI:

- Sidebar app shell.
- Quota warning bar.
- Animated overall match score ring.
- Quick fixes card.
- Tabbed deep-dive sections for keywords, formatting, and content impact.
- Keyword found/missing panels.
- Keyword density heatmap.
- Formatting compatibility table.
- AI rewrite suggestion card.

Special behavior:

- `demo` analysis id renders demo content.

### Job Match Explorer `/job-matches`

Goal: help users browse high-potential job opportunities and compare them against their profile.

Connected endpoints:

- `GET /api/jobmatches`
- `POST /api/jobmatches/search`
- `POST /api/jobmatches/{id}/compare`
- `POST /api/jobmatches/{id}/save`
- `DELETE /api/jobmatches/{id}/save`

Current status:

- Frontend page exists and is wired to the backend.
- Search/compare uses the backend MVP match catalog until an external job-board integration is added.

UI:

- Sidebar app shell.
- Quota/AI credit warning bar.
- Search, filter, and sort controls.
- 3D animated job cards with match scores and skill chips.
- Quick Compare slide-in modal.
- AI optimization bento card.
- Featured opportunity card.

### New Cover Letter `/cover-letter/new`

Goal: generate a tailored cover letter.

Connected endpoints:

- `GET /api/resumes`
- `POST /api/coverletters`

Fields:

- Resume selector.
- Job title.
- Company name.
- Job description.
- Tone selector.

States:

- Loading resumes.
- Generating cover letter.
- Validation error.
- Success redirect to `/cover-letter/[documentId]`.

### Cover Letter Editor `/cover-letter/[documentId]`

Goal: read, edit, copy, save, and regenerate a cover letter.

Connected endpoints:

- `GET /api/coverletters/{id}`
- `PUT /api/coverletters/{id}`
- `POST /api/coverletters/{id}/regenerate`

UI:

- Cover letter editor.
- Save button.
- Regenerate button.
- Copy button.
- Metadata panel with job title, company, tone, creation/update date.

Special behavior:

- `demo` document id may render demo content.

### Profile `/profile`

Goal: manage the user's career profile.

Connected endpoints:

- `GET /api/profile`
- `PUT /api/profile`

Fields:

- First name.
- Last name.
- Phone number.
- Location.
- LinkedIn URL.
- Portfolio URL.
- Career goal.
- Target role.
- Career level.

### Settings `/settings`

Goal: manage account/session and subscription.

Connected endpoints:

- `GET /api/subscriptions/current`
- `GET /api/subscriptions/history`
- `POST /api/subscriptions/upgrade`
- `POST /api/subscriptions/cancel`

Current behavior:

- Logout clears local frontend session.
- Backend `POST /api/auth/logout` is not wired yet.

## Backend Endpoints

### Auth

- `POST /api/auth/register` - connected on `/register`.
- `POST /api/auth/login` - connected on `/login` and used after registration.
- `POST /api/auth/refresh` - not connected yet.
- `POST /api/auth/logout` - not connected yet.
- `POST /api/auth/verify-email` - not connected yet.
- `POST /api/auth/forgot-password` - no frontend page yet.
- `POST /api/auth/reset-password` - no frontend page yet.

### Profile

- `GET /api/profile` - connected on `/profile`.
- `PUT /api/profile` - connected on `/profile` and partially after registration.
- `PUT /api/profile/onboarding` - should be connected to onboarding pages.

### Resumes

- `POST /api/resumes` - connected on `/resume/analyze`.
- `GET /api/resumes` - connected on `/resume/analyze` and `/cover-letter/new`.
- `GET /api/resumes/{id}` - no dedicated frontend detail page yet.
- `DELETE /api/resumes/{id}` - connected on `/resume/analyze`.

### ATS Analyses

- `POST /api/analyses` - connected on `/resume/analyze`.
- `GET /api/analyses` - no analysis history/list page yet.
- `GET /api/analyses/{id}` - connected on `/resume/results/[analysisId]`.

### Cover Letters

- `POST /api/coverletters` - connected on `/cover-letter/new`.
- `POST /api/coverletters/{id}/regenerate` - connected on `/cover-letter/[documentId]`.
- `GET /api/coverletters` - no cover letter library/list page yet.
- `GET /api/coverletters/{id}` - connected on `/cover-letter/[documentId]`.
- `PUT /api/coverletters/{id}` - connected on `/cover-letter/[documentId]`.
- `DELETE /api/coverletters/{id}` - no delete UI yet.

### Subscriptions

- `GET /api/subscriptions/current` - connected on `/settings`.
- `POST /api/subscriptions/upgrade` - connected on `/settings`.
- `POST /api/subscriptions/cancel` - connected on `/settings`.
- `GET /api/subscriptions/history` - connected on `/settings`.

### Admin

- `GET /api/admin/users` - connected on `/admin/users`.
- `GET /api/admin/analytics` - connected on `/admin/analytics`.
- `GET /api/admin/auditlogs` - connected on `/admin/analytics`.

### Health

- `GET /health` - no frontend status page needed for MVP.
- `GET /health/ready` - no frontend status page needed for MVP.

## Endpoints Still Needing Frontend Work

Highest priority:

- Token refresh handling using `POST /api/auth/refresh`.
- Real logout using `POST /api/auth/logout`.
- Email verification flow using `POST /api/auth/verify-email`.
- Forgot/reset password pages using `POST /api/auth/forgot-password` and `POST /api/auth/reset-password`.
- Connect onboarding pages to `PUT /api/profile/onboarding`.

Medium priority:

- Analysis history page using `GET /api/analyses`.
- Cover letter library page using `GET /api/coverletters`.
- Cover letter delete action using `DELETE /api/coverletters/{id}`.
- Resume detail drawer/page using `GET /api/resumes/{id}`.
- External job board integration for `/job-matches`.

Later:

- Dashboard summary endpoint or frontend composition from existing list endpoints.
- Admin mutation endpoints for creating, suspending, or editing users.
- Health/status page if needed.

## Shared Components To Design

- App shell with sidebar and topbar.
- Auth card layout.
- Upload dropzone.
- Resume card/list row.
- ATS score ring.
- Score breakdown card.
- AI generation progress panel.
- Empty state card.
- Error alert.
- Email verification banner.
- Quota/plan badge.
- Subscription cards.
- Confirmation modal.
- Data table.
- Cover letter editor.
- Toast notifications.

## Visual Direction

Use an editorial career-product aesthetic:

- Background: warm off-white, graphite, or soft gradient surfaces.
- Primary dark: ink, navy, charcoal.
- Accent: teal, green, amber, or signal blue for action and scoring.
- Avoid purple as the dominant color.
- Typography: modern, professional, slightly editorial.
- Cards: subtle borders, calm shadows, document-inspired surfaces.
- Charts: circular score rings, progress bars, keyword chips.
- Motion: page entrance, floating 3D documents, generation shimmer, subtle card lift.

## Key Empty States

- No resumes: "Upload your first resume to start improving your applications."
- No analyses: "Run an ATS analysis to see how your resume performs."
- No cover letters: "Generate your first tailored cover letter in under a minute."
- No subscription history: "Your plan history will appear here."
- Admin no audit logs: "No admin activity has been recorded yet."

## Key Error States

- Validation error: show field-level messages.
- Unauthorized: route to login.
- Email not verified: show verification CTA.
- Free resume limit reached: show upgrade CTA.
- AI provider unavailable: show retry state and reassurance.
- Not found: show friendly 404 with route back to relevant list.

## Suggested User Flow

1. User lands on marketing page.
2. User registers.
3. User verifies email when email provider is implemented.
4. User completes onboarding/profile.
5. User uploads resume.
6. User runs ATS analysis with optional job description.
7. User reviews recommendations and missing keywords.
8. User generates a cover letter for the same job.
9. User edits and copies the cover letter.
10. User manages subscription from settings.

## Implementation Notes

- Store access token and refresh token locally for MVP only.
- Use `Authorization: Bearer {accessToken}` for protected endpoints.
- Set `NEXT_PUBLIC_API_BASE_URL=http://localhost:5217` for local frontend development.
- Keep current frontend design language and improve only the pages that already exist.
- Use CSS-driven 3D/motion first to avoid unnecessary bundle weight. Add GSAP later only where timeline animation is clearly worth it.
