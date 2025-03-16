using System.ComponentModel.DataAnnotations;

namespace CadastroService.Entities
{
    public class ContatosUpdateRequest
    {
        public string? nome { get; set; }
        [EmailAddress(ErrorMessage = "Email em formato invalido.")]
        public string? email { get; set; }
        [RegularExpression(@"^\(\d{2}\) \d{4,5}-\d{4}$", ErrorMessage = "Telefone em formato invalido. Exemplo: (11) 91234-5678")]
        public string? telefone { get; set; }
    }
}
