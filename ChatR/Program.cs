using ChatR.Data;
using ChatR.Hosted;
using ChatR.Hubs;
using ChatR.Models.Constatns;
using ChatR.Models.Settings;
using ChatR.Repos;
using ChatR.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});

// http
builder.Services.AddControllers();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X‑XSRF‑Token";
    options.Cookie.Name = ".ChatR.Antiforgery";
    options.Cookie.HttpOnly = true;
#if DEBUG
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
#else
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
#endif
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.FormFieldName = "__RequestVerificationToken";
});

// явно указываем опции для TempData
builder.Services.Configure<CookieTempDataProviderOptions>(options =>
{
    options.Cookie.HttpOnly = true;
#if DEBUG
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
#else
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
#endif
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Name = ".ChatR.TempData";
});

// cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// background
builder.Services.AddHostedService<CleanupService>();

// auth
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = new JwtSettings();

        var key = Encoding.UTF8.GetBytes(jwtSettings.Secret!);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies[Auth.TOKEN_COOKIE_NAME];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

// repo
builder.Services.AddScoped<UserRepo>();
builder.Services.AddScoped<RoomRepo>();
builder.Services.AddScoped<MessageRepo>();
builder.Services.AddScoped<UserInRoomRepo>();

// services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<UserInRoomService>();
builder.Services.AddScoped<MessageService>();

// database
builder.Services.AddDbContext<ApplicationDbContext>(options => options
    .UseSnakeCaseNamingConvention()
    .UseNpgsql(Environment.GetEnvironmentVariable(Env.DB_CONN_ENV_NAME)));

// razor
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"./keys"))
    .SetApplicationName("ChatR")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(30));

builder.Services.AddRazorPages()
    .AddViewOptions(options =>
    {
        options.HtmlHelperOptions.ClientValidationEnabled = true;
    });

// signalr
builder.Services.AddSignalR(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(10);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.EnableDetailedErrors = true;
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseHsts();
app.UseHttpsRedirection();
app.UseForwardedHeaders();

app.UseSecurityHeaders(policies => policies
    // default
    .AddFrameOptionsDeny()
    .AddContentTypeOptionsNoSniff()
    .RemoveServerHeader()
    .AddContentSecurityPolicy(builder =>
    {
        builder.AddUpgradeInsecureRequests();

        builder.AddDefaultSrc()
            .Self();

        builder.AddImgSrc()
            .Self()
            .Data(); // for icons: "data:image/svg+xml"

        builder.AddScriptSrc()
            .Self()
            .WithNonce(); // for script type="importmap"
    })
    .AddCrossOriginOpenerPolicy(x => x.SameOrigin())
    .AddCrossOriginEmbedderPolicy(builder => builder.Credentialless())
    .AddCrossOriginResourcePolicy(builder => builder.SameSite())
    // modified default
    .AddReferrerPolicyNoReferrer()
    .AddStrictTransportSecurityMaxAgeIncludeSubDomains()
    // addinionally
    .AddPermissionsPolicyWithDefaultSecureDirectives());

app.UseRouting();
app.UseCors("AllowAll");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await context.Database.MigrateAsync();
}

// общее
app.UseCookiePolicy(new CookiePolicyOptions
{
#if DEBUG
    Secure = CookieSecurePolicy.SameAsRequest,
#else
    Secure = CookieSecurePolicy.Always,
#endif
    HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
    MinimumSameSitePolicy = SameSiteMode.Strict
});

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chatHub");

app.MapStaticAssets();
app.MapControllers();
app.MapRazorPages()
   .WithStaticAssets();

await app.RunAsync();
