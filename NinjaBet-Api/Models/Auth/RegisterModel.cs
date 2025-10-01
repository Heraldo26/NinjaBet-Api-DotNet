namespace NinjaBet_Api.Models.Auth
{
    public class RegisterModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Perfil { get; set; } = "Apostador"; // default
    }
}
