using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Switch.Models
{
    public class Container
    {
        public string Res_type { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public DateTime creation_dt { get; set; }
        public int parent {  get; set; }
    }
}