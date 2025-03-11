using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
            optionsBuilder.UseSqlServer("Server=.;Database=CarDealerDb;Trusted_Connection=True;")
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

            using var transaction = context.Database.BeginTransaction();
            try
            {
                var dealer = new Dealer { Name = "AutoWorld", Location = "New York" };
                context.Dealers.Add(dealer);
                context.SaveChanges();

                var car = new Car { Make = "Toyota", Model = "Corolla", Year = 2020, DealerId = dealer.Id };
                context.Cars.Add(car);
                context.SaveChanges();

                transaction.Commit();
                Console.WriteLine("Transaction committed.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Transaction failed: {ex.Message}");
            }

            var cars = context.Cars.Include(c => c.Dealer).ToList();
            foreach (var car in cars)
            {
                Console.WriteLine($"{car.Make} {car.Model} ({car.Year}) - Dealer: {car.Dealer.Name}");
            }

            var specificCar = context.Cars.FirstOrDefault(c => c.Make == "Toyota");
            if (specificCar != null)
            {
                context.Entry(specificCar).Reference(c => c.Dealer).Load();
                Console.WriteLine($"Eager Loaded: {specificCar.Make} {specificCar.Model} from {specificCar.Dealer.Name}");
            }

            var sqlCars = context.Cars.FromSqlRaw("SELECT * FROM Cars WHERE Make = 'Toyota'").ToList();
            Console.WriteLine("Cars from SQL query:");
            foreach (var car in sqlCars)
            {
                Console.WriteLine($"{car.Make} {car.Model}");
            }

            var carToDelete = context.Cars.FirstOrDefault();
            if (carToDelete != null)
            {
                carToDelete.IsDeleted = true;
                context.SaveChanges();
                Console.WriteLine("Car marked as deleted.");
            }
        }
    }
}
