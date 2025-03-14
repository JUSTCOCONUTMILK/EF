using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceApp {
    public class Client {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public List<Car> Cars { get; set; }
    }

    public class Car {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
    }

    public class Service {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class Order {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int CarId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public Client Client { get; set; }
        public Car Car { get; set; }
        public List<OrderedService> OrderedServices { get; set; }
    }

    public class OrderedService {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
        public Order Order { get; set; }
        public Service Service { get; set; }
    }

    public class AutoServiceContext : DbContext {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderedService> OrderedServices { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options) {
            options.UseSqlite("Data Source=autoservice.db");
        }
    }

    public class Program {
        public static void Main() {
            using var db = new AutoServiceContext();
            db.Database.Migrate();
            Console.WriteLine("Auto service started.");
            while (true) {
                Console.WriteLine("\n1. Add client");
                Console.WriteLine("2. Add car");
                Console.WriteLine("3. Add service");
                Console.WriteLine("4. Create order");
                Console.WriteLine("5. Show order history");
                Console.WriteLine("0. Exit");
                Console.Write("Choose an action: ");
                var choice = Console.ReadLine();
                switch (choice) {
                    case "1": AddClient(db); break;
                    case "2": AddCar(db); break;
                    case "3": AddService(db); break;
                    case "4": CreateOrder(db); break;
                    case "5": ShowOrderHistory(db); break;
                    case "0": return;
                    default: Console.WriteLine("Invalid choice."); break;
                }
            }
        }

        private static void AddService(AutoServiceContext db) {
            Console.Write("Service name: ");
            var name = Console.ReadLine()?.Trim();
if (string.IsNullOrEmpty(name)) {
    Console.WriteLine("Name cannot be empty.");
    return;
}
            Console.Write("Price: ");
            if (!decimal.TryParse(Console.ReadLine(), out var price) || price < 0) {
    Console.WriteLine("Invalid price.");
    return;
}
            var service = new Service { Name = name, Price = price };
            db.Services.Add(service);
            db.SaveChanges();
            Console.WriteLine("Service added.");
        }

        private static void CreateOrder(AutoServiceContext db) {
            Console.Write("Client ID: ");
var clientId = int.TryParse(Console.ReadLine(), out var result) ? result : -1;
if (!db.Clients.Any(c => c.Id == clientId)) {
    Console.WriteLine("Invalid client ID.");
    return;
}
            Console.Write("Car ID: ");
var carId = int.TryParse(Console.ReadLine(), out var result) ? result : -1;
if (!db.Cars.Any(c => c.Id == carId)) {
    Console.WriteLine("Invalid car ID.");
    return;
}
            Console.Write("Status: ");
var status = Console.ReadLine()?.Trim();
if (string.IsNullOrEmpty(status)) {
    Console.WriteLine("Status cannot be empty.");
    return;
}
var order = new Order { ClientId = clientId, CarId = carId, Date = DateTime.Now, Status = status };
            db.Orders.Add(order);
            db.SaveChanges();
            Console.WriteLine($"Order created with status: {order.Status}.");
        }

        private static void ShowOrderHistory(AutoServiceContext db) {
            var orders = db.Orders.Include(o => o.Client).Include(o => o.Car).ToList();
            foreach (var order in orders) {
    Console.WriteLine($"Order #{order.Id}, Client: {order.Client.FirstName} {order.Client.LastName}, Car: {order.Car.Make} {order.Car.Model}, Date: {order.Date}, Status: {order.Status}");
    var orderedServices = db.OrderedServices.Include(os => os.Service).Where(os => os.OrderId == order.Id).ToList();
    foreach (var os in orderedServices) {
        Console.WriteLine($"  - Service: {os.Service.Name}, Quantity: {os.Quantity}, Total: {os.Total:C}");
    }
}
            }
        }
    }
}
