using Dapper;
using NetTopologySuite;
using Npgsql;
using RiverAPI.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add CORS Policy Service
// This enables the browser-based frontend (index.html) to communicate with this API
// "AllowAll" is permissible for development environments
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Create Npgsql DataSource
var connectionString = builder.Configuration.GetConnectionString("PostgresDb");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.UseNetTopologySuite();

var dataSource = dataSourceBuilder.Build();
builder.Services.AddSingleton(dataSource);

// Repository & Service Injection 
builder.Services.AddScoped<IRiverRepository, PostgresRiverRepository>();
builder.Services.AddScoped<RiverAPI.Services.SemanticRiverService>();

// Add services to the container
// Registering GeoJsonConverterFactory to handle spatial data serialization correctly
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable CORS Middleware
// Must be placed before UseAuthorization and MapControllers
app.UseCors("AllowAll");

// Enable Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();