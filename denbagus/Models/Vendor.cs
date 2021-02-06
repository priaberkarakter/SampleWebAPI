using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DenBagus.Models
{
    public class Vendor
    {
        [Key]
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public virtual IEnumerable<Models.Stock> Stocks { get; set; }
    }
}
