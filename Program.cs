using System.Security.Claims;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Liguria_Trasporti.Data;
using Liguria_Trasporti.Enums;
using Liguria_Trasporti.Services.Shipments;
using Liguria_Trasporti.Services.Employees;
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

var firebaseCredPath = Path.Combine(AppContext.BaseDirectory, "liguriatrasporti-ca606-firebase-adminsdk-fbsvc-bd27571915.json");
if (!File.Exists(firebaseCredPath))
{
    throw new Exception($"Firebase credential file not found at {firebaseCredPath}");
}
var credential = GoogleCredential.FromFile(firebaseCredPath);
FirebaseApp.Create(new AppOptions { Credential = credential, ProjectId = configurationProjectId });
builder.Services.AddSingleton(FirebaseAuth.DefaultInstance);

builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.Authority = $"https://securetoken.google.com/{configurationProjectId}";
    options.TokenValidationParameters.ValidIssuer = $"https://securetoken.google.com/{configurationProjectId}";
    options.TokenValidationParameters.ValidAudience = configurationProjectId;
    options.Events.OnTokenValidated = context =>
    {
        // Additional validation can be done here if needed
        var roleClaim = context.Principal!.Claims.FirstOrDefault(c => c.Type == "role");
        if (roleClaim is not null)
        {
            var identity = context.Principal.Identity as ClaimsIdentity;
            if (identity is null)
            {
                context.Fail("Identity is null.");
                return Task.CompletedTask;
            }
            if (!identity.HasClaim(c => c.Type == ClaimTypes.Role))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
            }
        }
        
        var firebaseId = context.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (firebaseId is not null)
        {
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();     
            var employee = dbContext.Employees.FirstOrDefault(e => e.FirebaseId == firebaseId);
            if (employee is null)
            {
                context.Fail("Employee does not exist.");
                return Task.CompletedTask;
            }
            if (employee!.AccountStatus is AccountStatus.Disabled)
            {
                context.Fail("Account is disabled.");
                return Task.CompletedTask;
            }
        }
        if (firebaseId is null)
        {
            context.Fail("Firebase ID claim is missing.");
        }
        return Task.CompletedTask;
    };
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
