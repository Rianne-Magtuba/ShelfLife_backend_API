
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

            const string jwtKeySecretPath = "/secrets-jwt/jwt-key";
            if (File.Exists(jwtKeySecretPath))
            {
                var jwtKey = File.ReadAllText(jwtKeySecretPath).Trim();
                builder.Configuration["Jwt:Key"] = jwtKey;
            }

            const string emailPasswordSecretPath = "/secrets-email/app-password";
            if (File.Exists(emailPasswordSecretPath))
            {
                var emailPassword = File.ReadAllText(emailPasswordSecretPath).Trim();
                builder.Configuration["EmailSettings:AppPassword"] = emailPassword;
            }
            var firestoreOptions = builder.Configuration
            .GetSection(FirestoreOptions.SectionName)
            .Get<FirestoreOptions>();

            const string cloudRunSecretPath = "/secrets/firebase-sa";

            string firebasePath = null;

            // Cloud Run
            if (File.Exists(cloudRunSecretPath))
            {
                firebasePath = cloudRunSecretPath;
            }
            // Local fallback (same as Firestore DI)
            else if (!string.IsNullOrEmpty(firestoreOptions?.CredentialsPath)
                     && File.Exists(firestoreOptions.CredentialsPath))
            {
                firebasePath = firestoreOptions.CredentialsPath;
            }

            // Final safety check
            if (firebasePath == null)
            {
                throw new Exception("Firebase credentials not found (Cloud Run or local)");
            }

            // Initialize FirebaseApp only once
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(firebasePath)
                });
            }


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

            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection("Jwt"));

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
                options.DefaultAuthenticateScheme = "CustomJwt";
                options.DefaultChallengeScheme = "CustomJwt";
            })
            .AddJwtBearer("CustomJwt", options =>
            {
                var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

                if (jwt == null || string.IsNullOrEmpty(jwt.Key))
                    throw new Exception("JWT settings are missing or invalid");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,

                    ValidateAudience = false,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.Key)
                    ),

                    ValidateLifetime = true
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("CustomAuth", policy =>
                    policy.AddAuthenticationSchemes("CustomJwt")
                          .RequireAuthenticatedUser());
            });

            var app = builder.Build();

      
     
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
