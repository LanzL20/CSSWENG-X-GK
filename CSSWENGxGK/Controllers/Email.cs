using System;
using System.Net;
using System.Net.Mail;
using CSSWENGxGK.Data;
using CSSWENGxGK.Models;
using System.Threading;

class Emailer
{
    public string Send_OTP(string recipientEmail)
    {
        try
        {
            // Your Outlook/Hotmail email address
            string senderEmail = "CCAPDEV.LLL@outlook.com";

            // Use an App Password or your Microsoft account password
            string senderAppPassword = "Larvi!n<_>";

            Random random = new Random();
            string OTP_code = random.Next(100000, 1000000).ToString();

            var message = new MailMessage(senderEmail, recipientEmail)
            {
                Subject = "Gawad Kalinga Login OTP",
                Body = OTP_code,
                IsBodyHtml = false
            };

            using (var client = new SmtpClient("smtp-mail.outlook.com"))
            {
                client.Port = 587;
                client.Credentials = new NetworkCredential(senderEmail, senderAppPassword);
                client.EnableSsl = true;
                Thread.Sleep(5000);
                client.Send(message);
                return OTP_code;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending test email: {ex.Message}");
            return "-1";
        }
    }
 
}
