namespace QMS.Models
{
    public class AgentDashboardViewModel
    {
        public List<AgentFeedBackDetails> FeedbackList { get; set; }
        public List<AgentFeedBackDetails> DisputeList { get; set; }

        public List<ZtSignOffDataAgentWise> ZtSignOffDataAgent { get; set; }
        public List<AssesmentModel> assmentonl { get; set; }
    }
}
