namespace QMS.Models
{
    public class AccountModel
    {
        public string AccountID { get; set; }
        public string AccountName { get; set; }
        public string AuthenticationType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
