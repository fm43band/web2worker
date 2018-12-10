using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Messaging.Abstracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using ServiceBusMessageForwarder;

namespace MessageForwarder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageGatewayController : ControllerBase
    {
        private IMessageSender<string> sender;

        public MessageGatewayController(IConfiguration configutation)
        {
            var sbConnectionString = 
                configutation.GetSection("ServiceBus:ConnectionString").Value;
            var sbQueueName =
                configutation.GetSection("ServiceBus:QueueName").Value;
            sender = new SbMessageSender<string>(sbConnectionString,
                sbQueueName); 
        }
        // GET: api/MessageGateway
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/MessageGateway/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/MessageGateway
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Message<string> value)
        {
            if (ModelState.IsValid)
            {
                await sender.SendAsync(value);

                return Ok();
            }

            return BadRequest(ModelState);
        }

        // PUT: api/MessageGateway/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
