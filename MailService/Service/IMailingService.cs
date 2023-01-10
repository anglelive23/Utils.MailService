

namespace EmailService.Service
{
    public interface IMailingService
    {
        Task SendMailAsync(string mailTo, string subject, string body, IList<IFormFile> attatchments = null);
    }
}
