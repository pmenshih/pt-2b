using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace pt_2b.Core
{
    public class BLL
    {
        public static string ReadUploadedFileToString(HttpPostedFileBase file)
        {
            byte[] fileBytes = new byte[file.ContentLength];
            file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
            return Encoding.UTF8.GetString(fileBytes);
        }

        public static string GenerateRandomDigStrCode(int len)
        {
            string chars = "1234567890qwertyuiopasdfghjklzxcvbnm";
            string code = "";
            Random rnd = new Random();
            for (int i = 0; i < len; i++)
            {
                int next = rnd.Next(0, chars.Length);
                code += chars.Substring(next, 1);
            }

            return code;
        }

        public static string GenerateRandomDigitCode(int len)
        {
            string chars = "1234567890";
            string code = "";
            Random rnd = new Random();
            for (int i = 0; i < len; i++)
            {
                int next = rnd.Next(0, 10);
                code += chars.Substring(next, 1);
            }

            return code;
        }
    }
    public class UploadFailedString
    {
        public UploadFailedString() { }
        public UploadFailedString(int rN, string rD, string fCN, string fCD)
        {
            rowNumber = rN;
            rowData = rD;
            failedColumnName = fCN;
            failedColumnData = fCD;
        }
        public int rowNumber { get; set; }
        public string rowData { get; set; }
        public string failedColumnName { get; set; }
        public string failedColumnData { get; set; }
    }

    public static class ErrorMessages
    {
        public const string LoginFail = "Неверный логин или пароль.";
        public const string LoginIncorrect = "Введен некорректный логин.";
        public const string EmailRegistered = "Указанный адрес электронной почты уже зарегистрирован.";
        public const string EmailIncorrect = "Указан неверный адрес электронной почты.";
        public const string SMSCodeIncorrect = "Указан неверный код из СМС.";
        public const string UploadFileNotSelect = "Не выбран файл для загрузки.";
        public const string UploadUsersFileLessTwoStrings = "В загруженном файле меньше двух строк.";
        public const string UploadUsersFileNoEmail = "В файле отсутствует поле \"email\".";
        public const string UploadFileNoSeparator = "Не указан разделитель столбцов.";
        public const string ResearchCreateValidate = "Не указаны обязательные параметры.";
        public const string ResearchIncorrectPassword = "Анкеты с указанным кодовым словом не найдено.";
        public const string ResearchNotActive = "Заполнение анкеты в настоящий момент невозможно. Попробуйте позже, или свяжитесь с администратором анкеты.";
        public const string ResearchNoActiveScenario = "У исследования нет активной анкеты.";
        public const string ResearchSessionNotExist = "Сессия с указанным идентификатором не найдена.";
    }
}