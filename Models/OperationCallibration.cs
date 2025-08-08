namespace QMS.Models
{
    public class OperationCallibration
    {
        public string category { get; set; }
        public string level { get; set; }
        public string sectionName { get; set; }
        public string parameters { get; set; }
        public string subparameters { get; set; }
     
        public string qaRating { get; set; }
        public string scorable { get; set; }
        public string score { get; set; }
        public string comments { get; set; }
        public string Transaction_ID { get; set; }
        public string ProgramID { get; set; }

        public string SUBProgramID { get; set; }
        public string fatal { get; set; }
        public string Comment { get; set; }
    }
}
