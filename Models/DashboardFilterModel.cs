namespace QMS.Models
{
    public class DashboardFilterModel
    {
        public string Program { get; set; }
        public string SubProgram { get; set; }
        public string Filter { get; set; }  // day, week, or month
    }
}
