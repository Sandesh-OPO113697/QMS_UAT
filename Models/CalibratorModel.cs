namespace QMS.Models
{
    public class CalibratorModel
    {
        public string ProgramId { get; set; }
        public string SubProgram { get; set; }
        public string transactionID { get; set; }
        public string CalibratedComment { get; set; }
        public List<string> SelectedParticipants { get; set; }
    }
}
