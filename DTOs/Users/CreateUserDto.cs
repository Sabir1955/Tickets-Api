namespace Authentication.DTOs.Users
{
    public class CreateUserDto
    {
        public string Name { get; set; } = "";

        public string Email { get; set; } = "";

        public string Password { get; set; } = "";
    }
}