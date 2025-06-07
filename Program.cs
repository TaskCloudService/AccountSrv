using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Presentation.Interfaces;
using Presentation.Models;
using Presentation.Services;
using IEmailSender = Presentation.Interfaces.IEmailSender;
using Presentaion.Data;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;
var env = builder.Environment;

var connectionString = cfg.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Missing config: ConnectionStrings:Default");

var jwtKey = cfg["Jwt:SigningKey"]
    ?? throw new InvalidOperationException("Missing config: Jwt:SigningKey");

var emailConnString = cfg["Email:ConnectionString"]
    ?? throw new InvalidOperationException("Missing config: Email:ConnectionString");
var emailFrom = cfg["Email:From"]
    ?? throw new InvalidOperationException("Missing config: Email:From");

var sbConn = cfg["ServiceBus:ConnectionString"]
    ?? throw new InvalidOperationException("Missing config: ServiceBus:ConnectionString");
var sbTopic = cfg["ServiceBus:EmailVerifyTopic"]
    ?? throw new InvalidOperationException("Missing config: ServiceBus:EmailVerifyTopic");

builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(connectionString));

builder.Services.AddCors(o => o.AddPolicy("AllowFrontend", p => p
    .WithOrigins("http://localhost:3000", "https://localhost:3000")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
));

builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>(opts =>
    {
        opts.SignIn.RequireConfirmedEmail = true;
        opts.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Auth API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        }] = Array.Empty<string>()
    });
});

builder.Services.AddTransient<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IEmailSender, AzureEmailSender>();
builder.Services.AddSingleton(_ => new ServiceBusClient(sbConn));
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<ServiceBusClient>().CreateSender(sbTopic)
);
builder.Services.AddScoped<IVerificationService, VerificationService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddControllers();

var app = builder.Build();

if (env.IsDevelopment())
{
    app.Use(async (ctx, next) =>
    {
        Console.WriteLine($"→ {ctx.Request.Method} {ctx.Request.Path}");
        try { await next(); }
        catch (Exception ex)
        {
            Console.WriteLine("‼ Unhandled exception:");
            Console.WriteLine(ex);
            throw;
        }
    });

    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.Use(async (ctx, next) =>
    {
        Console.WriteLine($"→ {ctx.Request.Method} {ctx.Request.Path}");
        try { await next(); }
        catch (Exception ex)
        {
            Console.WriteLine("‼ Unhandled exception:");
            Console.WriteLine(ex);
            throw;
        }
    });

    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();

    endpoints.MapGet("/", () => Results.Text("Auth API online"));
});

app.Run();
