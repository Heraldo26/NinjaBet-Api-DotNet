namespace NinjaBet_Api.Models.Auth
{
    public class CreateUsuarioModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Perfil { get; set; } = "Apostador"; // default
    }
}
