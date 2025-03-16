using System.ComponentModel.DataAnnotations;

namespace ConsultaService.Entities
{

    public class ContatosRequest
    {
        [Required(ErrorMessage = "Nome e obrigatorio.")]
        public string nome { get; set; }

        [Required(ErrorMessage = "Email e obrigatorio.")]
        [EmailAddress(ErrorMessage = "Email em formato invalido. Exemplo: email@exemple.com")]
        public string email { get; set; }

        [Required(ErrorMessage = "Telefone e obrigatorio.")]
        [RegularExpression(@"^\(\d{2}\) \d{4,5}-\d{4}$", ErrorMessage = "Telefone em formato invalido. Exemplo: (11) 91234-5678")]
        public string telefone { get; set; }
    }
}
