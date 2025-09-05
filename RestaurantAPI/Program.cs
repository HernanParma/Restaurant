using Application.Dishes;
using Application.Queries;
using Infrastructure.Dishes;
using Infrastructure.Persistence;
using Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSwaggerGen(c => { c.EnableAnnotations(); });
builder.Services.AddControllers();
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // Esto hace que los enums (PriceSort) se serialicen como "asc"/"desc"
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IDishService, DishService>();
builder.Services.AddSwaggerGen(c =>
{
    // <-- acá defines el documento "v1"
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RestaurantAPI",
        Version = "v1"
    });
});

// custom (Inyecciones de dependencias)
builder.Services.AddScoped<IDishService, DishService>();
builder.Services.AddScoped<IDishQuery, DishQuery>();
var connectionString = builder.Configuration["ConnectionString"];
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // <-- y acá le decís a la UI dónde está el JSON
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RestaurantAPI v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
