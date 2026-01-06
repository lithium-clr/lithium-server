using Lithium.Server.Dashboard;
using Lithium.Server.Dashboard.Collections;
using Lithium.Server.Dashboard.Components;
using Lithium.Snowflake.Extensions;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var connectionString = builder.Configuration["Mongo:Uri"];
ArgumentException.ThrowIfNullOrEmpty(connectionString);

var client = new MongoClient(connectionString);
builder.Services.AddMongoDB<WebDbContext>(client, "web");

builder.Services.AddScoped<UserCollection>();

builder.Services.AddIdGen();

builder.Services.AddCascadingAuthenticationState();
// builder.Services.AddScoped<IdentityRedirectManager>();
// builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
//                        throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseSqlite(connectionString));
// builder.Services.AddDatabaseDeveloperPageExceptionFilter();
//
// builder.Services.AddIdentityCore<ApplicationUser>(options =>
//     {
//         options.SignIn.RequireConfirmedAccount = true;
//         options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
//     })
//     .AddEntityFrameworkStores<ApplicationDbContext>()
//     .AddSignInManager()
//     .AddDefaultTokenProviders();

// builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
// app.MapAdditionalIdentityEndpoints();

app.Run();