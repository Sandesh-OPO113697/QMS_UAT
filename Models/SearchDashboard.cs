namespace QMS.Models
{
    public class SearchDashboard
    {

        public string Program { get; set; }
        public string SubProgram { get; set; }
        public string Filter { get; set; }  // day, week, or month

        public string selectedDate { get; set; }

    }
}
