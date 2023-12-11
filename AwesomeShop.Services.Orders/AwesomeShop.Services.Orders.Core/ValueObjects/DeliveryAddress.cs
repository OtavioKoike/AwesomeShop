using System;

namespace AwesomeShop.Services.Orders.Core.ValueObjects
{
    public class DeliveryAddress
    {
        public string Street { get; private set; }
        public string Number { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string ZipCode { get; private set; }

        public DeliveryAddress(string street, string number, string city, string state, string zipCode)
        {
            Street = street;
            Number = number;
            City = city;
            State = state;
            ZipCode = zipCode;
        }

        //Como um Value Object é identificado por todos atributos, é importante a comparação de todos
        public override bool Equals(object obj)
        {
            return obj is DeliveryAddress address &&
                   Street == address.Street &&
                   Number == address.Number &&
                   City == address.City &&
                   State == address.State &&
                   ZipCode == address.ZipCode;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Street, Number, City, State, ZipCode);
        }
    }
}
