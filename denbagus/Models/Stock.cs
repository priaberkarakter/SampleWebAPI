using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DenBagus.Models
{
    public class Stock
    {
        public int id { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime as_of_date_time { get; set; }
        public string vendor_code { get; set; }
        public string part_code { get; set; }
        public string plant { get; set; }
        public string storage_location { get; set; }
        public decimal available_stock { get; set; }
        public decimal blocked_stock { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime last_modified { get; set; }
        public DateTime? api_receive_datetime { get; set; }

        [JsonIgnore]
        public virtual Models.Vendor Vendor { get; set; }
    }
}
