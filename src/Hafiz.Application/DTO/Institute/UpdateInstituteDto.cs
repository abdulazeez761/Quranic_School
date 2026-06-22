using System.ComponentModel.DataAnnotations;

namespace Hafiz.DTOs
{
    public class UpdateInstituteDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "اسم المركز مطلوب")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "اسم المركز")]
        public string Name { get; set; }

        [StringLength(500)]
        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        [StringLength(255)]
        [Display(Name = "العنوان")]
        public string? Address { get; set; }

        [Phone]
        [StringLength(20)]
        [Display(Name = "هاتف المركز")]
        public string? PhoneNumber { get; set; }
    }
}
