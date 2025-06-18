using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KomunikatorServer.Models
{
    public class ChatMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; }

        [ForeignKey("SenderId")]
        public AppUser Sender { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        [ForeignKey("ReceiverId")]
        public AppUser Receiver { get; set; }

        [Required]
        [MaxLength(500)]
        public string Content { get; set; }

        public DateTime Timestamp { get; set; }
    }
}