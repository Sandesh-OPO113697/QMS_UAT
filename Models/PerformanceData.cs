namespace QMS.Models
{
    public class PerformanceData
    {
        public string ProgramID { get; set; }
        public string SUBProgramID { get; set; }
        public string AgentID { get; set; }
        public List<string> SelectedPerformance { get; set; }
    }
}
