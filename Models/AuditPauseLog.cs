namespace QMS.Models
{
    public class AuditPauseLog
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalDuration { get; set; }
        public int TotalPauseCount { get; set; }
        public double TotalPauseDuration { get; set; }
        public string Transaction_ID { get; set; }
        public string ProgramID { get; set; }
        public string SUBProgramID { get; set; }
        public List<AuditEntry> Logs { get; set; }
    }
}
