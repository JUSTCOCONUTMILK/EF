using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CarDealerApp
{
    public class Car
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int DealerId { get; set; }
        public Dealer Dealer { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class Dealer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public List<Car> Cars { get; set; } = new();
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Order> Orders { get; set; } = new();
    }

    public class Order
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public Car Car { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
    }

    public class AppDbContext : DbContext
    {
        public DbSet<Car> Cars { get; set; }
        public DbSet<Dealer> Dealers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=COCO;Database=CarDealerDb;Trusted_Connection=True;")
                          .LogTo(Console.WriteLine);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Car>().HasIndex(c => new { c.Make, c.Model }).IsUnique();
            modelBuilder.Entity<Car>().Property(c => c.Make).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Car>().Property(c => c.Model).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Car>().Property(c => c.Year).IsRequired().HasColumnType("int").HasDefaultValue(2000);
        }
    }

    class Program
    {
        static void Main()
        {
            using var context = new AppDbContext();
            context.Database.Migrate();

            context.Dealers.Add(new Dealer { Name = "AutoWorld", Location = "New York" });
            context.SaveChanges();

            var dealer = context.Dealers.FirstOrDefault();
            context.Cars.Add(new Car { Make = "Toyota", Model = "Corolla", Year = 2020, DealerId = dealer.Id });
            context.SaveChanges();

            var cars = context.Cars.Include(c => c.Dealer).ToList();
            foreach (var car in cars)
            {
                Console.WriteLine($"{car.Make} {car.Model} ({car.Year}) - Dealer: {car.Dealer.Name}");
            }
        }
    }
}
