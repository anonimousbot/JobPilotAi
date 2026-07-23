# JobPilotAi Backend & Frontend

JobPilotAi is an AI-powered career platform designed to help job seekers optimize their resumes, analyze ATS compatibility, generate tailored cover letters, and track applications. This repository contains the backend modular monolith API built on .NET 10 and ASP.NET Core, alongside the premium React-based frontend dashboard.

---

## Technical Stack & Architecture

### Backend
* **Runtime**: .NET 10 & ASP.NET Core (Minimal APIs)
* **Database**: PostgreSQL with EF Core 10 (Snake Case Conventions)
* **Architecture**: Modular Monolith organized with a Feature-first **Vertical Slice Architecture**.
* **AI Provider**: Custom `GeminiProvider` implementing `IAiProvider` with built-in failover to Nvidia NIM Hosted APIs.
* **Text Extraction**: High-performance binary parser using `PdfPig` for PDF parsing and XML zip decoding for Word (`.docx`) file formatting.
* **Logging**: Serilog structured logging.
* **Testing**: xUnit, Moq, and Testcontainers.

### Frontend
* **Core**: React & TypeScript with Vite
* **Styling**: Premium custom CSS system featuring modern responsive grids, fluid typography, glassmorphism card layouts, and dynamic micro-animations.
* **ATS UI**: Renders mathematically weighted match formula percentages to clearly show how formatting, experience, keywords, skills, and education contribute to the final applicant tracking score.

---

## Configuration & Security

To protect your API keys and prevent exposing secrets in source control, API keys should be configured locally using ASP.NET Core **User Secrets**.

### Setup User Secrets
Open your terminal in the `JobPilotAi_Backend` project folder and run:

```bash
# Set your primary Google Gemini API Key
dotnet user-secrets set "Ai:Gemini:ApiKey" "YOUR_GOOGLE_GEMINI_API_KEY"

# Set your fallback Nvidia NIM API Key
dotnet user-secrets set "Ai:Gemini:FallbackApiKey" "YOUR_NVIDIA_API_KEY"
```

The system will read configuration values from User Secrets in development mode, merging them securely with your `appsettings.Development.json` values.

---

## Running the Application

### Prerequisites
* .NET 10 SDK
* PostgreSQL Server
* Node.js & npm

### Running the Backend
1. Navigate to the backend directory:
   ```bash
   cd JobPilotAi_Backend
   ```
2. Build the project:
   ```bash
   dotnet build
   ```
3. Run the development server:
   ```bash
   dotnet run
   ```
   The API will bind to `http://localhost:5217` by default with Swagger interactive documentation accessible at `http://localhost:5217/swagger`.

### Running the Frontend
1. Navigate to the frontend directory:
   ```bash
   cd JobPilotAiFrontend/jobpilotaifrontend
   ```
2. Install dependencies:
   ```bash
   npm install
   ```
3. Start the Vite dev server:
   ```bash
   npm run dev
   ```
   Open your browser to the URL displayed in the terminal (usually `http://localhost:5173`).
