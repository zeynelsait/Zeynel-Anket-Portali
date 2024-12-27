namespace AnketPortali.Models
{
    public class Survey
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public string CreatedById { get; set; }
        public AppUser CreatedBy { get; set; }

        public List<SurveyQuestion> Questions { get; set; }
    }
}
