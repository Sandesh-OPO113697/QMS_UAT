using Microsoft.AspNetCore.Mvc.Rendering;

namespace QMS.Models
{
    public class AgentFeedBackSectionList
    {
        public List<MonitoredSectionGridModel> sectionList { get; set; }
        public List<SelectListItem> filteredRatingList { get; set; }
    }
}
