namespace CadastroService.Entities
{
    public class ApiResponse<T>
    {
        public string Message { get; set; }
        public T Data { get; set; }
        public bool HasError { get; set; }
    }
}
