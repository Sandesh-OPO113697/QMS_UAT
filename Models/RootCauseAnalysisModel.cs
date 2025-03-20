namespace QMS.Models
{
    public class RootCauseAnalysisModel
    {
        public string metricRCA { get; set; }
        public string controllable { get; set; }
        public string rca1 { get; set; }
        public string rca2 { get; set; }
        public string rca3 { get; set; }
        public string comments { get; set; }
        public string Transaction_ID { get; set; }
        public string ProgramID { get; set; }

        public string SUBProgramID { get; set; }
    }
}
