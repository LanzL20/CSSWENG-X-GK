using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using CSSWENGxGK.Models;

class Emailer
{
    private readonly string senderEmail = "ccapdev.lll@gmail.com";
    private readonly string sendInBlueApiKey = "DMtZGbrOxFI1QJcN";
    private readonly string smtpHost = "smtp-relay.sendinblue.com";
    private readonly int smtpPort = 587;

    public async Task<string> Send_OTP(string recipientEmail)
    {
        try
        {
            string otp = GenerateOtp();

            var message = new MailMessage(senderEmail, recipientEmail)
            {
                Subject = "Gawad Kalinga Login OTP",
                Body = $"Your one-time password (OTP) is: {otp}",
                IsBodyHtml = false
            };

            using (var client = new SmtpClient(smtpHost))
            {
                client.Port = smtpPort;
                client.Credentials = new NetworkCredential(senderEmail, sendInBlueApiKey);
                client.EnableSsl = true;
                await client.SendMailAsync(message);
                return otp;
            }
        }
        catch (Exception ex)
        {
            // Log or handle the exception appropriately
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
                Body = "Your account has been successfully registered. You may now log in with this email address.",
                IsBodyHtml = false
            };

            using (var client = new SmtpClient(smtpHost))
            {
                client.Port = smtpPort;
                client.Credentials = new NetworkCredential(senderEmail, sendInBlueApiKey);
                client.EnableSsl = true;
                await client.SendMailAsync(message);
                return true;
            }
        }
        catch (Exception ex)
        {
            // Log or handle the exception appropriately
            Console.WriteLine($"Error sending welcome email: {ex.Message}");
            return false;
        }
    }

    public bool Send_Notif_Email(List<string> bccRecipients, Event eventDetails)
    {
        try
        {
            var message = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = "Notification for Active Volunteers",
                Body = $"Thank you for being active volunteers. Here are the details of the latest event:\n\nEvent Name: {eventDetails.EventName}\nDate: {eventDetails.EventDate}\nLocation: {eventDetails.EventLocation}\nShort Description: {eventDetails.EventShortDesc}\nLong Description: {eventDetails.EventLongDesc}",
                IsBodyHtml = false
            };

            foreach (var recipient in bccRecipients)
            {
                message.Bcc.Add(recipient);
            }

            using (var client = new SmtpClient(smtpHost))
            {
                client.Port = smtpPort;
                client.Credentials = new NetworkCredential(senderEmail, sendInBlueApiKey);
                client.EnableSsl = true;
                client.Send(message);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending Bcc email: {ex.Message}");
            return false;
        }
    }

    public bool Send_Near_Expire(List<string> bccRecipients)
    {
        try
        {
            var message = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = "Account almost inactive",
                Body = "Your account will be inactive in a month. Please go to your profile page and reactivate your account.",
                IsBodyHtml = false
            };

            foreach (var recipient in bccRecipients)
            {
                message.Bcc.Add(recipient);
            }

            using (var client = new SmtpClient(smtpHost))
            {
                client.Port = smtpPort;
                client.Credentials = new NetworkCredential(senderEmail, sendInBlueApiKey);
                client.EnableSsl = true;
                client.Send(message);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending Bcc email: {ex.Message}");
            return false;
        }
    }

    private static string GenerateOtp()
    {
        // Use a more secure method for OTP generation
        Random random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
