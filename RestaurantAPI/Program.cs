using Application.Dishes;
using Application.Dishes.Command.CreateDish;
using Application.Dishes.Command.UpdateDish;
using Application.Queries;
using Infrastructure.Dishes;
using Infrastructure.Dishes.Command;
using Infrastructure.Dishes.Command.UpdateDish;
using Infrastructure.Persistence;
using Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(c => { c.EnableAnnotations(); });
builder.Services.AddControllers();
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IDishService, DishService>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RestaurantAPI",
        Version = "v1"
    });
});

// custom (Inyecciones de dependencias)
builder.Services.AddScoped<IDishService, DishService>();
builder.Services.AddScoped<ICreateDishHandler, CreateDishHandler>();
builder.Services.AddScoped<IDishUpdateHandler, DishUpdateHandler>();
builder.Services.AddScoped<IDishQuery, DishQuery>();

var connectionString = builder.Configuration["ConnectionString"];
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RestaurantAPI v1");
    });
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
