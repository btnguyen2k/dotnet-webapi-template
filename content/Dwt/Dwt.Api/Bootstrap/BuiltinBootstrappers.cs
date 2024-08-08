using Dwt.Api.Helpers;
using Dwt.Api.Middleware;
using Dwt.Api.Models;
using Dwt.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Dwt.Api.Bootstrap;

/// <summary>
/// Built-in main application bootstrapper.
/// </summary>
public class ApplicationBootstrapper : IApplicationBootstrapper
{
    private readonly ILogger<ApplicationBootstrapper> logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<ApplicationBootstrapper>();

    /// <summary>
    /// Prepares and configures database services.
    /// </summary>
    /// <param name="builder"></param>
    protected void ConfigureDbContext(WebApplicationBuilder builder)
    {
        var services = builder.Services;

        logger.LogInformation("Configuring DbContext services...");

        services.AddSingleton<IUserRepository, StaticConfigUserRepository>();
        logger.LogInformation("StaticConfigUserRepository --> IUserRepository.");

        var tryParse = Boolean.TryParse(Environment.GetEnvironmentVariable(GlobalVars.ENV_INIT_DB), out var initDb);

        services.AddDbContext<ITodoRepository, TodoDbContextRepository>(options =>
        {
            options.UseInMemoryDatabase("TodoList");
            if (builder.Environment.IsDevelopment() || (tryParse && initDb))
            {
                DbContextOptionsBuilder<TodoDbContextRepository> optBuilder = (DbContextOptionsBuilder<TodoDbContextRepository>)options;
                using var dbContext = new TodoDbContextRepository(optBuilder.Options);
                dbContext.Database.EnsureCreated();
                logger.LogInformation("EnsureCreated() is called for TodoDbContextRepository.");
            }
        });
        logger.LogInformation("TodoDbContextRepository --> ITodoRepository.");

        services.AddDbContext<INoteRepository, NoteDbContextRepository>(options =>
        {
            var connStr = builder.Configuration.GetConnectionString("NotesDbContext");
            options.UseSqlite(connStr);
            if (builder.Environment.IsDevelopment() || (tryParse && initDb))
            {
                DbContextOptionsBuilder<NoteDbContextRepository> optBuilder = (DbContextOptionsBuilder<NoteDbContextRepository>)options;
                using var dbContext = new NoteDbContextRepository(optBuilder.Options);
                dbContext.Database.EnsureCreated();
                logger.LogInformation("EnsureCreated() is called for NoteDbContextRepository.");
            }
        });
        logger.LogInformation("NoteDbContextRepository --> INoteRepository.");
    }

    /// <summary>
    /// Prepares and configures controllers for APIs.
    /// </summary>
    /// <param name="builder"></param>
    protected void ConfigureControllers(WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        // services.AddEndpointsApiExplorer(); // required only for minimal APIs
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "DWT Backend API",
                Description = "Dotnet WebAPI Template Backend.",
                // TermsOfService = new Uri("https://example.com/terms"),
                Contact = new OpenApiContact
                {
                    Name = "GitHub Repo",
                    Url = new Uri("https://github.com/btnguyen2k/dotnet-webapi-template")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT - License",
                    Url = new Uri("https://github.com/btnguyen2k/dotnet-webapi-template/blob/main/LICENSE.md")
                }
            });

            // Define the OAuth2.0 scheme that's in use (i.e., Implicit Flow)
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                   new OpenApiSecurityScheme
                   {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                   },
                   new List<string>()
                }
            });

            // https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-8.0&tabs=visual-studio-code#xml-comments
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });
    }

    /// <inheritdoc />
    public WebApplication CreateApplication(WebApplicationBuilder builder)
    {
        logger.LogInformation("Creating WebApplication instance...");
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        ConfigureDbContext(builder);
        ConfigureControllers(builder);
        var app = builder.Build();
        GlobalVars.App = app;
        logger.LogInformation("WebApplication instance created and added to GlobalVars.");
        return app;
    }

    protected void ConfigureSwagger(WebApplication app)
    {
        var tryParse = Boolean.TryParse(Environment.GetEnvironmentVariable(GlobalVars.ENV_ENABLE_SWAGGER_UI), out var enableSwaggerUi);
        if (app.Environment.IsDevelopment() || (tryParse && enableSwaggerUi))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            logger.LogInformation("Swagger UI enabled at /swagger");
        }
    }

    protected void ConfigureMiddlewares(WebApplication app)
    {
        app.UseExceptionHandler(o => { }) //workaround for https://github.com/dotnet/aspnetcore/issues/51888
            .UseMiddleware<JwtMiddleware>();
    }

    /// <summary>
    /// Bootstraps services defined in configuration.
    /// </summary>
    /// <param name="app"></param>
    /// <returns>List of background bootstrapping tasks, if any.</returns>
    protected List<Task> BootstrapServices(WebApplication app)
    {
        var logger = app.Logger;
        logger.LogInformation("Start bootstrapping services...");
        var bootstrapperNames = app.Configuration.GetSection("Bootstrap:Components").Get<List<String>>() ?? [];
        var asyncBootstrapTasks = new List<Task>();
#pragma warning disable CA2254 // Template should be a static expression
        foreach (var bootstrapperName in bootstrapperNames)
        {
            app.Logger.LogInformation($"Loading bootstrapper: {bootstrapperName}...");

            var bootstrapperType = Type.GetType(bootstrapperName);
            if (bootstrapperType == null)
            {
                app.Logger.LogWarning($"Bootstrapper not found: {bootstrapperName}");
                continue;
            }

            if (bootstrapperType.IsAssignableTo(typeof(IBootstrapper)))
            {
                var bootstrapper = ReflectionHelper.CreateInstance<IBootstrapper>(app.Services, bootstrapperType);
                if (bootstrapper == null)
                {
                    app.Logger.LogWarning($"Bootstrapper not found: {bootstrapperName}");
                    continue;
                }
                bootstrapper.Bootstrap(app);
            }
            else if (bootstrapperType.IsAssignableTo(typeof(IAsyncBootstrapper)))
            {
                var bootstrapper = ReflectionHelper.CreateInstance<IAsyncBootstrapper>(app.Services, bootstrapperType);
                if (bootstrapper == null)
                {
                    app.Logger.LogWarning($"Bootstrapper not found: {bootstrapperName}");
                    continue;
                }
                asyncBootstrapTasks.Add(bootstrapper.BootstrapAsync(app));
            }
            else
            {
                app.Logger.LogError($"Bootstrapper {bootstrapperName} does not implement IBootstrapper or IAsyncBootstrapper");
            }
        }
#pragma warning restore CA2254 // Template should be a static expression
        return asyncBootstrapTasks;
    }

    /// <inheritdoc />
    public List<Task> InitializeApplication(WebApplication app)
    {
        ConfigureSwagger(app);
        ConfigureMiddlewares(app);

        // app.UseHttpsRedirection();

        // app.UseAuthorization();

        app.MapControllers();

        return BootstrapServices(app);
    }
}

/// <summary>
/// Built-in bootstrapper that initializes Cryptography keys.
/// </summary>
public class CryptoKeysBootstrapper(ILogger<CryptoKeysBootstrapper> logger, IConfiguration config) : IBootstrapper
{
    public void Bootstrap(WebApplication app)
    {
        logger.LogInformation("Initializing Cryptography keys...");

        RSA? privKey = null;
        var rsaPfxFile = config["Keys:RSAPFXFile"];
        var rsaPrivKeyFile = config["Keys:RSAPrivKeyFile"];

        if (!string.IsNullOrWhiteSpace(rsaPfxFile))
        {
            // load RSA private key from PFX file if available
            logger.LogInformation($"Loading RSA private key from PFX file: {rsaPfxFile}...");
            var rsaPfxPassword = config["Keys:RSAPFXPassword"] ?? "";
            using var cert = new X509Certificate2(rsaPfxFile, rsaPfxPassword);
            privKey = cert.GetRSAPrivateKey() ?? throw new InvalidDataException($"Failed to load RSA private key from PFX file: {rsaPfxFile}");
        }
        else if (!string.IsNullOrWhiteSpace(rsaPrivKeyFile))
        {
            // load RSA private key from PEM file if available
            logger.LogInformation($"Loading RSA private key from file: {rsaPrivKeyFile}...");
            var rsaPrivKey = File.ReadAllText(rsaPrivKeyFile);
            privKey = RSA.Create();
            privKey.ImportFromPem(rsaPrivKey);
        }
        else
        {
            // generate new RSA private key
            logger.LogInformation("Generating new RSA key...");
            privKey = RSA.Create(3072);
        }

        GlobalVars.RSAPrivKey = privKey;
        GlobalVars.RSAPubKey = RSA.Create(privKey.ExportParameters(false));

        logger.LogInformation("Cryptography keys initialized.");
    }
}


/// <summary>
/// Built-in bootstrapper that performs JWT-related intializing tasks.
/// Note: this bootstrapper requires access to the RSA private key. Hence, it should be initialized after the CryptoKeysBootstrapper.
/// </summary>
public class JwtBootstrapper(ILogger<JwtBootstrapper> logger, IConfiguration config) : IBootstrapper
{
    public void Bootstrap(WebApplication app)
    {
        logger.LogInformation("Initializing JWT...");

        JwtRepository.Issuer = config["Jwt:Issuer"] ?? "<not defined>";
        JwtRepository.Audience = config["Jwt:Audience"] ?? "http://localhost:8080";
        JwtRepository.DefaultExpirationSeconds = int.Parse(config["Jwt:Expiration"] ?? "3600");

        var key = config["Jwt:Key"]?.Trim() ?? "";
        if (key == "")
        {
            if (GlobalVars.RSAPrivKey == null)
            {
                throw new NullReferenceException("RSA private key is null.");
            }
            JwtRepository.Key = new RsaSecurityKey(GlobalVars.RSAPrivKey);
            JwtRepository.Algorithm = SecurityAlgorithms.RsaSha256Signature;
        }
        else
        {
            JwtRepository.Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            JwtRepository.Algorithm = SecurityAlgorithms.HmacSha256;
        }

        logger.LogInformation("JWT initialized.");
    }
}
