using AwesomeShop.Services.Orders.Application.Commands;
using AwesomeShop.Services.Orders.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AwesomeShop.Services.Orders.Api.Controllers
{
    //[Route("api/customers/{customerId}/orders")]
    [Route("api/orders")]
    [ApiController]

    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var result = await _mediator.Send(new GetOrderById(id));

                if (result == null)
                    return NotFound();

                return Ok();
            }
            catch(Exception e)
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AddOrder command)
        {
            try
            {
                var id = await _mediator.Send(command);

                // Retorno p/ cliente: gerar rota do Get concatenando com id de retorno
                return CreatedAtAction(nameof(Get), new { id }, command);
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }
    }
}
