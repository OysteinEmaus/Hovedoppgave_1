using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hovedoppgave.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [Required]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; }

        public string Role { get; set; } = "User"; // Default rolle

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Report> Reports { get; set; }
    }
}