namespace QMS.Models
{
    public class AttemptTestViewModel
    {
        public int TestID { get; set; }
        public string TestName { get; set; }
        public string TestCategory { get; set; }

        public List<QuestionViewModel> Questions { get; set; }


    }
}
