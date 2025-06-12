namespace QMS.Models
{
    public class PerformanceMetric
    {
        public string AgentID { get; set; }
        public string Matrix { get; set; }
        public int Target { get; set; }
        public int Actual_Performance { get; set; }
    }
}
