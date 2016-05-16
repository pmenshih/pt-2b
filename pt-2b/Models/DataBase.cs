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
    }
}