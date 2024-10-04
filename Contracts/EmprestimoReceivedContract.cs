using BibliotecaFunctionApp.Contracts.Dtos;

namespace BibliotecaFunctionApp.Contracts
{
    public class EmprestimoReceivedContract
    {
        public string MessageId { get; set; }
        public CreateEmprestimoDto Message { get; set; }
    }
}
