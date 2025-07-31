namespace QMS.Models
{
    public class MonitoredSectionGridModel
    {
    
        public string category { get; set; }
        public string level { get; set; }
        public string SectionName { get; set; }
        public string parameters { get; set; }
        public string subparameters { get; set; }


        public string QA_rating { get; set; }
        public string Scorable { get; set; }
        public string Weightage { get; set; }

        public string Commentssection { get; set; }
        public string Fatal { get; set; }
        public string ratingdrop { get; set; }
    }
}
