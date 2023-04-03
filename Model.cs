using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

public class ArtigosEsportivosContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql
            (
                "Host=localhost;Port=5432;Pooling=true;Database=artigosesportivos;Username=artigosesportivos;Password=12345678"
            );

    protected override void OnModelCreating(ModelBuilder modelbuilder)
    {
        modelbuilder.Entity<User>()
            .Property(u => u.UserId)
            .UseIdentityAlwaysColumn();

        modelbuilder.Entity<Product>()
            .Property(p => p.ProductId)
            .UseIdentityAlwaysColumn();

        modelbuilder.Entity<Order>()
            .Property(o => o.OrderId)
            .UseIdentityAlwaysColumn();

        modelbuilder.Entity<Order>()
            .Property(o => o.Date)
            .HasDefaultValueSql("now()");
    }
}

public class User
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
}

public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public decimal Value { get; set; }
    public decimal LastValue { get; set; }
    public string Description { get; set; }
    public int Stock { get; set; }
    public int Sales { get; set; }
}

public class Order
{
    public int OrderId { get; }
    public User User { get; set; }
    public DateTime Date { get; set; }
    public List<Product> Products { get; set; }
}