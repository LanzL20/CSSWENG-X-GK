using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using CSSWENGxGK.Data;
using CSSWENGxGK.Models;

class Emailer
{
    private readonly string senderEmail = "CSSWENGGROUP6@outlook.com";
    private readonly string senderAppPassword = "Larvi!n<_>";

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

            using (var client = new SmtpClient("smtp.office365.com"))
            {
                client.Port = 587;
                client.Credentials = new NetworkCredential(senderEmail, senderAppPassword);
                client.EnableSsl = true;
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

            using (var client = new SmtpClient("smtp.office365.com"))
            {
                client.Port = 587;
                client.Credentials = new NetworkCredential(senderEmail, senderAppPassword);
                client.EnableSsl = true;
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

            // Add Bcc recipients
            foreach (var recipient in bccRecipients)
            {
                message.Bcc.Add(recipient);
            }

            using (var client = new SmtpClient("smtp.office365.com"))
            {
                client.Port = 587;
                client.Credentials = new NetworkCredential(senderEmail, senderAppPassword);
                client.EnableSsl = true;

                client.Send(message); // Synchronous sending for Bcc email
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending Bcc email: {ex.Message}");
            return false;
        }
    }

    static string GenerateOtp()
    {
        Random random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
