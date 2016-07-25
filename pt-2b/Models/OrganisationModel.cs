using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace pt_2b.Models
{
    public class Organisation
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class UsersOrganisations
    {
        public int id { get; set; }
        public int userId { get; set; }
        public int organisationId { get; set; }
        public DateTime dateCreate { get; set; }
    }
}