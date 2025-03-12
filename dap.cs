using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using Dapper;

namespace DapperExample
{
    public class Car
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }
    }

    public class CarRepository
    {
        private string connectionString = "Server=localhost; Database=COCO; Integrated Security=True;";

        public void AddCar(Car car)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var query = "INSERT INTO Cars (Brand, Model, Year, Price) VALUES (@Brand, @Model, @Year, @Price)";
                connection.Execute(query, car);
            }
        }

        public void UpdateCarPrice(int carId, decimal newPrice)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var query = "UPDATE Cars SET Price = @NewPrice WHERE Id = @CarId";
                connection.Execute(query, new { NewPrice = newPrice, CarId = carId });
            }
        }

        public void DeleteCar(int carId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var query = "DELETE FROM Cars WHERE Id = @CarId";
                connection.Execute(query, new { CarId = carId });
            }
        }

        public IEnumerable<Car> GetAllCars()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var query = "SELECT * FROM Cars";
                return connection.Query<Car>(query);
            }
        }

        public IEnumerable<Car> GetCarsByBrand(string brand)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var query = "SELECT * FROM Cars WHERE Brand = @BrandName";
                return connection.Query<Car>(query, new { BrandName = brand });
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var carRepo = new CarRepository();

            var newCar = new Car { Brand = "Toyota", Model = "Camry", Year = 2022, Price = 30000 };
            carRepo.AddCar(newCar);

            carRepo.UpdateCarPrice(1, 28000);

            carRepo.DeleteCar(2);

            var allCars = carRepo.GetAllCars();
            foreach (var car in allCars)
            {
                Console.WriteLine($"{car.Brand} {car.Model} {car.Year} {car.Price}");
            }

            var toyotaCars = carRepo.GetCarsByBrand("Toyota");
            foreach (var car in toyotaCars)
            {
                Console.WriteLine($"{car.Brand} {car.Model} {car.Year} {car.Price}");
            }
        }
    }
}
