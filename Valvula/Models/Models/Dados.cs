﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Valvula.Models
{
    public class Dados
    {
        public string Res_type { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Creation_dt { get; set; }
        public int Parent { get; set; }

    }

}