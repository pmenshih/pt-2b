using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace pt_2b.Models
{
    public class DataBase : DbContext
    {
        public DataBase(): base("DefaultConnection"){ }

        public DbSet<Form> Forms { get; set; }
        public DbSet<FormAnswer> FormAnswers { get; set; }

        public DbSet<THSUserTypes> THSUserType { get; set; }
        public DbSet<User> User { get; set; }

        public DbSet<THSUser> THSUsers { get; set; }
        public System.Data.Entity.DbSet<pt_2b.Models.Organisation> Organisations { get; set; }
        public System.Data.Entity.DbSet<pt_2b.Models.THSForm> THSForms { get; set; }
    }
}