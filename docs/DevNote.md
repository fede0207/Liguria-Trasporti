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