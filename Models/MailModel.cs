namespace QMS.Models
{
    public class MailModel
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }               // HTML content
        public string AttachmentBase64 { get; set; }   // File as base64 string
        public string AttachmentFileName { get; set; }
        public List<string> AgentCodes { get; set; }
    }
}
