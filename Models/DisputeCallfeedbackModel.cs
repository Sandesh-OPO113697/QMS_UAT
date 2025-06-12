namespace QMS.Models
{
    public class DisputeCallfeedbackModel
    {
        public string Process { get; set; }
        public string SubProcessName { get; set; }

        public string TransactionID { get; set; }
        public string AgentID { get; set; }
        public string TLName { get; set; }
        public string MonitorBy { get; set; }
        public string AgentDispute { get; set; }
        public DateTime CreatedDate { get; set; }
    }


}
