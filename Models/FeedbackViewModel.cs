namespace QMS.Models
{
    public class FeedbackViewModel
    {
        public string MonitoringId { get; set; }
        public string AgentId { get; set; }
        public string AgentComment { get; set; }
        public List<FeedbackQuestion> Questions { get; set; }
    }
}
