using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WorkflowAutomation.Core.Interfaces;
using WorkflowAutomation.Engine.Extensions;
using WorkflowAutomation.Infrastructure.Data;
using WorkflowAutomation.Infrastructure.Repositories;
using WorkflowAutomation.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT authentication support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Workflow Automation API",
        Version = "v1",
        Description = "n8n-like workflow automation platform API"
    });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var jwtSecret = builder.Configuration["JWT:Secret"]
    ?? throw new InvalidOperationException("JWT:Secret is not configured");
var jwtIssuer = builder.Configuration["JWT:Issuer"] ?? "WorkflowAutomation";
var jwtAudience = builder.Configuration["JWT:Audience"] ?? "WorkflowAutomation";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
    };
});

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("RequireDeveloperRole", policy =>
        policy.RequireRole("Admin", "Developer"));

    options.AddPolicy("RequireViewerRole", policy =>
        policy.RequireRole("Admin", "Developer", "Viewer"));
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                    ?? new[] { "http://localhost:3000", "http://localhost:5173" })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IWorkflowRepository, WorkflowRepository>();
builder.Services.AddScoped<IExecutionRepository, ExecutionRepository>();
builder.Services.AddScoped<ICredentialRepository, CredentialRepository>();
builder.Services.AddScoped<IAIConfigurationRepository, AIConfigurationRepository>();
builder.Services.AddScoped<ICollaborationRepository, CollaborationRepository>();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

// Register Workflow Engine Components (Phase 6 & 7)
builder.Services.AddScoped<WorkflowAutomation.Core.Interfaces.IWorkflowExecutor, WorkflowAutomation.Engine.Executor.WorkflowExecutor>();
builder.Services.AddScoped<WorkflowAutomation.Core.Interfaces.IExecutionHub, WorkflowAutomation.Engine.Hubs.ExecutionHub>();

// Register Expression Evaluator (Phase 7)
builder.Services.AddScoped<WorkflowAutomation.Core.Interfaces.IExpressionEvaluator, WorkflowAutomation.Engine.Expressions.ExpressionEvaluator>();

// Register Node System (Phase 8)
builder.Services.AddSingleton<WorkflowAutomation.Core.Interfaces.INodeRegistry, WorkflowAutomation.Engine.Registry.NodeRegistry>();

// Register HttpClient for HttpRequestNode (Phase 9)
builder.Services.AddHttpClient();

// Register Built-in Nodes (Phase 9)
builder.Services.AddBuiltInNodes();

// Configure SignalR for real-time execution updates
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

// Authentication & Authorization middleware (order is important)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR hubs
app.MapHub<WorkflowAutomation.Engine.Hubs.ExecutionHub>("/hubs/execution");

// Register built-in nodes with NodeRegistry (Phase 9)
using (var scope = app.Services.CreateScope())
{
    var registry = scope.ServiceProvider.GetRequiredService<INodeRegistry>();
    registry.RegisterBuiltInNodes(scope.ServiceProvider);
}

app.Run();
