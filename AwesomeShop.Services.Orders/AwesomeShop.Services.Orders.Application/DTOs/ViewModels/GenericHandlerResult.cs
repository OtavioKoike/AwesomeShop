using System.Collections.Generic;

namespace AwesomeShop.Services.Orders.Application.DTOs.ViewModels
{
    // Classe de Exemplo para Erros Genericos
    public class GenericHandlerResult
    {
        public string Message { get; private set; }
        public object Data { get; private set; }
        public bool Success { get; private set; }
        public List<ValidationObject> Validations { get; private set; }

        public GenericHandlerResult(string message, object data, bool success, List<ValidationObject> validations)
        {
            Message = message;
            Data = data;
            Success = success;
            Validations = validations;
        }
    }

    public class ValidationObject
    {
        public string Property { get; private set; }
        public string Message { get; private set; }

        public ValidationObject(string property, string message)
        {
            Property = property;
            Message = message;
        }
    }
}
