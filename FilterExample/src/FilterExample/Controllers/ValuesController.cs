using FilterExample.Concurrency;
using FilterExample.Core;
using FilterExample.Models;
using FilterExample.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FilterExample.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private ApplicationContext _applicationContext;

        private IRepository<Value> _repository;
        public ValuesController(ApplicationContext applicationContext,
            IRepository<Value> repository)
        {
            _applicationContext = applicationContext;
            _repository = repository;
        }

        [ValueEnableConcurrency]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = _applicationContext.GetObject<IEnumerable<Value>>();

            return await Task.FromResult(Ok(result));
        }

        [ValueEnableConcurrency]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = _applicationContext.GetObject<Value>();
            return await Task.FromResult(Ok(result));
        }

        [ValueEnableConcurrency(IsCreateAction = true)]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Value value)
        {
            var result = await _repository.CreateAsync(value);

            _applicationContext.ResponseCheckSum = EnableConcurrenyAttributeBase.CalculateConcurrencyValue(result);
            return await Task.FromResult(Ok(result));
        }

        [ValueEnableConcurrency(true)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Value value)
        {
            var result = await _repository.UpdateAsync(value, id);

            _applicationContext.ResponseCheckSum = EnableConcurrenyAttributeBase.CalculateConcurrencyValue(result);
            
            //Return result
            //Note: It's debatable if the status code should be managed here or in the ActionFilterAttribute
            return await Task.FromResult(StatusCode(200, result));
        }
    }
}
