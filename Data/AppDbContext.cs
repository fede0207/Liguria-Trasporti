using Liguria_Trasporti.Models;
using Microsoft.EntityFrameworkCore;

namespace Liguria_Trasporti.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Shipment> Shipments { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Address> Addresses { get; set; }
}