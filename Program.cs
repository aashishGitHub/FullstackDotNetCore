using DbUp;
using FullstackDotNetCore.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using FullstackDotNetCore.Authorization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add services to the container.
var connString = builder.Configuration.GetConnectionString("DefaultConnection");

EnsureDatabase.For.SqlDatabase(connString);

var upgrader = DeployChanges.To
    .SqlDatabase(connString, null)
    .WithScriptsEmbeddedInAssembly(
      System.Reflection.Assembly.GetExecutingAssembly()
    )
    .WithTransaction()
    .Build();
if (upgrader.IsUpgradeRequired())
{
    upgrader.PerformUpgrade();
}

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDataRepository, DataRepository>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IQuestionCache, QuestionCache>();

// Configure authentication first
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        if (builder.Environment.IsDevelopment())
        {
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, "mock-user-id"),
                        new Claim(ClaimTypes.Name, "mock@example.com"),
                        new Claim(ClaimTypes.Role, "Admin")
                    };
                    var identity = new ClaimsIdentity(claims, "Mock");
                    context.Principal = new ClaimsPrincipal(identity);
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    // In development, always succeed with mock identity
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, "mock-user-id"),
                        new Claim(ClaimTypes.Name, "mock@example.com"),
                        new Claim(ClaimTypes.Role, "Admin")
                    };
                    var identity = new ClaimsIdentity(claims, "Mock");
                    context.Principal = new ClaimsPrincipal(identity);
                    context.Success();
                    return Task.CompletedTask;
                }
            };
        }
    });

// Configure authorization
builder.Services.AddAuthorization(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .RequireAssertion(_ => true)
            .Build();
        
        options.AddPolicy("MustBeQuestionAuthor", policy => 
            policy.RequireAssertion(_ => true));
    }
    else 
    {
        options.AddPolicy("MustBeQuestionAuthor", policy =>
            policy.Requirements.Add(new MustBeQuestionAuthorRequirement()));
    }
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<IAuthorizationHandler, MustBeQuestionAuthorHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("*")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Correct middleware order
app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);

// Authentication and Authorization must be after UseRouting but before UseEndpoints
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

