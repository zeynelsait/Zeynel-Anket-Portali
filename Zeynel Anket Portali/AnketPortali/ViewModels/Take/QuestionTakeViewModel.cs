namespace AnketPortali.ViewModels.Take
{
    public class QuestionTakeViewModel
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public List<OptionTakeViewModel> Options { get; set; } = new List<OptionTakeViewModel>();
    }
} 