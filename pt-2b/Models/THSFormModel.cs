using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace pt_2b.Models
{
    public class THSForm
    {
        public int id { get; set; }
        public string name { get; set; }
        public int? organisationId { get; set; }
        public int? researchId { get; set; }
        public string targetName { get; set; }
        public string defScenario { get; set; }
    }

    public class THSUser
    {
        public int id { get; set; }
        public string userId { get; set; }
        public int thsId { get; set; }
        public int thsUType { get; set; }
        public string code { get; set; }
        public string raw { get; set; }
        public int answered { get; set; }
    }

    public class THSUserTypes
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}