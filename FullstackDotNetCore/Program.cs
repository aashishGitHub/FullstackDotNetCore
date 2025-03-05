using DbUp;
using FullstackDotNetCore.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using FullstackDotNetCore.Authorization;
using System.Security.Claims;

namespace FullstackDotNetCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
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
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<IDataRepository, DataRepository>();
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<IQuestionCache, QuestionCache>();

            // Configure authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    // In development, accept any token and set up mock authentication
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            // Always succeed and set up mock identity
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
                            // Don't fail in development, just set up mock identity
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
                else 
                {
                    // Production JWT validation settings
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = false
                    };
                }
            });

            // Configure authorization
            builder.Services.AddAuthorization(options =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    // In development, make all policies pass
                    options.DefaultPolicy = new AuthorizationPolicyBuilder()
                        .RequireAssertion(_ => true)
                        .Build();
                    
                    options.AddPolicy("MustBeQuestionAuthor", policy => 
                        policy.RequireAssertion(_ => true));
                }
                else 
                {
                    // Production authorization policies
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
            app.UseRouting();
            app.UseCors(MyAllowSpecificOrigins);

            // Keep authentication/authorization middleware for both environments
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
} 