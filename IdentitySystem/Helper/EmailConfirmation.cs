using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace IdentitySystem.Helper
{
    public static class EmailConfirmation
    {

        public static void SendEmail(string link, string email)
        {
            MailMessage mail = new MailMessage();

            // bunu host firmanızdan öğrenilir
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");

            // email kulakberkay15@gmail.com den  beko_468@hotmail.com ' e bir tane email gelecek
            mail.From = new MailAddress("kulakberkay15@gmail.com");

            // kime gidicek email burada belirtiyoruz
            mail.To.Add(email);
            mail.Subject = $"www.bıdıbıdı.com::Email doğrulama";
            mail.Body = "<h2>Email adresinizi doğrulamak için lütfen aşağıdaki linke tıklayınız.</h2><hr/>";
            mail.Body += $"<a href = '{link}'> email doğrulama linki </a>";
            mail.IsBodyHtml = true;
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = new System.Net.NetworkCredential("Your Mail Adress", "Your Password");
            smtpClient.Send(mail);

        }


    }
}
