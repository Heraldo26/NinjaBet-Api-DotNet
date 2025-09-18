namespace NinjaBet_Dmain.Entities.Log
{
    public class LogErro
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Mensagem { get; private set; } = string.Empty;
        public string? StackTrace { get; private set; }
        public string Nivel { get; private set; } = "Erro"; // Informação, Aviso, Erro, Crítico
        public DateTime Data { get; private set; } = DateTime.UtcNow;

        public LogErro(string mensagem, string? stackTrace = null, string nivel = "Erro")
        {
            Mensagem = mensagem;
            StackTrace = stackTrace;
            Nivel = nivel;
        }
    }
}
