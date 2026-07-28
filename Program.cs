using Liguria_Trasporti.Data;
using Liguria_Trasporti.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var configurationProjectId = builder.Configuration["Firebase:ProjectId"];
if (string.IsNullOrWhiteSpace(configurationProjectId))
{
    throw new Exception("FirebaseProjectId is missing.");
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.Authority = $"https://securetoken.google.com/{configurationProjectId}";
    options.TokenValidationParameters.ValidIssuer = $"https://securetoken.google.com/{configurationProjectId}";
    options.TokenValidationParameters.ValidAudience = configurationProjectId;
});
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
