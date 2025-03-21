namespace QMS.Models
{
    public class AuditRecord
    {
        public DateTime StartTime { get; set; }
        public DateTime PauseTime { get; set; }
        public DateTime EndTime { get; set; }
        public int IsPaused { get; set; }
    }
}
