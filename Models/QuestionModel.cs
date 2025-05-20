namespace QMS.Models
{
    public class QuestionModel
    {
        public string question { get; set; }
        public string answerType { get; set; }
        public List<string> options { get; set; }
        public List<int> correctAnswers { get; set; }
        public string programId { get; set; }
        public string SUBProgramID { get; set; }
        public string category { get; set; }
        public string subject { get; set; }
        public string expiryType { get; set; }
        public string expiryDate { get; set; }
        public string expiryHours { get; set; }
    }
}
