## EF Core setup

1. Install packages:
    - Microsoft.EntityFrameworkCore.SqlServer
    - Microsoft.EntityFrameworkCore.Design

2. Create DbContext:
    - inherit from DbContext
    - constructor receives DbContextOptions<AppDbContext>
    - expose DbSet<TEntity>

3. Register in Program.cs:
    - read a connection string with GetConnectionString("DefaultConnection")
    - call AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString))

4. Store connection string:
    - run dotnet user-secrets init
    - run dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."

5. Create migration:
    - dotnet ef migrations add InitialCreate
    - dotnet ef database update

## Controller and services setup

1. Create the service contract:
    - define an interface, for example IShipmentService
    - add the methods the controller needs, for example GetAllShipments()

2. Create the service implementation:
    - inject AppDbContext in the service constructor
    - make the service implement the interface
    - use EF Core async methods such as ToListAsync()

3. Register the service in Program.cs:
    - call AddScoped<IShipmentService, ShipmentService>()
    - the concrete service must implement the interface

4. Register and map controllers:
    - call builder.Services.AddControllers()
    - call app.MapControllers()
    - MapControllers() should be outside the Development-only block

5. Create the controller:
    - use [ApiController]
    - use [Route("api/[controller]")]
    - inject the service through the constructor
    - use HTTP attributes such as [HttpGet]

## Employee module checkpoint

1. Employee entity:
    - represents company users/workers in the MVP
    - contains Id, Name, Surname, Email
    - contains Role, OperationalStatus, AccountStatus
    - contains nullable DrivingLicenseCategory because it applies only to drivers

2. Employee enums:
    - EmployeeRole models LogisticsOperator, ShippingManager, Driver
    - EmployeeOperationalStatus models Active, Absent, Unavailable
    - AccountStatus models Active, Disabled
    - DrivingLicenseCategory models simple driving categories for MVP assignment checks

3. EF Core registration:
    - add DbSet<Employee> Employees in AppDbContext
    - create a migration after adding the entity
    - run dotnet ef database update to apply it locally

4. Employee service/controller:
    - IEmployeeService defines the operations exposed to the controller
    - EmployeeService injects AppDbContext and queries Employees
    - EmployeeController injects IEmployeeService

5. Employee request DTO:
    - request DTOs represent input from the client
    - do not expose backend-controlled fields such as Id directly as client input
    - later create separate DTOs for create, update, and response if their shape differs

## Scalar/OpenAPI setup

1. Install package:
    - Scalar.AspNetCore

2. Register OpenAPI:
    - call builder.Services.AddOpenApi()

3. Map API docs only in development:
    - call app.MapOpenApi()
    - call app.MapScalarApiReference()

## Common mistakes

1. Empty using line:
    - a line like `using` without a namespace breaks Program.cs parsing

2. Service registration mismatch:
    - AddScoped<IService, Service>() requires Service to implement IService

3. Controllers registered but not mapped:
    - AddControllers() registers MVC/controller services
    - MapControllers() exposes attribute-routed controller endpoints

4. Calling service methods incorrectly:
    - call methods through the injected service
    - do not call a service method as if it were a local controller method

5. Build blocked by running app:
    - if the executable is locked, stop the app from Rider or stop the process by id

6. Mixing account status and operational status:
    - AccountStatus decides whether the user can access the system
    - OperationalStatus decides whether the employee is available for work/planning
