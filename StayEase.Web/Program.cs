using StayEase.Infrastructure.Data; 
using Microsoft.EntityFrameworkCore;  
using StayEase.Web.Components;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<StayEaseDbContext>(options =>
    options.UseSqlite("Data Source=stay_ease.db"));

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();