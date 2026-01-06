using BLL.Interfaces;
using FluentEmail.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class FluentEmailService : IEmailService
    {
        private readonly IFluentEmailFactory _emailFactory;
        private readonly ILogger<FluentEmailService> _logger;

        public FluentEmailService(IFluentEmailFactory emailFactory, ILogger<FluentEmailService> logger)
        {
            _emailFactory = emailFactory;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            try
            {
                var email = _emailFactory
                    .Create()
                    .To(to)
                    .Subject(subject)
                    .Body(body, isHtml: true);

                await email.SendAsync();
                _logger.LogInformation("Email sent to {To}", to);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(string to, string username)
        {

            try
            {
                var email = _emailFactory
                    .Create()
                    .To(to)
                    .Subject("Welcome to Double M Platform!")
                    .Body($"<h1>Welcome, {username}!</h1><p>Thank you for joining our service.</p>", isHtml: true);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {To}", to);
                throw;
            }
        }
    }
}
