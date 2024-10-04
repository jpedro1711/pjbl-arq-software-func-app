using Azure.Messaging.ServiceBus;
using BibliotecaFunctionApp.Contracts;
using BibliotecaFunctionApp.Models;
using BibliotecaFunctionApp.Models.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace BibliotecaFunctionApp.Functions
{
    public class CreateEmprestimo
    {
        private readonly ILogger<CreateEmprestimo> _logger;
        private readonly IMongoCollection<Emprestimo> collection;

        public CreateEmprestimo(ILogger<CreateEmprestimo> logger)
        {
            _logger = logger;
            var client = new MongoClient(Environment.GetEnvironmentVariable("MongoDBAtlasConnectionString"));
            var database = client.GetDatabase("biblioteca");
            collection = database.GetCollection<Emprestimo>("emprestimos");
        }

        [Function(nameof(CreateEmprestimo))]
        public async Task Run(
            [ServiceBusTrigger("emprestimos-queue", Connection = "ServiceBusConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            string messageBody = message.Body.ToString();

            var emprestimoMessage = JsonConvert.DeserializeObject<EmprestimoReceivedContract>(messageBody);

            if (emprestimoMessage?.Message != null)
            {
                _logger.LogInformation("Reserva ID: {reservaId}", emprestimoMessage.Message.ReservaId);
                _logger.LogInformation("Data de Devolução: {dataDevolucao}", emprestimoMessage.Message.DataDevolucao);
            }

            var data = emprestimoMessage.Message;

            Emprestimo emprestimo = new Emprestimo { ReservaId = data.ReservaId, DataDevolucao = data.DataDevolucao, StatusEmprestimo = StatusEmprestimo.Ativo };

            await collection.InsertOneAsync(emprestimo);

            _logger.LogInformation("Emprestimo inserido com ID: {emprestimoId}", emprestimo.Id);

            await messageActions.CompleteMessageAsync(message);
        }
    }
}
