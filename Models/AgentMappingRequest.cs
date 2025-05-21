namespace QMS.Models
{
    public class AgentMappingRequest
    {
        public string ProcessName { get; set; }
        public string SubProcessName { get; set; }
        public List<string> AgentIds { get; set; }
        public bool NotepadAccess { get; set; }
    }
}
