namespace QMS.Models
{
    public class SectionUpdateModel
    {
        public string Category { get; set; }
        public string Parameters { get; set; }
                  
        public string SubParameters { get; set; }
        public string Ratingid { get; set; }
        public string Fatal { get; set; }
        public string SectionName { get; set; }
    


        public string Scorable { get; set; }
        public int Score { get; set; }
        public string Level { get; set; }

        public string ProgramID { get; set; }
        public string SubProgramID { get; set; }
    }
}
