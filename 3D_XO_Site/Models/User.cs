using System.ComponentModel.DataAnnotations;

namespace _3D_XO_Site.Models
{
    public class User
    {
        [ScaffoldColumn(false)]
        public string Id { get; set; }

        [Required]
        [Display(Name = "UserName")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "IsActive")]
        [UIHint("Boolean")]
        public bool IsActive { get; set; }

        [Display(Name = "Violations")]
        public int Violation { get; set; }
    }
}
