namespace QMS.Models
{

    public class ZtSignOffSectionDatas
    {
        public List<ZtSignOffSectionData> sections { get; set; }
    }
    public class ZtSignOffSectionData
    {
        public string Parameter { get; set; }
        public string Procedure1 { get; set; }
        public string Procedure2 { get; set; }
        public string Procedure3 { get; set; }
        public string Procedure4 { get; set; }
        public string Procedure5 { get; set; }
        public string Procedure6 { get; set; }
        public string Procedure7 { get; set; }
        public string Procedure8 { get; set; }
        public string Procedure9 { get; set; }
        public string Procedure10 { get; set; }

        // New fields
        public string ProgramID { get; set; }
        public string SUBProgramID { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
