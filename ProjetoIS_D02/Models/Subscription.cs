using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjetoIS_D02.Models
{
    public class Subscription
    {
        public string Res_type { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public DateTime creation_dt { get; set; }
    }
}