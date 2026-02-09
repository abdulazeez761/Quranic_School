using System;
using System.ComponentModel.DataAnnotations;

namespace Hifz.Models
{
    public class Video
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(150)] // Max length for title
        public string Title { get; set; }

        [Required]
        [StringLength(500)] // Max length for description
        public string Description { get; set; }

        [Required]
        public VideoCategory Category { get; set; }

        [Required]
        [StringLength(50)] // Typical length for YouTube video ID is 11, adding buffer
        public string YoutubeId { get; set; }

        [Required]
        [StringLength(255)] // Max length for thumbnail URL or path
        public string Thumbnail { get; set; }
    }
}
