namespace QMS.Models
{
    public class AuditEntry
    {
        public string Type { get; set; }
        public DateTime Time { get; set; }
        public double? PauseDuration { get; set; }
    }
}
