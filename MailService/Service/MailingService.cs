namespace EmailService.Service
{
    public class MailingService : IMailingService
    {
        private readonly MailSettings _mailSettings;
        public MailingService(IOptions<MailSettings> mailSettings)
        {
            this._mailSettings = mailSettings.Value;
        }
        public async Task SendMailAsync(string mailTo, string subject, string body, IList<IFormFile> attatchments = null)
        {
            var email = new MimeMessage
            {
                Sender = MailboxAddress.Parse(_mailSettings.SenderEmail),
                Subject = subject
            };

            email.To.Add(MailboxAddress.Parse(mailTo));

            var builder = new BodyBuilder();
            if(attatchments != null)
            {
                byte[] fileBytes;
                foreach (var file in attatchments)
                {
                    if(file.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        file.CopyTo(memoryStream);
                        fileBytes= memoryStream.ToArray();

                        builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    }
                }
            }

            builder.HtmlBody= body;
            email.Body = builder.ToMessageBody();
            email.From.Add(new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail));

            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Server, _mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_mailSettings.SenderEmail, _mailSettings.Password);

            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }
    }
}
