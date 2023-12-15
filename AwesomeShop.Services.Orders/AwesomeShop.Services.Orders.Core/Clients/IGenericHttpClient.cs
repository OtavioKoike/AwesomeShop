using System;
using System.Threading.Tasks;

namespace AwesomeShop.Services.Orders.Core.Clients
{
    public interface IGenericHttpClient
    {
        Task<string> GetAsync(Uri url);
    }
}
