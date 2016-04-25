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

        public DbSet<Test> Tests { get; set; }
        public DbSet<TestAnswer> TestAnswers { get; set; }
    }
}