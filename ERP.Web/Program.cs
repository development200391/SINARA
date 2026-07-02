using ERP.Web.Filters;
using ERP.Web.Models;
using ERP.Web.Services;
using ERP.Web.Services.Exports;
using ERP.Web.Services.Localization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllersWithViews(options =>
    {
        options.Filters.AddService<AutoRequireMenuPermissionFilter>();
    })
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Insert(0, "/Views/HR/{1}/{0}.cshtml");
        options.ViewLocationFormats.Insert(0, "/Views/Config/{1}/{0}.cshtml");
        options.ViewLocationFormats.Insert(0, "/Views/Inventory/{1}/{0}.cshtml");
    });
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(ApiSettings.SectionName));

builder.Services.AddTransient<CorrelationIdHandler>();

builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
    var baseUrl = options.BaseUrl?.TrimEnd('/') ?? "http://localhost:8081";
    client.BaseAddress = new Uri($"{baseUrl}/");
}).AddHttpMessageHandler<CorrelationIdHandler>();

builder.Services.AddHttpClient<IConfigApiClient, ConfigApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
    var baseUrl = options.BaseUrl?.TrimEnd('/') ?? "http://localhost:8081";
    client.BaseAddress = new Uri($"{baseUrl}/");
}).AddHttpMessageHandler<CorrelationIdHandler>();

builder.Services.AddScoped<ITextLocalizer, TextLocalizer>();
builder.Services.AddScoped<AutoRequireMenuPermissionFilter>();
builder.Services.AddScoped<IEmployeePhotoService, EmployeePhotoService>();
builder.Services.AddScoped<IMenuPermissionService, MenuPermissionService>();
builder.Services.AddScoped<IAuditLogExcelExportService, AuditLogExcelExportService>();

builder.Services.AddHttpClient<IHrApiClient, HrApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
    var baseUrl = options.BaseUrl?.TrimEnd('/') ?? "http://localhost:8081";
    client.BaseAddress = new Uri($"{baseUrl}/");
}).AddHttpMessageHandler<CorrelationIdHandler>();

builder.Services.AddHttpClient<IFinanceApiClient, FinanceApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
    var baseUrl = options.BaseUrl?.TrimEnd('/') ?? "http://localhost:8081";
    client.BaseAddress = new Uri($"{baseUrl}/");
}).AddHttpMessageHandler<CorrelationIdHandler>();

builder.Services.AddHttpClient<IInventoryApiClient, InventoryApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
    var baseUrl = options.BaseUrl?.TrimEnd('/') ?? "http://localhost:8081";
    client.BaseAddress = new Uri($"{baseUrl}/");
}).AddHttpMessageHandler<CorrelationIdHandler>();

builder.Services.AddHttpClient<IPurchasingApiClient, PurchasingApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
    var baseUrl = options.BaseUrl?.TrimEnd('/') ?? "http://localhost:8081";
    client.BaseAddress = new Uri($"{baseUrl}/");
}).AddHttpMessageHandler<CorrelationIdHandler>();

builder.Services.AddHttpClient<ISalesApiClient, SalesApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
    var baseUrl = options.BaseUrl?.TrimEnd('/') ?? "http://localhost:8081";
    client.BaseAddress = new Uri($"{baseUrl}/");
}).AddHttpMessageHandler<CorrelationIdHandler>();

builder.Services.AddHttpClient<IManufacturingApiClient, ManufacturingApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
    var baseUrl = options.BaseUrl?.TrimEnd('/') ?? "http://localhost:8081";
    client.BaseAddress = new Uri($"{baseUrl}/");
}).AddHttpMessageHandler<CorrelationIdHandler>();

builder.Services.AddHttpClient<IApprovalApiClient, ApprovalApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
    var baseUrl = options.BaseUrl?.TrimEnd('/') ?? "http://localhost:8081";
    client.BaseAddress = new Uri($"{baseUrl}/");
}).AddHttpMessageHandler<CorrelationIdHandler>();

builder.Services.AddHttpClient<IFixedAssetsApiClient, FixedAssetsApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
    var baseUrl = options.BaseUrl?.TrimEnd('/') ?? "http://localhost:8081";
    client.BaseAddress = new Uri($"{baseUrl}/");
}).AddHttpMessageHandler<CorrelationIdHandler>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/auth/login";
        options.AccessDeniedPath = "/auth/access-denied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseMiddleware<ERP.Web.Middleware.CorrelationIdMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (ApiUnauthorizedException)
    {
        if (context.Response.HasStarted)
        {
            throw;
        }

        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var returnUrl = $"{context.Request.Path}{context.Request.QueryString}";
        var loginUrl = $"/auth/login?returnUrl={Uri.EscapeDataString(returnUrl)}";
        context.Response.Redirect(loginUrl);
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


