namespace AnketPortali.Models
{
    public class SurveyResponse
    {
     


        public int Id { get; set; }
        public int? SurveyId { get; set; }
        public Survey Survey { get; set; }

        public int? QuestionId { get; set; }
        public SurveyQuestion Question { get; set; }

        public int? SelectedOptionId { get; set; }
        public SurveyOption SelectedOption { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public DateTime ResponseDate { get; set; }
    }
}
