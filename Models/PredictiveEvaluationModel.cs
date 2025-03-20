namespace QMS.Models
{
    public class PredictiveEvaluationModel
    {
        public string PredictiveCSAT { get; set; }
        public string PredictiveNPS { get; set; }
        public string PredictiveFCR { get; set; }
        public string PredictiveRepeat { get; set; }
        public string PredictiveSalesEffort { get; set; }
        public string PredictiveCollectionEffort { get; set; }
        public string PredictiveEscalation { get; set; }
        public string Transaction_ID { get; set; }
        public string ProgramID { get; set; }

        public string SUBProgramID { get; set; }
    }
}
