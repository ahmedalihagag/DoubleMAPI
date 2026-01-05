using BLL.Interfaces;
using FluentEmail.Core;
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

        public FluentEmailService(IFluentEmailFactory emailFactory)
        {
            _emailFactory = emailFactory;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            // Create a new email each time
            var email = _emailFactory
                .Create()
                .To(to)
                .Subject(subject)
                .Body(body, isHtml: true);

            await email.SendAsync();
        }
    }
}
