using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace evocontest.WebApp.Core
{
    public sealed class EmailSender : IEmailSender
    {
        public EmailSender(IConfiguration config)
        {
            var apiKey = config.GetValue<string>("SendGridKey");
            myClient = new SendGridClient(apiKey);
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var from = new EmailAddress("evocontest@evosoft.com", "evocontest");
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, htmlMessage, htmlMessage);
            var response = await myClient.SendEmailAsync(msg);
        }

        private readonly SendGridClient myClient;
    }
}
