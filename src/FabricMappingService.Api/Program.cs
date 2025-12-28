using FabricMappingService.Core.Models;
using FabricMappingService.Core.Services;
using FabricMappingService.Core.Workload;

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

// Configure lakehouse storage options
var lakehouseOptions = builder.Configuration.GetSection("LakehouseStorage").Get<LakehouseStorageOptions>()
    ?? new LakehouseStorageOptions
    {
        UseInMemoryStorage = true, // Default to in-memory for backward compatibility
        BasePath = Path.Combine(Path.GetTempPath(), "FabricMappingService", "Lakehouse")
    };

builder.Services.AddSingleton(lakehouseOptions);

// Configure reference mapping services based on storage option
if (lakehouseOptions.UseInMemoryStorage)
{
    builder.Services.AddSingleton<IReferenceMappingStorage, InMemoryReferenceMappingStorage>();
}
else
{
    builder.Services.AddSingleton<ILakehouseStorage, LakehouseStorage>();
    builder.Services.AddSingleton<IReferenceMappingStorage>(sp =>
    {
        var lakehouseStorage = sp.GetRequiredService<ILakehouseStorage>();
        return new LakehouseReferenceMappingStorage(lakehouseStorage, lakehouseOptions.BasePath);
    });
}

builder.Services.AddScoped<IMappingIO, MappingIO>();

// Configure item definition and OneLake storage services
builder.Services.AddSingleton<IItemDefinitionStorage, ItemDefinitionStorage>();
builder.Services.AddSingleton<IOneLakeStorage, OneLakeStorage>();

// Configure workload
builder.Services.AddScoped<IWorkload, MappingWorkload>();

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
    workload = new
    {
        id = "fabric-mapping-service",
        displayName = "Reference Table & Data Mapping Service",
        status = "Available"
    },
    endpoints = new[]
    {
        "/api/workload/info",
        "/api/workload/health",
        "/api/workload/execute",
        "/api/workload/validate",
        "/api/mapping/info",
        "/api/mapping/health",
        "/api/mapping/customer/legacy-to-modern",
        "/api/mapping/product/external-to-internal",
        "/api/mapping/customer/batch-legacy-to-modern",
        "/api/reference-tables",
        "/api/reference-tables/sync",
        "/api/reference-tables/{tableName}",
        "/api/items",
        "/api/items/{itemId}",
        "/api/items/workspace/{workspaceId}",
        "/api/items/store-to-onelake",
        "/api/items/read-from-onelake/{workspaceId}/{itemId}/{tableName}"
    },
    documentation = "/openapi/v1.json"
})
.WithName("Root");

app.Run();
