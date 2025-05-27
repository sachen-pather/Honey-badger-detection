using Honey_badger_detection.Configuration;
using Honey_badger_detection.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Configure Custom Vision Settings
builder.Services.Configure<CustomVisionSettings>(
    builder.Configuration.GetSection("CustomVision"));

// Configure Dropbox Settings
builder.Services.Configure<DropboxSettings>(
    builder.Configuration.GetSection("Dropbox"));

// Register HTTP client
builder.Services.AddHttpClient<ICustomVisionService, CustomVisionService>();

// Register services
builder.Services.AddScoped<ICustomVisionService, CustomVisionService>();
builder.Services.AddScoped<IDropboxService, DropboxService>();

// Add basic health checks
builder.Services.AddHealthChecks();

// Configure to listen on any IP
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(7061, listenOptions =>
    {
        listenOptions.UseHttps();
    });
    serverOptions.ListenAnyIP(5134);
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Add simple health endpoint
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        await context.Response.WriteAsync("Healthy! API is running");
    }
});

app.UseAuthorization();
app.MapControllers();

app.Run();