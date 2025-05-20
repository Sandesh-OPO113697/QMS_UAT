namespace QMS.Models
{
    public class Assesmentmonitor
    {
        public int TestID { get; set; }
        public string TestName { get; set; }
        public string Process { get; set; }
        public string SubProcessName { get; set; }
        public string TestCategory { get; set; }
        public DateTime CreatedDate { get; set; }
        public string expiryType { get; set; }
        public DateTime expiryDate { get; set; }
        public int expiryHours { get; set; }
    }
}
