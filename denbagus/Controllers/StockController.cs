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
    [ApiController]
    [ApiVersion("1")]
    public class StockController : ControllerBase
    {
        private readonly DataAPI ctx;
        private readonly ILogger<StockController> _logger;

        public StockController(ILogger<StockController> logger, DataAPI ctx)
        {
            _logger = logger;
            this.ctx = ctx;
        }

        [HttpPost]
        public async Task<object> Post([FromBody] ViewModel.StockPost value)
        {
            try
            {
                var vendoCodeClaim = HttpContext.User.Claims.Where(x => x.Type == "vendorcode").FirstOrDefault();

                if(vendoCodeClaim == null)
                {
                    value.ErrorMessage = "Unknown vendor code, please provide vendor code in user attribute!";
                    return BadRequest(value);
                }

                string vendorCode = vendoCodeClaim.Value;
                DateTime currentDateUtc = DateTime.UtcNow;

                // Validate duplicate stock
                var duplicateStocks = value.Stocks
                    .GroupBy(x => new { x.plant, x.storage_location, x.part_code })
                    .Select(x => new
                    {
                        plant = x.Key.plant,
                        storage_location = x.Key.storage_location,
                        part_code = x.Key.part_code,
                        Count = x.Count()
                    })
                    .Where(x => x.Count > 1)
                    .ToList();

                if (duplicateStocks.Count() > 0)
                {
                    var stock = duplicateStocks.First();
                    string errMsg = string.Format("Plant {0}, Storage {1}, PartCode {2} must be unique per request", stock.plant, stock.storage_location, stock.part_code);
                    value.ErrorMessage = errMsg;
                    return BadRequest(value);
                }

                var plants = value.Stocks.Select(x => x.plant).Distinct();
                var storLocs = value.Stocks.Select(x => x.storage_location).Distinct();
                var parts = value.Stocks.Select(x => x.part_code).Distinct();

                var existDatas = ctx.Stock
                    .Where(x => x.vendor_code == vendorCode)
                    .Where(x => plants.Contains(x.plant))
                    .Where(x => storLocs.Contains(x.storage_location))
                    .Where(x => parts.Contains(x.part_code))
                    .ToList();


                using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    foreach (var item in value.Stocks)
                    {
                        Stock oldStock = existDatas
                            .Where(x => x.vendor_code == vendorCode)
                            .Where(x => x.plant == item.plant)
                            .Where(x => x.storage_location == item.storage_location)
                            .Where(x => x.part_code == item.part_code)
                            .FirstOrDefault();
                        
                        item.vendor_code = vendorCode;
                        item.api_receive_datetime = currentDateUtc;

                        if (oldStock == null)
                        {
                            await ctx.AddAsync(item);
                        }
                        else
                        {
                            oldStock.as_of_date_time = item.as_of_date_time;
                            oldStock.available_stock = item.available_stock;
                            oldStock.blocked_stock = item.blocked_stock;
                            oldStock.last_modified = item.last_modified;
                            oldStock.api_receive_datetime = item.api_receive_datetime;

                            item.id = oldStock.id;
                            ctx.Update(oldStock);
                        }
                    }
                    
                    await ctx.SaveChangesAsync();
                    scope.Complete();
                }

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error post stocks with request {0}", value);
                value.ErrorMessage = ex.GetExceptionMessages();
                return value;
            }
        }
    }
}