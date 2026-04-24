using AIPortfolioGenerator.Models;

namespace AIPortfolioGenerator.Services;

public class PortfolioStateService
{
    public PortfolioData Data { get; private set; } = new();
    public event Action? OnChange;

    public void SetPortfolio(PortfolioData data)
    {
        Data = data;
        NotifyStateChanged();
    }

    public void UpdateBasicInfo(string fullName, string title, string email, string github, string linkedIn, string about)
    {
        Data.FullName = fullName;
        Data.Title = title;
        Data.Email = email;
        Data.GitHubUrl = github;
        Data.LinkedInUrl = linkedIn;
        Data.About = about;
        NotifyStateChanged();
    }

    public void SetProfileImage(string base64Image)
    {
        Data.ProfileImage = base64Image;
        NotifyStateChanged();
    }

    public void SetProjectImage(Guid projectId, string base64Image)
    {
        var project = Data.Projects.FirstOrDefault(p => p.Id == projectId);
        if (project != null)
        {
            project.ImageUrl = base64Image;
            NotifyStateChanged();
        }
    }

    public void AddProject(Project project)
    {
        Data.Projects.Add(project);
        NotifyStateChanged();
    }

    public void RemoveProject(Guid id)
    {
        Data.Projects.RemoveAll(p => p.Id == id);
        NotifyStateChanged();
    }

    public void UpdateProject(Project project)
    {
        var existing = Data.Projects.FirstOrDefault(p => p.Id == project.Id);
        if (existing != null)
        {
            var index = Data.Projects.IndexOf(existing);
            Data.Projects[index] = project;
            NotifyStateChanged();
        }
    }

    public void SetTheme(string themeId, ThemeColors? colors = null)
    {
        Data.SelectedTheme = themeId;
        if (colors != null)
        {
            Data.CustomColors = colors;
        }
        NotifyStateChanged();
    }

    public void SetSkills(List<string> skills)
    {
        Data.Skills = skills;
        NotifyStateChanged();
    }

    public void SetLayout(string layout)
    {
        Data.LayoutStyle = layout;
        NotifyStateChanged();
    }

    public void MarkAIEnhanced()
    {
        Data.AIEnhanced = true;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
