namespace LibraryManagement.API.Services.Interface
{
    public interface IEmailMessageService
    {
        Task SendMessage(string email, string subject, string bodyHtml);
        Task SendMessageWithAttachment(string email, string subject, string bodyHtml, byte[] attachmentData, string attachmentFileName);
    }
}
