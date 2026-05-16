using System.ComponentModel.DataAnnotations;

namespace DOANCOSO26.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string ConversationId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string SenderId { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string SenderName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string SenderRole { get; set; } = string.Empty;

        [Required]
        public string MessageText { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; }
    }
}
