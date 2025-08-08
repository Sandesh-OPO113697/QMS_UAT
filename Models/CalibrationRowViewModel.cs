namespace QMS.Models
{
    public class CalibrationRowViewModel
    {
        public string Category { get; set; }
        public string Level { get; set; }
        public string SectionName { get; set; }
        public Dictionary<string, CalibrationParticipantData> ParticipantData { get; set; } = new();
    }
}
