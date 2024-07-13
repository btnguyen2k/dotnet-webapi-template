using dwt;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// builder.Services.AddDbContext<TodoContext>(opt => opt.UseInMemoryDatabase("TodoList"));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer(); // required only for minimal APIs
builder.Services.AddSwaggerGen(options =>
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

    // https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-8.0&tabs=visual-studio-code#xml-comments
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();
Global.App = app;

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

// app.UseAuthorization();

app.MapControllers();

AppInit(app);

app.Run();

// Application initialization.
void AppInit(WebApplication app)
{
    app.Logger.LogInformation("Initializing application...");

    InitKeys(app);

    Global.Ready = true; // server is ready to handle requests
}

// Initialize Cryptography keys.
void InitKeys(WebApplication app)
{
    RSA? privKey = null, pubKey = null;


    var rsaPfxFile = app.Configuration["Keys:RSAPFXFile"];
    var rsaPrivKeyFile = app.Configuration["Keys:RSAPrivKeyFile"];

    if (!string.IsNullOrWhiteSpace(rsaPfxFile))
    {
        // load RSA private key from PFX file if available
        app.Logger.LogInformation($"Loading RSA private key from PFX file: {rsaPfxFile}...");
        var rsaPfxPassword = app.Configuration["Keys:RSAPFXPassword"] ?? "";
        using var cert = new X509Certificate2(rsaPfxFile, rsaPfxPassword);
        privKey = cert.GetRSAPrivateKey() ?? throw new InvalidDataException($"Failed to load RSA private key from PFX file: {rsaPfxFile}");
        pubKey = cert.GetRSAPublicKey();
    }
    else if (!string.IsNullOrWhiteSpace(rsaPrivKeyFile))
    {
        app.Logger.LogInformation($"Loading RSA private key from file: {rsaPrivKeyFile}...");
        var rsaPrivKey = File.ReadAllText(rsaPrivKeyFile);
        privKey = RSA.Create();
        privKey.ImportFromPem(rsaPrivKey);
        pubKey = RSA.Create(privKey.ExportParameters(false));
    }

    if (privKey == null) throw new InvalidDataException("No RSA private key provided!");

    Global.RSAPrivKey = privKey;
    Global.RSAPubKey = pubKey;
}
