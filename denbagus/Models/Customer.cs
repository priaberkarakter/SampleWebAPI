using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DenBagus.Models
{
    public class Customer
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }
}
