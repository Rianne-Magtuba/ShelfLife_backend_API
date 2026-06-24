
using Business_Layer.Services;
using Business_Layer.Settings;
using Common_Class.Interfaces;
using Data_Layer.Configuration;
using Data_Layer.Extensions;
using Data_Layer.Services;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;


namespace ShellLife_backend_API

{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
           

            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
            // Add services to the container.

            // ... [Web builder initialization] ...

            try
            {
                // 1. Check JWT Secret
                const string jwtKeySecretPath = "/secrets-jwt/jwt-key";
                if (File.Exists(jwtKeySecretPath))
                {
                    var jwtKey = File.ReadAllText(jwtKeySecretPath).Trim();
                    builder.Configuration["Jwt:Key"] = jwtKey;
                }
                else
                {
                    Console.WriteLine("WARNING: JWT secret file not found at " + jwtKeySecretPath);
                }

                // 2. Check Email Secret
                const string emailPasswordSecretPath = "/secrets-email/app-password";
                if (File.Exists(emailPasswordSecretPath))
                {
                    var emailPassword = File.ReadAllText(emailPasswordSecretPath).Trim();
                    builder.Configuration["EmailSettings:AppPassword"] = emailPassword;
                }

                // 3. Check Firebase Secret
                var firestoreOptions = builder.Configuration.GetSection(FirestoreOptions.SectionName).Get<FirestoreOptions>();
                const string cloudRunSecretPath = "/secrets/firebase-sa";
                string firebasePath = null;

                if (File.Exists(cloudRunSecretPath))
                {
                    firebasePath = cloudRunSecretPath;
                }
                else if (!string.IsNullOrEmpty(firestoreOptions?.CredentialsPath) && File.Exists(firestoreOptions.CredentialsPath))
                {
                    firebasePath = firestoreOptions.CredentialsPath;
                }

                if (firebasePath == null)
                {
                    throw new Exception("CRITICAL: Firebase credentials not found (Cloud Run or local)");
                }

                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(firebasePath)
                    });
                }
            }
            catch (Exception ex)
            {
                // THIS is the magic part. We force it to print to standard output before dying.
                Console.WriteLine("================ STARTUP CRASH ================");
                Console.WriteLine($"ERROR MESSAGE: {ex.Message}");
                Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
                Console.WriteLine("===============================================");

                // Allow the app to exit cleanly with an error code rather than an abrupt Signal 6
                Environment.Exit(1);
            }

            // ... [Rest of your service registrations] ...


            builder.Services.AddControllers();
          

            builder.Services.AddFirestoreDatabase(builder.Configuration);
            builder.Services.AddScoped<IInventoryDataService, InventoryDataService>();
            builder.Services.AddScoped<inventoryLogicService>();
            builder.Services.AddScoped<IProductDataService, ProductDataService>();
            builder.Services.AddScoped<ProductLogicService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserDataService, UserDataService>();
            builder.Services.AddScoped<FirebaseAuthService>();
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddScoped<IProductUpdateDataService, ProductUpdateDataService>();
            builder.Services.AddScoped<ProductUpdateLogicService>();

            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection("Jwt"));

            var jwtTest = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

            builder.Services.Configure<EmailSettings>(
                builder.Configuration.GetSection("EmailSettings"));


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token like this: Bearer {token}"
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
                        new string[] {}
                    }
                });
            });

            
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "CustomJwt";
                options.DefaultAuthenticateScheme = "CustomJwt";
                options.DefaultChallengeScheme = "CustomJwt";
            })
            .AddJwtBearer("CustomJwt", options =>
            {
                var key = builder.Configuration["Jwt:Key"];
                var issuer = builder.Configuration["Jwt:Issuer"];

                if (string.IsNullOrWhiteSpace(key))
                    throw new Exception("JWT Key missing at runtime");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,

                    ValidateAudience = false,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(key)
                    ),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("CustomAuth", policy =>
                    policy.AddAuthenticationSchemes("CustomJwt")
                          .RequireAuthenticatedUser());
            });

            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            
            app.MapControllers();
            app.Run();
        }
    }
}
