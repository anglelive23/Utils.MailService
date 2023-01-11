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
            // Email
            var email = new MimeMessage
            {
                Sender = MailboxAddress.Parse(_mailSettings.SenderEmail),
                Subject = subject
            };

            email.To.Add(MailboxAddress.Parse(mailTo));

            // Message body
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

            // Email Body
            email.Body = builder.ToMessageBody();
            email.From.Add(new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail));

            // Sending Email
            using var smtp = new SmtpClient();
            try
            {
                smtp.Connect(_mailSettings.Server, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp.AuthenticationMechanisms.Remove("XOAUTH2");
                smtp.Authenticate(_mailSettings.SenderEmail, _mailSettings.Password);
                await smtp.SendAsync(email);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                smtp.Disconnect(true);
                smtp.Dispose();
            }
        }
    }
}
