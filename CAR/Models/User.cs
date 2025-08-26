using System.ComponentModel.DataAnnotations;

namespace CAR.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; }

        [Required, StringLength(50)]
        public string Password { get; set; } // Plain text for assignment simplicity

        [Required, StringLength(20)]
        public string Role { get; set; } // "Admin" or "Customer"
    }
}

