namespace QMS.Models
{
    public class SectionUpdateModel
    {
        public string Category { get; set; }
        public string Section { get; set; }

        public string Scorable { get; set; }
        public int Score { get; set; }
        public string Level { get; set; }

        public string ProgramID { get; set; }
        public string SubProgramID { get; set; }
    }
}
