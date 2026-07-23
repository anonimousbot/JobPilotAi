using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using JobPilotAi_Backend.Modules.Resumes;
using Microsoft.Extensions.Options;

namespace JobPilotAi_Backend.Core.Ai;

public sealed class GeminiProvider(
    HttpClient httpClient,
    IOptions<GeminiOptions> options,
    ILogger<GeminiProvider> logger) : IAiProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly GeminiOptions _options = options.Value;

    public async Task<ResumeAnalysisDraft> AnalyzeResumeAsync(
        Resume resume,
        string? jobDescription,
        CancellationToken cancellationToken)
    {
        var prompt = $"""
            Analyze this resume for ATS compatibility.

            Resume metadata:
            File name: {resume.FileName}
            File type: {resume.FileType}
            File size bytes: {resume.FileSize}
            Extracted text, when available:
            {Truncate(resume.ExtractedText, 12000)}

            Target job description, when available:
            {Truncate(jobDescription, 12000)}
            """;

        var instructions = "You are an expert ATS resume reviewer. Return only strict JSON matching this schema: " +
            "{ \"keywordMatchScore\": integer (0-100), \"skillsCoverageScore\": integer (0-100), \"formattingScore\": integer (0-100), \"experienceScore\": integer (0-100), \"educationScore\": integer (0-100), \"weaknesses\": string[], \"recommendations\": string[], \"missingKeywords\": string[], \"strengths\": string[] }";

        var stopwatch = Stopwatch.StartNew();
        GeminiChatResponse response;
        try
        {
            response = await CreateResponseAsync(instructions, prompt, cancellationToken, useFallback: false);
        }
        catch (Exception ex) when (!string.IsNullOrWhiteSpace(_options.FallbackApiKey))
        {
            logger.LogWarning(ex, "Primary Gemini API failed. Attempting fallback to Nvidia NIM API.");
            response = await CreateResponseAsync(instructions, prompt, cancellationToken, useFallback: true);
        }
        stopwatch.Stop();

        var analysis = DeserializeStructuredContent<GeminiAnalysisResponse>(response.OutputText);
        var usage = CreateUsageMetrics(response.Usage, stopwatch.Elapsed);

        return new ResumeAnalysisDraft(
            ClampScore(analysis.KeywordMatchScore),
            ClampScore(analysis.SkillsCoverageScore),
            ClampScore(analysis.FormattingScore),
            ClampScore(analysis.ExperienceScore),
            ClampScore(analysis.EducationScore),
            JsonSerializer.Serialize(analysis.Weaknesses),
            JsonSerializer.Serialize(analysis.Recommendations),
            JsonSerializer.Serialize(analysis.MissingKeywords),
            JsonSerializer.Serialize(analysis.Strengths),
            usage);
    }

    public async Task<CoverLetterDraft> GenerateCoverLetterAsync(
        Resume? resume,
        string jobTitle,
        string companyName,
        string jobDescription,
        string tone,
        CancellationToken cancellationToken)
    {
        var prompt = $"""
            Generate a concise, high-quality cover letter.

            Job title: {jobTitle}
            Company: {companyName}
            Tone: {tone}
            Job description:
            {Truncate(jobDescription, 12000)}

            Resume context, when available:
            {Truncate(resume?.ExtractedText, 12000)}
            """;

        var instructions = "You write tailored cover letters for job seekers. Return only strict JSON matching this schema: { \"content\": string }";

        var stopwatch = Stopwatch.StartNew();
        GeminiChatResponse response;
        try
        {
            response = await CreateResponseAsync(instructions, prompt, cancellationToken, useFallback: false);
        }
        catch (Exception ex) when (!string.IsNullOrWhiteSpace(_options.FallbackApiKey))
        {
            logger.LogWarning(ex, "Primary Gemini API failed. Attempting fallback to Nvidia NIM API.");
            response = await CreateResponseAsync(instructions, prompt, cancellationToken, useFallback: true);
        }
        stopwatch.Stop();

        var coverLetter = DeserializeStructuredContent<GeminiCoverLetterResponse>(response.OutputText);

        return new CoverLetterDraft(
            coverLetter.Content.Trim(),
            CreateUsageMetrics(response.Usage, stopwatch.Elapsed));
    }

    private async Task<GeminiChatResponse> CreateResponseAsync(
        string instructions,
        string input,
        CancellationToken cancellationToken,
        bool useFallback)
    {
        var apiKey = useFallback ? _options.FallbackApiKey : _options.ApiKey;
        var baseUrl = useFallback ? _options.FallbackBaseUrl : _options.BaseUrl;
        var model = useFallback ? _options.FallbackModel : _options.Model;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(useFallback 
                ? "Ai:Gemini:FallbackApiKey must be configured for fallback execution."
                : "Ai:Gemini:ApiKey must be configured before Gemini requests can be executed.");
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        // Parse and build request URL correctly matching base URL pathing
        var cleanBaseUrl = baseUrl.TrimEnd('/') + "/";
        var requestUri = new Uri(new Uri(cleanBaseUrl), "chat/completions");

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = instructions },
                    new { role = "user", content = input }
                },
                response_format = new
                {
                    type = "json_object"
                }
            }, options: JsonOptions)
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await httpClient.SendAsync(request, timeout.Token);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Gemini request failed (Fallback: {Fallback}). StatusCode: {StatusCode}, Body: {Body}",
                useFallback,
                (int)response.StatusCode,
                body);

            throw new InvalidOperationException($"Gemini request failed with status {(int)response.StatusCode}.");
        }

        var envelope = JsonSerializer.Deserialize<GeminiChatResponse>(body, JsonOptions);
        if (envelope is null)
        {
            throw new InvalidOperationException("Gemini returned an empty response.");
        }

        var outputText = envelope.OutputText;
        if (string.IsNullOrWhiteSpace(outputText))
        {
            throw new InvalidOperationException("Gemini returned no output text.");
        }

        return envelope;
    }

    private AiUsageMetrics CreateUsageMetrics(GeminiChatUsage? usage, TimeSpan elapsed)
    {
        var inputTokens = usage?.PromptTokens ?? 0;
        var outputTokens = usage?.CompletionTokens ?? 0;
        var estimatedCost =
            inputTokens / 1_000_000m * _options.EstimatedInputCostPerMillionTokens
            + outputTokens / 1_000_000m * _options.EstimatedOutputCostPerMillionTokens;

        return new AiUsageMetrics(
            inputTokens + outputTokens,
            Math.Max(1, (int)elapsed.TotalMilliseconds),
            decimal.Round(estimatedCost, 6));
    }

    private static T DeserializeStructuredContent<T>(string json)
    {
        var cleaned = json.Trim();

        // Robustly extract the JSON payload between the first '{' and last '}'
        // to bypass any markdown wrappers (like ```json) or thinking/thought tags (<thinking>...</thinking>)
        var firstBrace = cleaned.IndexOf('{');
        var lastBrace = cleaned.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            cleaned = cleaned.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        var result = JsonSerializer.Deserialize<T>(cleaned, JsonOptions);
        return result ?? throw new InvalidOperationException("Gemini returned JSON that could not be parsed.");
    }

    private static int ClampScore(int score) => Math.Clamp(score, 0, 100);

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Not provided.";
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private sealed record GeminiChatResponse(
        [property: JsonPropertyName("choices")] IReadOnlyCollection<GeminiChoice> Choices,
        [property: JsonPropertyName("usage")] GeminiChatUsage? Usage)
    {
        public string OutputText => Choices.FirstOrDefault()?.Message.Content ?? string.Empty;
    }

    private sealed record GeminiChoice(
        [property: JsonPropertyName("message")] GeminiChatMessage Message);

    private sealed record GeminiChatMessage(
        [property: JsonPropertyName("content")] string Content);

    private sealed record GeminiChatUsage(
        [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
        [property: JsonPropertyName("completion_tokens")] int CompletionTokens);

    private sealed record GeminiAnalysisResponse(
        int KeywordMatchScore,
        int SkillsCoverageScore,
        int FormattingScore,
        int ExperienceScore,
        int EducationScore,
        IReadOnlyCollection<string> Weaknesses,
        IReadOnlyCollection<string> Recommendations,
        IReadOnlyCollection<string> MissingKeywords,
        IReadOnlyCollection<string> Strengths);

    private sealed record GeminiCoverLetterResponse(string Content);
}
