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
        [ServiceBusOutput("teste_topic", Connection = "ServiceBusConnectionString")]
        public async Task<string> Run(
        [ServiceBusTrigger("emprestimos-queue", Connection = "ServiceBusConnectionString")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
        {
            _logger.LogWarning("Message ID: {id}", message.MessageId);
            _logger.LogWarning("Message Body: {body}", message.Body);
            _logger.LogWarning("Message Content-Type: {contentType}", message.ContentType);

            string messageBody = message.Body.ToString();

            var emprestimoMessage = JsonConvert.DeserializeObject<EmprestimoReceivedContract>(messageBody);

            await messageActions.CompleteMessageAsync(message);

            return messageBody;
        }


        [Function(nameof(ServiceBusReceivedMessageFunction))]
        public void ServiceBusReceivedMessageFunction(
        [ServiceBusTrigger("teste_topic", "subscription1", Connection = "ServiceBusConnectionString")]
        ServiceBusReceivedMessage message)
        {
            _logger.LogWarning("Mensagem recebida e consumida do tópico - funcao01");
        }

        [Function(nameof(ServiceBusReceivedMessageFunction2))]
        public void ServiceBusReceivedMessageFunction2(
            [ServiceBusTrigger("teste_topic", "subscription2", Connection = "ServiceBusConnectionString")]
            ServiceBusReceivedMessage message)
        {
            _logger.LogWarning("Mensagem recebida e consumida do tópico - funcao02");
        }

    }
}
