using CSSWENGxGK.Data;
using CSSWENGxGK.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Web.Http;
using Hangfire;
using Hangfire.Storage;
using Hangfire.SqlServer;
using System.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure your DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity with your User and Role classes
builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2); // Set session timeout to 2 hours
    options.Cookie.HttpOnly = true; // Make the session cookie HTTP-only
    options.Cookie.IsEssential = true; // Mark the session cookie as essential
    options.Cookie.Name = "UserCookie"; // Set the name of the session cookie
});


var serviceCollection = new ServiceCollection();
var serviceProvider = builder.Services.BuildServiceProvider();
var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();


// Create admin role if it doesn't exist
var adminRoleExists = await roleManager.RoleExistsAsync("Admin");
if (!adminRoleExists)
{
    await roleManager.CreateAsync(new IdentityRole<int>("Admin"));
}

// Create user role if it doesn't exist
var userRoleExists = await roleManager.RoleExistsAsync("User");
if (!userRoleExists)
{
    await roleManager.CreateAsync(new IdentityRole<int>("User"));
}

Hangfire.GlobalConfiguration.Configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"));

var Active_Checker = new ActiveChecker();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Use session middleware
app.UseSession(); // Add this line to enable sessions

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

RecurringJob.AddOrUpdate(() => Active_Checker.PerformDatabaseCheck(), Cron.Daily);

// Start Hangfire server in the background
using (var server = new BackgroundJobServer())
{
    app.Run();
}
