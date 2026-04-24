namespace AIPortfolioGenerator.Models;

public class PortfolioData
{
    public string FullName { get; set; } = "";
    public string Title { get; set; } = "";
    public string Email { get; set; } = "";
    public string GitHubUrl { get; set; } = "";
    public string LinkedInUrl { get; set; } = "";
    public string About { get; set; } = "";
    public string ProfileImage { get; set; } = "";
    public List<Project> Projects { get; set; } = new();
    public List<string> Skills { get; set; } = new();
    public string SelectedTheme { get; set; } = "minimal";
    public ThemeColors CustomColors { get; set; } = new();
    public string LayoutStyle { get; set; } = "standard";
    public bool AIEnhanced { get; set; } = false;
}

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Technologies { get; set; } = "";
    public string GitHubUrl { get; set; } = "";
    public string DemoUrl { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public List<string> SuggestedSkills { get; set; } = new();
}

public class ThemeColors
{
    public string Primary { get; set; } = "#6366f1";
    public string Secondary { get; set; } = "#8b5cf6";
    public string Background { get; set; } = "#ffffff";
    public string Text { get; set; } = "#1f2937";
    public string Accent { get; set; } = "#f59e0b";
}

public class AISuggestion
{
    public string Type { get; set; } = "";
    public string Content { get; set; } = "";
    public List<string> SuggestedSkills { get; set; } = new();
    public string LayoutRecommendation { get; set; } = "";
}

public class ThemeOption
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public ThemeColors Colors { get; set; } = new();
    public bool IsDark { get; set; } = false;
}
