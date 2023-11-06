using System;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Mvc;

namespace CSSWENGxGK.Controllers
{
    public class MailController : Controller
    {
        private static string credentialsFilePath = ".json";
        private static string tokenFilePath = ".json";

        public ActionResult SendEmail()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendEmail(string toAddress, string subject, string body)
        {
            try
            {
                UserCredential credential;
                using (var stream = new FileStream(credentialsFilePath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        new[] { GmailService.Scope.MailGoogleCom },
                        "user",
                        CancellationToken.None,
                        new FileDataStore(tokenFilePath, true)
                    ).Result;
                }

                var service = new GmailService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Your Application Name"
                });

                var email = CreateEmail(toAddress, subject, body);
                var encodedEmail = Base64UrlEncodeEmail(email);

                var gmailMessage = new Google.Apis.Gmail.v1.Data.Message
                {
                    Raw = encodedEmail
                };

                service.Users.Messages.Send(gmailMessage, "me").Execute();

            }
            catch (Exception ex)
            {
            }

            return View(); // Return to the same View after sending the email
        }

        private Google.Apis.Gmail.v1.Data.Message CreateEmail(string toAddress, string subject, string body)
        {
            var email = new Google.Apis.Gmail.v1.Data.Message();
            var mimeMessage = new System.Net.Mail.MailMessage();
            mimeMessage.From = new System.Net.Mail.MailAddress("your@gmail.com");
            mimeMessage.To.Add(toAddress);
            mimeMessage.Subject = subject;
            mimeMessage.Body = body;

            var mimeMessageBytes = System.Text.Encoding.UTF8.GetBytes(mimeMessage.ToString());
            email.Raw = Convert.ToBase64String(mimeMessageBytes);

            return email;
        }

        private string Base64UrlEncodeEmail(Google.Apis.Gmail.v1.Data.Message email)
        {
            return email.Raw.Replace('+', '-').Replace('/', '_').Replace("=", "");
        }
    }
}
