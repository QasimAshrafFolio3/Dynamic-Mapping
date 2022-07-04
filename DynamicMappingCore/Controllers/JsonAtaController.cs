using DynamicMappingCore.Db;
using DynamicMappingCore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DynamicMappingCore.Controllers
{
    [Route("api/mapping")]
    [ApiController]
    public class JsonAtaController : ControllerBase
    {
        // GET: api/<JsonAtaController>
        [HttpGet]
        public List<JsonMetaData> Get()
        {
            return Database.InMemJsonMetaDataDb.Values.ToList().FirstOrDefault();
        }

        // GET api/<JsonAtaController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<JsonAtaController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<JsonAtaController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<JsonAtaController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
