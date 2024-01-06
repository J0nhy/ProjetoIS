using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjetoIS_D02.Models
{
    public class Subscription
    {
        public string Res_type { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Creation_dt { get; set; }
        public int Parent { get; set; }
        public string Event { get; set; }
        public string Endpoint { get; set; }
    }
}