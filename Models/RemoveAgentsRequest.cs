namespace QMS.Models
{
    public class RemoveAgentsRequest
    {
        public List<string> EmpCodes { get; set; }
        public string process { get; set; }
        public int subProcess { get; set; }
    }
}
