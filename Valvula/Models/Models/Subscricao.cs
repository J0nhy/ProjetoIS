using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Valvula.Models
{
    public class Subscricao
    {
        public string Res_type { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Creation_dt { get; set; }
        public int Parent { get; set; }
        public string Event { get; set; }
        public string Endpoint { get; set; }

    }

}