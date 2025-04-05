namespace QMS.Models
{
    public class MonitorFormModel
    {
        public string AuditID { get; set; }
        public string ProgramID { get; set; }
        public string SUBProgramID { get; set; }
        public string dispositionId { get; set; }
        public string SubDispositionID { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string AgentID { get; set; }
        public string TL_id { get; set; }
        public string Transaction_ID { get; set; }
        public string Monitored_date { get; set; }
        public string Transaction_Date { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public string Week { get; set; }
        public string ZTClassification { get; set; }
        public string ZeroTolerance { get; set; }
        public string CQ_Scrore { get; set; }

        public string Remarks { get; set; }
    }
}
