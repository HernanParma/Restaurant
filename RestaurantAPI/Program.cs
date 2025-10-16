using Application.Interfaces;
using Application.Queries;
using Application.Services;
using Domain.Enums;
using Infrastructure.Commands;
using Infrastructure.Persistence;
using Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using RestaurantAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// CORS para el front
const string FrontPolicy = "FrontPolicy";
builder.Services.AddCors(o =>
{
    o.AddPolicy(FrontPolicy, p => p
        .WithOrigins(
            "http://127.0.0.1:5500",
            "http://localhost:5500",
            "https://TU-FRONT-PROD.com"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RestaurantAPI", Version = "v1" });

    c.TagActionsBy(api =>
    {
        var g = api.GroupName;
        if (!string.IsNullOrWhiteSpace(g)) return new[] { g };
        var hasCtrl = api.ActionDescriptor.RouteValues.TryGetValue("controller", out var ctrl);
        return new[] { hasCtrl ? ctrl! : "Default" };
    });
    c.DocInclusionPredicate((docName, apiDesc) => true);

    // Enum PriceSort como "asc"/"desc"
    c.MapType<PriceSort>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = new List<IOpenApiAny> { new OpenApiString("asc"), new OpenApiString("desc") }
    });
});

// DI custom
builder.Services.AddScoped<ICategoryQuery, CategoryQuery>();
builder.Services.AddScoped<IDeliveryTypeQuery, DeliveryTypeQuery>();
builder.Services.AddScoped<IStatusQuery, StatusQuery>();
builder.Services.AddScoped<ICreateDishService, CreateDishService>();
builder.Services.AddScoped<IUpdateDishService, UpdateDishService>();
builder.Services.AddScoped<IGetAllDishesService, GetAllDishesService>();
builder.Services.AddScoped<IDishCommand, DishCommand>();
builder.Services.AddScoped<IDishQuery, DishQuery>();
builder.Services.AddTransient<ApiExceptionMiddleware>();
builder.Services.AddScoped<IDeleteDishService, DeleteDishService>();
builder.Services.AddScoped<ICreateOrderService, CreateOrderService>();
builder.Services.AddScoped<IOrderCommand, OrderCommand>();
builder.Services.AddScoped<IGetOrdersService, GetOrdersService>();
builder.Services.AddScoped<IOrderQuery, OrderQuery>();
builder.Services.AddScoped<IUpdateOrderService, UpdateOrderService>();
builder.Services.AddScoped<IGetOrderByIdService, GetOrderByIdService>();
builder.Services.AddScoped<IUpdateOrderItemStatusService, UpdateOrderItemStatusService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
}

var app = builder.Build();


if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RestaurantAPI v1"));
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseCors(FrontPolicy);

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseAuthorization();


app.MapControllers().RequireCors(FrontPolicy);



app.Run();

// Para los tests
public partial class Program { }
