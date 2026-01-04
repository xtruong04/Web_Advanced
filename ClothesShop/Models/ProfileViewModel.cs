namespace ClothesShop.Models
{
    public class ProfileViewModel 
    {
        public ApplicationUser User { get; set; }
        public Address? DefaultAddress { get; set; }
        public string AvatarUrl { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public IFormFile AvatarFile { get; set; }
    }

}
