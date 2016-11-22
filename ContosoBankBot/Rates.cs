using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContosoBankBot
{

    public class Rates
    {
        public double AUD { get; set; }
        public double GBP { get; set; }
        public double RUB { get; set; }
        public double USD { get; set; }
    }

    public class RootObject
    {
        public string @base { get; set; }
        public string date { get; set; }
        public Rates rates { get; set; }
    }
}