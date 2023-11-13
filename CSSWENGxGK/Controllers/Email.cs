using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

class Emailer
{
    private static string senderEmail = "CCAPDEV.LLL@outlook.com";
    private static string senderAppPassword = "Larvi!n<_>";

    public async Task<string> Send_OTP(string recipientEmail)
    {
        try
        {
            // Use a more secure method for OTP generation
            string otp = GenerateOtp();

            var message = new MailMessage(senderEmail, recipientEmail)
            {
                Subject = "Gawad Kalinga Login OTP",
                Body = otp,
                IsBodyHtml = false
            };

            using (var client = new SmtpClient("smtp-mail.outlook.com"))
            {
                client.Port = 587;
                client.Credentials = new NetworkCredential(senderEmail, senderAppPassword);
                client.EnableSsl = true;

                // Remove Thread.Sleep
                await client.SendMailAsync(message);
                return otp;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending OTP email: {ex.Message}");
            return "-1";
        }
    }

    public async Task<bool> Send_Welcome(string recipientEmail)
    {
        try
        {
            var message = new MailMessage(senderEmail, recipientEmail)
            {
                Subject = "Welcome to Gawad Kalinga",
                Body = "Your account has been successfully registered. You may now login with this email address.",
                IsBodyHtml = false
            };

            using (var client = new SmtpClient("smtp-mail.outlook.com"))
            {
                client.Port = 587;
                client.Credentials = new NetworkCredential(senderEmail, senderAppPassword);
                client.EnableSsl = true;

                // Remove Thread.Sleep
                await client.SendMailAsync(message);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending welcome email: {ex.Message}");
            return false;
        }
    }

    static string GenerateOtp()
    {
        // Use a more secure method for OTP generation, for example, using a cryptographic library
        Random random = new Random();
        return random.Next(100000, 1000000).ToString();
    }
}
