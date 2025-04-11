using System;
using System.Collections.Generic;

namespace FastMapper.Tests
{
    // Temel modeller
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public int Age => (int)((DateTime.Now - BirthDate).TotalDays / 365.25);
        public Address HomeAddress { get; set; }
        public List<Order> Orders { get; set; }
        public bool IsActive { get; set; }
    }

    public class PersonDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public AddressDto HomeAddress { get; set; }
        public List<OrderDto> Orders { get; set; }
        public string Status { get; set; }
    }

    // İç içe modeller
    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
    }

    public class AddressDto
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string FullAddress { get; set; }
    }

    // Koleksiyon içeren modeller
    public class Order
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItem> Items { get; set; }
    }

    public class OrderDto
    {
        public int OrderId { get; set; }
        public string TotalAmount { get; set; }
        public string OrderDate { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }

    public class OrderItem
    {
        public int ItemId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class OrderItemDto
    {
        public int ItemId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string UnitPrice { get; set; }
        public string TotalPrice { get; set; }
    }

    // Farklı property tipleri içeren modeller
    public class VariousTypesSource
    {
        public int IntValue { get; set; }
        public long LongValue { get; set; }
        public double DoubleValue { get; set; }
        public decimal DecimalValue { get; set; }
        public string StringValue { get; set; }
        public DateTime DateValue { get; set; }
        public TimeSpan TimeValue { get; set; }
        public bool BoolValue { get; set; }
        public Guid GuidValue { get; set; }
        public DayOfWeek EnumValue { get; set; }
    }

    public class VariousTypesTarget
    {
        public string IntValue { get; set; }
        public int LongValue { get; set; }
        public int DoubleValue { get; set; }
        public double DecimalValue { get; set; }
        public int StringValue { get; set; }
        public string DateValue { get; set; }
        public long TimeValue { get; set; }
        public string BoolValue { get; set; }
        public string GuidValue { get; set; }
        public string EnumValue { get; set; }
    }
} 