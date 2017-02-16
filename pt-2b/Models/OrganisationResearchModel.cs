using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace pt_2b.Models
{
    namespace OrganisationsResearches
    {
        public class OrganisationsResearch
        {
            public int id { get; set; }
            public int orgId { get; set; }
            public string title { get; set; }
            public DateTime dateCreate { get; set; }
            public string mailTitle { get; set; }
            public string mailBody { get; set; }
            public string scenario { get; set; }

            public static OrganisationsResearch GetById(int id)
            {
                using (DataBase db = new DataBase()) {
                    return db.OrganisationsResearches.SingleOrDefault(x => x.id == id);
                }
            }
        }
    }
}