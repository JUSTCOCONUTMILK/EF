using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EFCoreCarDealership
{
    public class CarDealershipContext : DbContext
    {
        public DbSet<Car> Cars { get; set; }
        public DbSet<Dealer> Dealers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }

        public CarDealershipContext(DbContextOptions<CarDealershipContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CarOrder>()
                .HasKey(co => new { co.CarId, co.CustomerId });
            modelBuilder.Entity<CarOrder>()
                .HasOne(co => co.Car)
                .WithMany(c => c.CarOrders)
                .HasForeignKey(co => co.CarId);

            modelBuilder.Entity<CarOrder>()
                .HasOne(co => co.Customer)
                .WithMany(c => c.CarOrders)
                .HasForeignKey(co => co.CustomerId);

            modelBuilder.Entity<Car>()
                .HasIndex(c => new { c.Make, c.Model })
                .IsUnique();
        }
    }

    public class Car
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Make { get; set; }

        [Required]
        [StringLength(100)]
        public string Model { get; set; }

        [Range(1900, 2100)]
        public int Year { get; set; }

        public int DealerId { get; set; }
        public Dealer Dealer { get; set; }

        public bool IsDeleted { get; set; } 

        public ICollection<CarOrder> CarOrders { get; set; }
    }

    public class Dealer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }

        public ICollection<Car> Cars { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<CarOrder> CarOrders { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int CarId { get; set; }
        public Car Car { get; set; }
    }

    public class CarOrder
    {
        public int CarId { get; set; }
        public Car Car { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CarDealershipContext>();
            optionsBuilder.UseSqlServer("Server=COCO\\MSSQLSERVER01;Database=CarDealership;Trusted_Connection=True;");

            using (var context = new CarDealershipContext(optionsBuilder.Options))
            {
                context.Database.EnsureCreated();

                var dealer = new Dealer { Name = "BestCars", Location = "New York" };
                context.Dealers.Add(dealer);
                context.SaveChanges();

                var car1 = new Car { Make = "Toyota", Model = "Camry", Year = 2020, DealerId = dealer.Id };
                var car2 = new Car { Make = "Honda", Model = "Civic", Year = 2021, DealerId = dealer.Id };
                context.Cars.AddRange(car1, car2);
                context.SaveChanges();

                var customer = new Customer { Name = "John Doe" };
                context.Customers.Add(customer);
                context.SaveChanges();

                var order = new Order { CustomerId = customer.Id, CarId = car1.Id };
                context.Orders.Add(order);
                context.SaveChanges();


                var cars = context.Cars.Include(c => c.Dealer).ToList();
                Console.WriteLine("Cars and Dealers:");
                foreach (var car in cars)
                {
                    Console.WriteLine($"Car: {car.Make} {car.Model} - Dealer: {car.Dealer.Name}");
                }

                var carToLoad = context.Cars.First();
                context.Entry(carToLoad).Reference(c => c.Dealer).Load();
                Console.WriteLine($"Loaded dealer for {carToLoad.Make} {carToLoad.Model}: {carToLoad.Dealer.Name}");


                optionsBuilder.LogTo(Console.WriteLine);

                var deletedCar = context.Cars.First();
                deletedCar.IsDeleted = true;
                context.SaveChanges();
                Console.WriteLine($"Car marked as deleted: {deletedCar.Make} {deletedCar.Model}");

                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var newCar = new Car { Make = "Ford", Model = "Focus", Year = 2022, DealerId = dealer.Id };
                        context.Cars.Add(newCar);
                        context.SaveChanges();

                        var newOrder = new Order { CustomerId = customer.Id, CarId = newCar.Id };
                        context.Orders.Add(newOrder);
                        context.SaveChanges();

                        transaction.Commit();
                        Console.WriteLine("Transaction completed successfully.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Transaction rolled back: {ex.Message}");
                    }
                }
            }
        }
    }
}
