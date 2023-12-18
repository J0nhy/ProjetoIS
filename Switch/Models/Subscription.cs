using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Web;

namespace Switch.Models
{
    public class Subscription
    {
        public string Res_type { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public DateTime creation_dt { get; set; }
        public int parent { get; set; }
        public string Event { get; set; }
        public string endpoint { get; set; }

    }
}