namespace QMS.Models
{
    public class AgentDashboardViewModel
    {
        public List<AgentFeedBackDetails> FeedbackList { get; set; }
        public List<AgentFeedBackDetails> DisputeList { get; set; }

        public List<ZtSignOffDataAgentWise> ZtSignOffDataAgent { get; set; }
        public List<AssesmentModel> assmentonl { get; set; }
        public List<AgentFeedBackDetails> agentsurvey { get; set; }
        public List<PerformanceMetric> performanceMatrix { get; set; }
        public List<LastUpdate> lastUpdate { get; set; }
        public MonthlySummary monthlyData { get; set; } // 👈 new property


    }
}
