using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TimesheetGeneratorApp.Data;

using Microsoft.AspNetCore.Localization;
using System.Globalization;

var LOCALE_IDN = "in-ID";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<HariLiburContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("HariLiburContext") ?? throw new InvalidOperationException("Connection string 'HariLiburContext' not found.")));
builder.Services.AddDbContext<CommitContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CommitContext") ?? throw new InvalidOperationException("Connection string 'CommitContext' not found.")));
builder.Services.AddDbContext<MasterProjectContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MasterProjectContext") ?? throw new InvalidOperationException("Connection string 'MasterProjectContext' not found.")));

builder.Services.AddLocalization();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var supportedCultures = new[]{
    new CultureInfo(LOCALE_IDN)
};
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(LOCALE_IDN),
    SupportedCultures = supportedCultures,
    FallBackToParentCultures = false
});
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture(LOCALE_IDN);


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=masterproject}/{action=Index}/{id?}");

app.Run();