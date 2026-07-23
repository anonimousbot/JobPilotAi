using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using JobPilotAi_Backend.Modules.Resumes;
using Microsoft.Extensions.Options;

namespace JobPilotAi_Backend.Core.Ai;

public sealed class OpenAiProvider(
    HttpClient httpClient,
    IOptions<OpenAiOptions> options,
    ILogger<OpenAiProvider> logger) : IAiProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly OpenAiOptions _options = options.Value;

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

        var stopwatch = Stopwatch.StartNew();
        var response = await CreateResponseAsync(
            "You are an expert ATS resume reviewer. Return only strict JSON that matches the requested schema.",
            prompt,
            CreateAnalysisSchema(),
            cancellationToken);
        stopwatch.Stop();

        var analysis = DeserializeStructuredContent<OpenAiAnalysisResponse>(response.OutputText);
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

        var stopwatch = Stopwatch.StartNew();
        var response = await CreateResponseAsync(
            "You write tailored cover letters for job seekers. Return only strict JSON that matches the requested schema.",
            prompt,
            CreateCoverLetterSchema(),
            cancellationToken);
        stopwatch.Stop();

        var coverLetter = DeserializeStructuredContent<OpenAiCoverLetterResponse>(response.OutputText);

        return new CoverLetterDraft(
            coverLetter.Content.Trim(),
            CreateUsageMetrics(response.Usage, stopwatch.Elapsed));
    }

    private async Task<OpenAiChatResponse> CreateResponseAsync(
        string instructions,
        string input,
        object schema,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Ai:OpenAI:ApiKey must be configured before OpenAI requests can be executed.");
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = JsonContent.Create(new
            {
                model = _options.Model,
                messages = new[]
                {
                    new { role = "system", content = instructions },
                    new { role = "user", content = input }
                },
                response_format = new
                {
                    type = "json_schema",
                    json_schema = new
                    {
                        name = "jobpilotai_response",
                        strict = true,
                        schema
                    }
                }
            }, options: JsonOptions)
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await httpClient.SendAsync(request, timeout.Token);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "OpenAI request failed. StatusCode: {StatusCode}, Body: {Body}",
                (int)response.StatusCode,
                body);

            throw new InvalidOperationException($"OpenAI request failed with status {(int)response.StatusCode}.");
        }

        var envelope = JsonSerializer.Deserialize<OpenAiChatResponse>(body, JsonOptions);
        if (envelope is null)
        {
            throw new InvalidOperationException("OpenAI returned an empty response.");
        }

        var outputText = envelope.OutputText;
        if (string.IsNullOrWhiteSpace(outputText))
        {
            throw new InvalidOperationException("OpenAI returned no output text.");
        }

        return envelope;
    }

    private AiUsageMetrics CreateUsageMetrics(OpenAiChatUsage? usage, TimeSpan elapsed)
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
        var result = JsonSerializer.Deserialize<T>(json, JsonOptions);
        return result ?? throw new InvalidOperationException("OpenAI returned JSON that could not be parsed.");
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

    private static object CreateAnalysisSchema() => new
    {
        type = "object",
        additionalProperties = false,
        properties = new Dictionary<string, object>
        {
            ["keywordMatchScore"] = ScoreSchema(),
            ["skillsCoverageScore"] = ScoreSchema(),
            ["formattingScore"] = ScoreSchema(),
            ["experienceScore"] = ScoreSchema(),
            ["educationScore"] = ScoreSchema(),
            ["weaknesses"] = StringArraySchema(),
            ["recommendations"] = StringArraySchema(),
            ["missingKeywords"] = StringArraySchema(),
            ["strengths"] = StringArraySchema()
        },
        required = new[]
        {
            "keywordMatchScore",
            "skillsCoverageScore",
            "formattingScore",
            "experienceScore",
            "educationScore",
            "weaknesses",
            "recommendations",
            "missingKeywords",
            "strengths"
        }
    };

    private static object CreateCoverLetterSchema() => new
    {
        type = "object",
        additionalProperties = false,
        properties = new Dictionary<string, object>
        {
            ["content"] = new { type = "string" }
        },
        required = new[] { "content" }
    };

    private static object ScoreSchema() => new
    {
        type = "integer",
        minimum = 0,
        maximum = 100
    };

    private static object StringArraySchema() => new
    {
        type = "array",
        items = new { type = "string" }
    };

    private sealed record OpenAiChatResponse(
        [property: JsonPropertyName("choices")] IReadOnlyCollection<OpenAiChoice> Choices,
        [property: JsonPropertyName("usage")] OpenAiChatUsage? Usage)
    {
        public string OutputText => Choices.FirstOrDefault()?.Message.Content ?? string.Empty;
    }

    private sealed record OpenAiChoice(
        [property: JsonPropertyName("message")] OpenAiChatMessage Message);

    private sealed record OpenAiChatMessage(
        [property: JsonPropertyName("content")] string Content);

    private sealed record OpenAiChatUsage(
        [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
        [property: JsonPropertyName("completion_tokens")] int CompletionTokens);

    private sealed record OpenAiAnalysisResponse(
        int KeywordMatchScore,
        int SkillsCoverageScore,
        int FormattingScore,
        int ExperienceScore,
        int EducationScore,
        IReadOnlyCollection<string> Weaknesses,
        IReadOnlyCollection<string> Recommendations,
        IReadOnlyCollection<string> MissingKeywords,
        IReadOnlyCollection<string> Strengths);

    private sealed record OpenAiCoverLetterResponse(string Content);
}
