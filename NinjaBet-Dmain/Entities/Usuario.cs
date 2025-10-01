using NinjaBet_Dmain.Enums;
using System.ComponentModel.DataAnnotations;

namespace NinjaBet_Dmain.Entities
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public PerfilAcessoEnum Perfil { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }


        public Usuario() { }
        public Usuario(string username, string passwordHash, PerfilAcessoEnum perfil)
        {
            Username = username;
            PasswordHash = passwordHash;
            Perfil = perfil;
            Ativo = true;
            DataCriacao = DateTime.UtcNow;
        }

        // Métodos de domínio
        public void AlterarPerfil(PerfilAcessoEnum novoPerfil) => Perfil = novoPerfil;
        public void Desativar() => Ativo = false;
        public void Ativar() => Ativo = true;
    }
}
