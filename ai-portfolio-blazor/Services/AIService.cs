using AIPortfolioGenerator.Models;
using System.Text.Json;

namespace AIPortfolioGenerator.Services;

public class AIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIService> _logger;
    private readonly string _model;

    public AIService(HttpClient httpClient, ILogger<AIService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _model = configuration["Ollama:Model"] ?? "llama3.2";

        var apiKey = Environment.GetEnvironmentVariable("OLLAMA_API_KEY") ?? configuration["Ollama:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        }
    }

    private async Task<string> GenerateWithOllamaAsync(string systemPrompt, string userPrompt)
    {
        try
        {
            var request = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                stream = false
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/chat", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Ollama API returned {StatusCode}", response.StatusCode);
                return $"Ollama unavailable (status: {response.StatusCode}). Is Ollama running?";
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            if (doc.RootElement.TryGetProperty("message", out var message) &&
                message.TryGetProperty("content", out var contentProp))
            {
                return contentProp.GetString() ?? "";
            }

            return "No response from Ollama";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to connect to Ollama");
            return "Could not connect to Ollama Cloud. Check your internet connection and API key.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Ollama");
            return "An error occurred while generating content.";
        }
    }

    public async Task<string> GenerateAboutAsync(string name, string title, List<Project> projects)
    {
        var projectList = projects.Any()
            ? string.Join("\n", projects.Select(p => $"- {p.Title}: {p.Technologies}"))
            : "No projects added yet";

        var systemPrompt = "You are a professional portfolio writer. Keep responses concise (2-3 sentences), professional, and in first person.";
        var userPrompt = $"Write a compelling 'About Me' paragraph for a portfolio. Name: {name}, Title: {title}, Projects: {projectList}. Return ONLY the paragraph, no headers or explanations.";

        return await GenerateWithOllamaAsync(systemPrompt, userPrompt);
    }

    public async Task<string> EnhanceDescriptionAsync(string title, string description, string technologies)
    {
        var systemPrompt = "You are a professional tech writer. Keep descriptions concise and impactful. Return ONLY the enhanced description, no explanations.";
        var userPrompt = string.IsNullOrWhiteSpace(description)
            ? $"Write a concise description for a project titled '{title}' using technologies: {technologies}"
            : $"Enhance this project description to be more professional and impactful: '{description}'. Keep it concise.";

        return await GenerateWithOllamaAsync(systemPrompt, userPrompt);
    }

    public async Task<List<string>> SuggestSkillsAsync(List<Project> projects)
    {
        var techList = projects
            .SelectMany(p => p.Technologies.Split(',').Select(t => t.Trim()))
            .Distinct()
            .ToList();

        var systemPrompt = "You are a tech career advisor. Suggest relevant skills as a comma-separated list (max 8 skills). Return ONLY the list, no explanations.";
        var userPrompt = $"Based on these technologies: {string.Join(", ", techList)}, suggest relevant professional skills for a portfolio.";

        var response = await GenerateWithOllamaAsync(systemPrompt, userPrompt);

        return response
            .Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Take(8)
            .ToList();
    }

    public async Task<string> RecommendLayoutAsync(PortfolioData data)
    {
        var systemPrompt = "You are a UI/UX advisor. Return ONLY one word: 'focused', 'grid', or 'masonry'.";
        var userPrompt = $"Suggest the best portfolio layout for someone with {data.Projects.Count} projects. Reply with just the layout name.";

        var response = (await GenerateWithOllamaAsync(systemPrompt, userPrompt)).ToLowerInvariant();

        if (response.Contains("grid")) return "grid";
        if (response.Contains("masonry")) return "masonry";
        return "focused";
    }

    public async Task<ThemeOption> GenerateThemeAsync(string mood)
    {
        var systemPrompt = "You are a design advisor. Generate a theme based on the mood provided. Return ONLY a JSON object with keys: id, name, description, primary (hex color), secondary (hex), background (hex), text (hex), accent (hex), isDark (true/false). No markdown.";
        var userPrompt = $"Create a professional portfolio theme for mood: '{mood}'. Return JSON only.";

        var response = await GenerateWithOllamaAsync(systemPrompt, userPrompt);

        try
        {
            var json = response.Trim();
            if (json.StartsWith("```"))
            {
                var lines = json.Split('\n');
                json = string.Join("\n", lines.Skip(1).Take(lines.Length - 2));
            }
            using var doc = JsonDocument.Parse(json);

            return new ThemeOption
            {
                Id = doc.RootElement.GetProperty("id").GetString() ?? "ai-theme",
                Name = doc.RootElement.GetProperty("name").GetString() ?? "AI Theme",
                Description = doc.RootElement.GetProperty("description").GetString() ?? "",
                Colors = new ThemeColors
                {
                    Primary = doc.RootElement.GetProperty("primary").GetString() ?? "#3b82f6",
                    Secondary = doc.RootElement.GetProperty("secondary").GetString() ?? "#6366f1",
                    Background = doc.RootElement.GetProperty("background").GetString() ?? "#ffffff",
                    Text = doc.RootElement.GetProperty("text").GetString() ?? "#1e293b",
                    Accent = doc.RootElement.GetProperty("accent").GetString() ?? "#f97316"
                },
                IsDark = doc.RootElement.TryGetProperty("isDark", out var isDark) && isDark.GetBoolean()
            };
        }
        catch
        {
            return new ThemeOption
            {
                Id = "ai-theme",
                Name = "AI Theme",
                Description = $"Theme based on: {mood}",
                Colors = new ThemeColors
                {
                    Primary = "#3b82f6",
                    Secondary = "#6366f1",
                    Background = "#ffffff",
                    Text = "#1e293b",
                    Accent = "#f97316"
                }
            };
        }
    }

    public async Task<AISuggestion> GetComprehensiveSuggestionsAsync(PortfolioData data)
    {
        var skills = await SuggestSkillsAsync(data.Projects);
        var layout = await RecommendLayoutAsync(data);

        var systemPrompt = "You are a portfolio advisor. Give one concrete, actionable suggestion. Be specific to the person's content.";
        var userPrompt = $"Analyze this portfolio: {data.FullName}, a {data.Title} with {data.Projects.Count} projects. Give ONE specific improvement suggestion.";

        var suggestion = await GenerateWithOllamaAsync(systemPrompt, userPrompt);

        return new AISuggestion
        {
            Type = "Comprehensive",
            Content = suggestion,
            SuggestedSkills = skills,
            LayoutRecommendation = layout
        };
    }
}