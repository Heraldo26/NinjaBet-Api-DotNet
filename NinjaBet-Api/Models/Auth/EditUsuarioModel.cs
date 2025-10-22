namespace NinjaBet_Api.Models.Auth
{
    public class EditUsuarioModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public decimal? Saldo { get; set; }
        public string Password { get; set; } = string.Empty;
        public string Perfil { get; set; }
    }
}
