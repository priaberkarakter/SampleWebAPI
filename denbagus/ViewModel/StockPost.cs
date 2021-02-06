using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DenBagus.ViewModel
{
    public class StockPost
    {
        [JsonProperty(PropertyName = "stocks")]
        public Models.Stock[] Stocks { get; set; }

        [JsonProperty(PropertyName = "error_message")]
        public string ErrorMessage { get; set; }     
    }
}
