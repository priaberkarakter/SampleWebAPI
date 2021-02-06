using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DenBagus.Models;

namespace DenBagus.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly DataAPI _ctx;
        private readonly ILogger<Customer> _logger;

        public CustomerController(ILogger<Customer> logger, DataAPI _ctx)
        {
            _logger = logger;
            this._ctx = _ctx;
        }
        public async Task<object> Get()
        {
            try
            {
                var outputData = _ctx.Customer.ToArray();
                await _ctx.DisposeAsync();
                return Ok(outputData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }

        [HttpGet("{id}")]
        public async Task<object> Get(int id)
        {
            try
            {
                var outputData = _ctx.Customer
                    .Where(k => k.id == id)
                    .ToArray();
                await _ctx.DisposeAsync();
                return Ok(outputData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }
    }
}
