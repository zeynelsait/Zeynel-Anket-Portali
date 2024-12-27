namespace AnketPortali.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string Message { get; set; }
        public DateTime SendTime { get; set; }
        public bool IsDeleted { get; set; }
        
        public AppUser Sender { get; set; }
    }
} 