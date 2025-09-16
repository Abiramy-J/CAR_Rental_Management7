namespace Car_Rental_Management.ViewModels
{
    public class EditProfileViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? ProfileImageUrl { get; set; }

        // For file upload
        public IFormFile? ProfileImage { get; set; }
    }
}
