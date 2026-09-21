using System.Security.Claims;
using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Application.Services;
using Stylo.Backend.Stylo.Domain.Entities;
using Stylo.Backend.Stylo.Infrastructure.Configurations;
using Stylo.Backend.Stylo.Infrastructure.Data;
using Stylo.Backend.Stylo.Infrastructure.Repositories;
using Stylo.Backend.Stylo.Infrastructure.Caching;
using Stylo.Backend.Stylo.Infrastructure.Email;
using Stylo.Backend.Stylo.Application.Settings;
using Stylo.Backend.Stylo.Infrastructure.Services;

namespace Stylo.Backend.Stylo.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
            services.AddScoped<IImageService, CloudinaryImageService>();

            services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<AppDbContext>();

            services.AddSingleton<ILookupNormalizer, CaseSensitiveLookupNormalizer>();
            services.AddSingleton<ITokenManagerService, TokenManagerService>();

            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<IFavoriteRepository, FavoriteRepository>();
            services.AddScoped<IFavoriteService, FavoriteService>();

            services.AddScoped<IProductFeedbackRepository, ProductFeedbackRepository>();
            services.AddScoped<IProductFeedbackService, ProductFeedbackService>();

            services.AddScoped<IWebsiteFeedbackRepository, WebsiteFeedbackRepository>();
            services.AddScoped<IWebsiteFeedbackService, WebsiteFeedbackService>();

            services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICategoryService, CategoryService>();

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductService, ProductService>();

            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICartService, CartService>();

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderService, OrderService>();

            services.AddHttpContextAccessor();

            var secretKey = configuration["Jwt:SecretKey"] ?? "SuperSecretKeyForStyloBackendECommerceApp2026!";
            var issuer = configuration["Jwt:Issuer"] ?? "StyloAPI";
            var audience = configuration["Jwt:Audience"] ?? "StyloClient";

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.NameIdentifier
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var tokenManager = context.HttpContext.RequestServices.GetRequiredService<ITokenManagerService>();
                        var rawHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(rawHeader) && rawHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            var token = rawHeader.Substring("Bearer ".Length).Trim();
                            if (tokenManager.IsTokenInvalidated(token))
                            {
                                context.Fail("Token has been revoked.");
                            }
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"[JWT Auth Failed]: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine($"[JWT Auth Challenge]: Error={context.Error}, Description={context.ErrorDescription}");
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Stylo Backend API",
                    Version = "v1",
                    Description = "E-Commerce Backend API for Stylo Clothing Store"
                });

                // Include XML documentation from controllers and models
                options.IncludeXmlComments(Assembly.GetExecutingAssembly());

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Input your JWT token in this format: Bearer {your token}"
                });

                options.AddSecurityRequirement(document =>
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                    });
            });

            // Redis Cache 
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration["Redis:ConnectionString"];
                options.InstanceName = configuration["Redis:InstanceName"];
            });
            services.AddScoped<ICacheService, RedisCacheService>();

            //  OTP 
            services.Configure<OtpSettings>(configuration.GetSection("Otp"));
            services.AddScoped<IOtpService, OtpService>();

            // Email
            services.Configure<EmailSettings>(configuration.GetSection("Email"));
            services.AddScoped<IEmailService, SmtpEmailService>();

            return services;
        }
    }

    public class CaseSensitiveLookupNormalizer : ILookupNormalizer
    {
        public string? NormalizeEmail(string? email) => email;
        public string? NormalizeName(string? name) => name;
    }
}
