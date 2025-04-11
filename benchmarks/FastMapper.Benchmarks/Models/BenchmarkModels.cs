using System;
using System.Collections.Generic;

namespace FastMapper.Benchmarks.Models
{
    // Simple models
    public class SimpleSource
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class SimpleDestination
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    // Complex models
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime BirthDate { get; set; }
        public Address HomeAddress { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
    }

    public class CustomerDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime BirthDate { get; set; }
        public AddressDto HomeAddress { get; set; } = new();
        public List<OrderDto> Orders { get; set; } = new();
    }

    public class Address
    {
        public string Street { get; set; } = "";
        public string City { get; set; } = "";
        public string State { get; set; } = "";
        public string ZipCode { get; set; } = "";
        public string Country { get; set; } = "";
    }

    public class AddressDto
    {
        public string Street { get; set; } = "";
        public string City { get; set; } = "";
        public string State { get; set; } = "";
        public string ZipCode { get; set; } = "";
        public string Country { get; set; } = "";
    }

    public class Order
    {
        public int OrderId { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = "";
        public List<OrderItem> Items { get; set; } = new();
    }

    public class OrderDto
    {
        public int OrderId { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = "";
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItem
    {
        public int ItemId { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
    }

    public class OrderItemDto
    {
        public int ItemId { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
} 