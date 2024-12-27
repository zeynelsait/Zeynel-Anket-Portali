namespace AnketPortali.Models
{
    public class SurveyQuestion
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public Survey Survey { get; set; }
        public string QuestionText { get; set; }
        public List<SurveyOption> Options { get; set; } = new List<SurveyOption>();
    }

}
