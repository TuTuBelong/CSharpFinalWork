﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportData
{
   
        public class Recommend
        {
            public int id { get; set; }
            public string account { get; set; }
            public int PoemId { get; set; }

            public Poem poem { get; set; }
        }
    
}
