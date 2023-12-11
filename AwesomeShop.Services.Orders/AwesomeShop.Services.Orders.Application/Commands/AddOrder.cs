using AwesomeShop.Services.Orders.Core.Entities;
using AwesomeShop.Services.Orders.Core.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AwesomeShop.Services.Orders.Application.Commands
{
    public class AddOrder : IRequest<Guid>
    {
        public CustumerInputModel Customer { get; set; }
        public List<OrderItemInputModel> Items { get; set; }
        public DeliveryAddressInputModel DeliveryAddress { get; set; }
        public PaymentAddressInputModel PaymentAddress { get; set; }
        public PaymentInfoInputModel PaymentInfo { get; set; }

        public Order ToEntity()
        {
            return new Order(
                CustumerInputModel.ToEntity(Customer),
                DeliveryAddressInputModel.ToEntity(DeliveryAddress),
                PaymentAddressInputModel.ToEntity(PaymentAddress),
                PaymentInfoInputModel.ToEntity(PaymentInfo),
                Items.Select(item => OrderItemInputModel.ToEntity(item)).ToList());
        }
    }

    public class CustumerInputModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public static Customer ToEntity(CustumerInputModel inputModel)
        {
            return new Customer(inputModel.Id, inputModel.FullName, inputModel.Email);
        }
    }

    public class OrderItemInputModel
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public static OrderItem ToEntity(OrderItemInputModel inputModel)
        {
            return new OrderItem(inputModel.ProductId, inputModel.Quantity, inputModel.Price);
        }
    }

    public class DeliveryAddressInputModel
    {
        public string Street { get; set; }
        public string Number { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }

        public static DeliveryAddress ToEntity(DeliveryAddressInputModel inputModel)
        {
            return new DeliveryAddress(inputModel.Street, inputModel.Number, inputModel.City, inputModel.State, inputModel.ZipCode);
        }
    }

    public class PaymentAddressInputModel
    {
        public string Street { get; set; }
        public string Number { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }

        public static PaymentAddress ToEntity(PaymentAddressInputModel inputModel)
        {
            return new PaymentAddress(inputModel.Street, inputModel.Number, inputModel.City, inputModel.State, inputModel.ZipCode);
        }
    }

    public class PaymentInfoInputModel
    {
        public string CardNumber { get; set; }
        public string FullName { get; set; }
        public string Expiration { get; set; }
        public string Cvv { get; set; }

        public static PaymentInfo ToEntity(PaymentInfoInputModel inputModel)
        {
            return new PaymentInfo(inputModel.CardNumber, inputModel.FullName, inputModel.Expiration, inputModel.Cvv);
        }
    }
}
