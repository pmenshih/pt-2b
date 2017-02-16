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

        public DbSet<THSUser> THSUsers { get; set; }
        public System.Data.Entity.DbSet<pt_2b.Models.Organisation> Organisations { get; set; }
        public System.Data.Entity.DbSet<pt_2b.Models.UsersOrganisations> UsersOrganisations { get; set; }
        public System.Data.Entity.DbSet<pt_2b.Models.THSForm> THSForms { get; set; }
        public DbSet<AspNetUser> AspNetUsers { get; set; }
        public DbSet<Organisations.OrganisationsUsersFile> OrganisationsUsersFiles { get; set; }
        public DbSet<OrganisationsResearches.OrganisationsResearch> OrganisationsResearches { get; set; }
    }

    public class AspNetUser
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string UserName { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronim { get; set; }
        public int Sex { get; set; }
    }
}