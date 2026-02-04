using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMAP.Web.Services.Email
{
    public interface IEmailService
    {
        Task SendAsync(IEnumerable<string> toEmails, string subject, string htmlBody);
    }
}
