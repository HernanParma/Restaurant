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

    // Mapear enum PriceSort como string con valores “asc/desc”
    c.MapType<PriceSort>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = new List<IOpenApiAny> { new OpenApiString("asc"), new OpenApiString("desc") }
    });
});

// custom
builder.Services.AddScoped<ICategoryQuery, CategoryQuery>();
builder.Services.AddScoped<IDeliveryTypeQuery, DeliveryTypeQuery>();
builder.Services.AddScoped<IStatusQuery, StatusQuery>();
builder.Services.AddScoped<ICreateDishService, CreateDishService>();
builder.Services.AddScoped<IUpdateDishService, UpdateDishService  > ();
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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<IUpdateOrderItemStatusService, UpdateOrderItemStatusService>();

var app = builder.Build();
app.UseMiddleware<ApiExceptionMiddleware>();
// esto Aplica migraciones automáticamente al iniciar 
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); 
}

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