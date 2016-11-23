using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContosoBankBot
{

    public class News
    {
        public class RootObject
        {
            public string Id { get; set; }
            public string CreatedAt { get; set; }

            public string UpdatedAt { get; set; }
            public string Version { get; set; }
            public bool Deleted { get; set; }
            public string Title { get; set; }
            public string Text { get; set; }
        }
    }

    public class OfficeAddress
    {
        public class RootObject
        {
            public string id { get; set; }
            public string createdAt { get; set; }
            public string updatedAt { get; set; }
            public string version { get; set; }
            public bool deleted { get; set; }
            public string City { get; set; }
            public string Location { get; set; }
            public string Phone { get; set; }
        }
    }
}