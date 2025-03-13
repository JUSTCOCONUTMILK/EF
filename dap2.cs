using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;

public class Car
{
    public int Id { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public decimal Price { get; set; }
}

public class CarWithOwner
{
    public int Id { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public decimal Price { get; set; }
    public string OwnerName { get; set; }
}

class Program
{
    static void Main()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        string connectionString = configuration.GetConnectionString("DefaultConnection");

        using (var connection = new SqlConnection(connectionString))
        {

            var createCars = @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Cars' AND xtype='U')
                               CREATE TABLE Cars (
                                   Id INT PRIMARY KEY IDENTITY(1,1),
                                   Brand NVARCHAR(50),
                                   Model NVARCHAR(50),
                                   Year INT,
                                   Price DECIMAL(18,2)
                               );";
            var createOwners = @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Owners' AND xtype='U')
                                 CREATE TABLE Owners (
                                     Id INT PRIMARY KEY IDENTITY(1,1),
                                     Name NVARCHAR(100),
                                     CarId INT,
                                     FOREIGN KEY (CarId) REFERENCES Cars(Id)
                                 );";
            connection.Execute(createCars);
            connection.Execute(createOwners);

            var carQuery = "INSERT INTO Cars (Brand, Model, Year, Price) VALUES (@Brand, @Model, @Year, @Price); SELECT CAST(SCOPE_IDENTITY() as int);";
            int carId = connection.QuerySingle<int>(carQuery, new { Brand = "Toyota", Model = "Corolla", Year = 2020, Price = 20000.00m });

            var ownerQuery = "INSERT INTO Owners (Name, CarId) VALUES (@Name, @CarId);";
            connection.Execute(ownerQuery, new { Name = "John Doe", CarId = carId });

            var updateQuery = "UPDATE Owners SET Name = @NewOwner WHERE CarId = @CarId;";
            connection.Execute(updateQuery, new { NewOwner = "Jane Smith", CarId = carId });

            var selectQuery = @"SELECT c.Id, c.Brand, c.Model, c.Year, c.Price, o.Name AS OwnerName 
                                FROM Cars c
                                INNER JOIN Owners o ON c.Id = o.CarId;";
            var carsWithOwners = connection.Query<CarWithOwner>(selectQuery);
            foreach (var car in carsWithOwners)
            {
                Console.WriteLine($"{car.Brand} {car.Model} ({car.Year}) - {car.OwnerName}");
            }

            var deleteOwner = "DELETE FROM Owners WHERE CarId = @CarId;";
            var deleteCar = "DELETE FROM Cars WHERE Id = @CarId;";
            connection.Execute(deleteOwner, new { CarId = carId });
            connection.Execute(deleteCar, new { CarId = carId });
        }
    }
}
