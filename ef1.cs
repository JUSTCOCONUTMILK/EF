using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarDealership
{
    public class Car
    {
        public int CarId { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }
        public List<ServiceHistory> ServiceHistories { get; set; }
        public List<Sale> Sales { get; set; }
    }

    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<Sale> Sales { get; set; }
    }

    public class Employee
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public List<Sale> Sales { get; set; }
    }

    public class Sale
    {
        public int SaleId { get; set; }
        public DateTime Date { get; set; }
        public int CarId { get; set; }
        public Car Car { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }

    public class ServiceHistory
    {
        public int ServiceHistoryId { get; set; }
        public int CarId { get; set; }
        public Car Car { get; set; }
        public DateTime ServiceDate { get; set; }
        public string ServiceDetails { get; set; }
    }

    public class CarDealershipContext : DbContext
    {
        public DbSet<Car> Cars { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<ServiceHistory> ServiceHistories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.;Database=CarDealership;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Car>().HasData(
                new Car { CarId = 1, Make = "Toyota", Model = "Camry", Year = 2020, Price = 25000 },
                new Car { CarId = 2, Make = "Honda", Model = "Civic", Year = 2021, Price = 22000 },
                new Car { CarId = 3, Make = "Ford", Model = "Focus", Year = 2020, Price = 20000 },
                new Car { CarId = 4, Make = "BMW", Model = "X5", Year = 2022, Price = 50000 },
                new Car { CarId = 5, Make = "Audi", Model = "A4", Year = 2021, Price = 35000 }
            );

            modelBuilder.Entity<Customer>().HasData(
                new Customer { CustomerId = 1, Name = "John Doe", Email = "john@example.com" },
                new Customer { CustomerId = 2, Name = "Jane Smith", Email = "jane@example.com" },
                new Customer { CustomerId = 3, Name = "Michael Johnson", Email = "michael@example.com" },
                new Customer { CustomerId = 4, Name = "Emily Davis", Email = "emily@example.com" },
                new Customer { CustomerId = 5, Name = "Chris Brown", Email = "chris@example.com" }
            );

            modelBuilder.Entity<Employee>().HasData(
                new Employee { EmployeeId = 1, Name = "Alice Green" },
                new Employee { EmployeeId = 2, Name = "Bob White" },
                new Employee { EmployeeId = 3, Name = "Charlie Black" }
            );

            modelBuilder.Entity<Sale>().HasData(
                new Sale { SaleId = 1, Date = new DateTime(2022, 5, 10), CarId = 1, CustomerId = 1, EmployeeId = 1 },
                new Sale { SaleId = 2, Date = new DateTime(2022, 6, 15), CarId = 2, CustomerId = 2, EmployeeId = 2 },
                new Sale { SaleId = 3, Date = new DateTime(2022, 7, 20), CarId = 3, CustomerId = 3, EmployeeId = 3 },
                new Sale { SaleId = 4, Date = new DateTime(2022, 8, 25), CarId = 4, CustomerId = 4, EmployeeId = 1 },
                new Sale { SaleId = 5, Date = new DateTime(2022, 9, 30), CarId = 5, CustomerId = 5, EmployeeId = 2 }
            );

            modelBuilder.Entity<ServiceHistory>().HasData(
                new ServiceHistory { ServiceHistoryId = 1, CarId = 1, ServiceDate = new DateTime(2022, 5, 20), ServiceDetails = "Oil change" },
                new ServiceHistory { ServiceHistoryId = 2, CarId = 2, ServiceDate = new DateTime(2022, 6, 25), ServiceDetails = "Tire replacement" }
            );
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new CarDealershipContext())
            {
                context.Database.Migrate();

                var salesByEmployee = context.Sales
                    .GroupBy(s => s.Employee.Name)
                    .Select(g => new { Employee = g.Key, SalesCount = g.Count() })
                    .ToList();

                Console.WriteLine("Sales by Employee:");
                foreach (var sale in salesByEmployee)
                {
                    Console.WriteLine($"{sale.Employee}: {sale.SalesCount} sales");
                }

                var customerCars = context.Sales
                    .Where(s => s.Customer.Name == "John Doe")
                    .Select(s => s.Car)
                    .ToList();

                Console.WriteLine("\nCars bought by John Doe:");
                foreach (var car in customerCars)
                {
                    Console.WriteLine($"{car.Make} {car.Model} ({car.Year})");
                }

                var salesIn2022 = context.Sales
                    .Where(s => s.Date.Year == 2022)
                    .ToList();

                Console.WriteLine("\nSales in 2022:");
                foreach (var sale in salesIn2022)
                {
                    Console.WriteLine($"{sale.Car.Make} {sale.Car.Model} - {sale.Customer.Name} - {sale.Date.ToShortDateString()}");
                }
            }
        }
    }
}

