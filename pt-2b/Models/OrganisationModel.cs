using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Web;

namespace pt_2b.Models
{
    public class Organisation
    {
        public int id { get; set; }
        public string name { get; set; }

        public static List<Organisations.Views.UsersUploadHistoryViewEntity> GetUploadHistory(
            string orgId, 
            bool getDeleted = false)
        {
            using (DataBase db = new DataBase())
            {
                string query = @"
SELECT id, dateCreate, usersCount
FROM OrganisationsUsersFiles
WHERE orgId=@orgId
    AND deleted=0
ORDER BY dateCreate DESC";
                if (getDeleted)
                    query = query.Replace("AND deleted=0", "");
                List<Organisations.Views.UsersUploadHistoryViewEntity> uploadHistory = db.Database.SqlQuery
                    <Models.Organisations.Views.UsersUploadHistoryViewEntity>
                    (query, new SqlParameter("orgId", orgId)).ToList();
                return uploadHistory;
            }
        }
    }

    public class UsersOrganisations
    {
        public int id { get; set; }
        public string userId { get; set; }
        public int organisationId { get; set; }
        public DateTime dateCreate { get; set; }
    }

    namespace Organisations {
        //таблица файлов со списками пользователей, которые загружались через инструмент импорта из файла
        public class OrganisationsUsersFile
        {
            [Key]
            public string id { get; set; }
            public string orgId { get; set; }
            public DateTime dateCreate { get; set; }
            public bool deleted { get; set; }
            public string usersData { get; set; }
            public int usersCount { get; set; }
        }

        namespace Views
        {
            public class UsersImport
            {
                public string orgId { get; set; }
                public string separator { get; set; } = ";";
                public string filename { get; set; } = null;
                public bool showResult { get; set; } = false;
                public int rowsCount { get; set; } = 0;
                public int rowsCorrect { get; set; } = 0;
                public int rowsIncorrect { get; set; } = 0;
                public int usersAdded { get; set; } = 0;
                public int usersNotAdded { get; set; } = 0;
                public List<Core.UploadFailedString> errorLog { get; set; }
                    = new List<Core.UploadFailedString>();
                public List<UsersUploadHistoryViewEntity> uploadHistory { get; set; }
                    = new List<UsersUploadHistoryViewEntity>();
            }

            public class UsersUploadHistoryViewEntity
            {
                public string id { get; set; }
                public DateTime dateCreate { get; set; }
                public int usersCount { get; set; }
            }

            public class MailTemplate
            {
                public Organisation organisation;
                public OrganisationsResearches.OrganisationsResearch research;
            }
        }
    
    }
}