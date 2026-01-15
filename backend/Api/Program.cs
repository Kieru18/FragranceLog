using Api.Middleware;
using Api.Swagger;
using Api.Validators;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Core.Enums;
using Core.Interfaces;
using Core.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Data;
using Infrastructure.Helpers;
using Infrastructure.Services;
using Infrastructure.Services.InsightProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using PerfumeRecognition.Services;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using PerfumeRecognition.Interfaces;


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

            builder.Services.AddHttpClient();

            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

            builder.Services
                .AddControllers()
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.Converters.Add(
                        new KebabCaseEnumConverter<InsightIconEnum>()
                    );
                });

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IPerfumeService, PerfumeService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<IPerfumeVoteService, PerfumeVoteService>();
            builder.Services.AddScoped<IPerfumeListService, PerfumeListService>();
            builder.Services.AddScoped<ISharedListService, SharedListService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IPerfumeAnalyticsService, PerfumeAnalyticsService>();
            builder.Services.AddScoped<IGeoService, GeoService>();

            builder.Services.AddScoped<IHomeInsightProvider, CommunityMoodInsightProvider>();
            builder.Services.AddScoped<IHomeInsightProvider, TrendingPerfumeInsightProvider>();
            builder.Services.AddScoped<IHomeInsightProvider, GlobalTasteInsightProvider>();
            builder.Services.AddScoped<IHomeInsightProvider, BrandMomentumInsightProvider>();


            builder.Services.AddScoped<IHomeInsightProvider, PersonalRatingBiasInsightProvider>();
            builder.Services.AddScoped<IHomeInsightProvider, PersonalReviewActivityInsightProvider>();
            builder.Services.AddScoped<IHomeInsightProvider, RatingStyleInsightProvider>();
            builder.Services.AddScoped<IHomeInsightProvider, FavoriteBrandInsightProvider>();
            builder.Services.AddScoped<IHomeInsightProvider, TasteProfileInsightProvider>();

            builder.Services.AddScoped<IHomeInsightService, HomeInsightService>();

            builder.Services.AddSingleton<IBackgroundRemover>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                var solutionRoot = FindSolutionRoot();

                var modelPath = Path.GetFullPath(Path.Combine(
                    solutionRoot,
                    config["PerfumeRecognition:BackgroundRemoverModelPath"]!));

                var outputRoot = Path.Combine(
                    Path.GetTempPath(),
                    "bg-removed");

                return new BackgroundRemover(modelPath, outputRoot);
            });

            builder.Services.AddSingleton<IImageCropper, AlphaBoundingBoxCropper>();
            builder.Services.AddSingleton<IColorDescriptorExtractor, LabHistogramColorDescriptor>();

            var modelPath = Path.Combine(
                AppContext.BaseDirectory,
                "Assets",
                "resnet101_ap_gem.onnx");

            builder.Services.AddSingleton<IEmbeddingExtractor>(sp => new EmbeddingExtractor(modelPath));

            var embeddingsPath = Path.Combine(
                AppContext.BaseDirectory,
                "Embeddings",
                "embeddings.json");
            
            var embeddings = EmbeddingJsonLoader.Load(embeddingsPath);

            builder.Services.AddSingleton(new EmbeddingIndex(embeddings));

            builder.Services.AddSingleton<SimilaritySearch>();
            builder.Services.AddSingleton<PerfumeRecognition.Interfaces.IPerfumeRecognitionService, PerfumeRecognition.Services.PerfumeRecognitionService>();

            builder.Services.AddScoped<Core.Interfaces.IPerfumeRecognitionService, Infrastructure.Services.PerfumeRecognitionService>();


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

            ValidatorOptions.Global.LanguageManager = new FluentValidation.Resources.LanguageManager
            {
                Culture = new CultureInfo("en-US")
            };

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

            app.MapControllers();

            app.Run();
        }

        static string FindSolutionRoot()
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (dir != null)
            {
                if (dir.GetFiles("*.sln").Any())
                    return dir.FullName;

                dir = dir.Parent;
            }

            throw new InvalidOperationException("Solution root not found");
        }
    }
}
