using Api.Middleware;
using Api.Swagger;
using Api.Validators;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Core.Interfaces;
using Core.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            if (!builder.Environment.IsDevelopment())
            {
                string? keyVaultUrl = builder.Configuration["KeyVaultUrl"];

                if (!string.IsNullOrEmpty(keyVaultUrl))
                {
                    var secretClient = new SecretClient(
                            new Uri(keyVaultUrl),
                            new DefaultAzureCredential()
                        );

                    builder.Configuration.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
                }
            }

            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<PasswordHasher>();
            builder.Services.AddScoped<JwtService>();
            builder.Services.AddScoped<IPerfumeService, PerfumeService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<IPerfumeVoteService, PerfumeVoteService>();
            builder.Services.AddScoped<IPerfumeListService, PerfumeListService>();
            builder.Services.AddScoped<ISharedListService, SharedListService>();


            if (!builder.Environment.IsDevelopment())
            {
                builder.Services.AddDbContext<FragranceLogContext>(options =>
                    options.UseSqlServer(
                        builder.Configuration.GetConnectionString("FragranceLog"),
                        sqlOptions =>
                        {
                            sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 3,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorNumbersToAdd: null
                            );
                        }
                    )
                );
            }
            else
            {
                builder.Services.AddDbContext<FragranceLogContext>(options =>
                    options.UseSqlServer(
                        builder.Configuration.GetConnectionString("FragranceLogLocal")
                    )
                );
            }

            var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "FragranceLog API",
                    Version = "v1"
                });

                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header
                });

                c.OperationFilter<AuthorizeOperationFilter>();
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowClient",
                    policy =>
                    {
                        policy.AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowAnyOrigin();
                    });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                        Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
                    RequestPath = ""
                });
            }

            app.UseMiddleware<ErrorHandlerMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowClient");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<JwtMiddleware>();

            app.MapControllers();

            app.Run();
        }
    }
}
