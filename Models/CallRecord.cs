namespace QMS.Models
{
    public class CallRecord
    {
        public double ID { get; set; }
        public string AgentId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string CallDuration { get; set; }
        public string SourcePath { get; set; }
        public string NewPath { get; set; }
        public string CPath { get; set; }
        public string CONNID { get; set; }
    }
}
