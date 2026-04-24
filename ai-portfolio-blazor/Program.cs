using AIPortfolioGenerator.Components;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using AIPortfolioGenerator.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBlazorise(options =>
{
    options.Immediate = true;
    options.Debounce = true;
})
.AddBootstrap5Providers()
.AddFontAwesomeIcons();

builder.Services.AddSingleton<PortfolioStateService>();
builder.Services.AddHttpClient<AIService>(client =>
{
    client.BaseAddress = new Uri("https://ollama.com");
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
