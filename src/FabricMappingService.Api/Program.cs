using FabricMappingService.Core.Models;
using FabricMappingService.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure mapping service
builder.Services.AddSingleton<MappingConfiguration>(new MappingConfiguration
{
    CaseSensitive = true,
    IgnoreUnmapped = false,
    ThrowOnError = false,
    MapNullValues = true,
    MaxDepth = 10
});

builder.Services.AddScoped<IAttributeMappingService, AttributeMappingService>();

// Configure reference mapping services
builder.Services.AddSingleton<IReferenceMappingStorage, InMemoryReferenceMappingStorage>();
builder.Services.AddScoped<IMappingIO, MappingIO>();

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Root endpoint
app.MapGet("/", () => new
{
    service = "Fabric Mapping Service",
    version = "1.0.0",
    description = "Data Attribute Mapping Service for Microsoft Fabric Extensibility Toolkit",
    endpoints = new[]
    {
        "/api/mapping/info",
        "/api/mapping/health",
        "/api/mapping/customer/legacy-to-modern",
        "/api/mapping/product/external-to-internal",
        "/api/mapping/customer/batch-legacy-to-modern",
        "/api/reference-tables",
        "/api/reference-tables/sync",
        "/api/reference-tables/{tableName}"
    },
    documentation = "/openapi/v1.json"
})
.WithName("Root");

app.Run();
