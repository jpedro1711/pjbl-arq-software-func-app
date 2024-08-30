using BibliotecaFunctionApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace BibliotecaFunctionApp
{
    public class EmprestimosFunction
    {
        private readonly ILogger<EmprestimosFunction> _logger;

        public EmprestimosFunction(ILogger<EmprestimosFunction> logger)
        {
            _logger = logger;
        }

        [Function("GetEmprestimos")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            var client = new MongoClient(System.Environment.GetEnvironmentVariable("MongoDBAtlasConnectionString"));
            var database = client.GetDatabase("biblioteca");
            var collection = database.GetCollection<Emprestimo>("emprestimos");


            _logger.LogInformation("GET EMPRESTIMOS - http trigger");

            var emprestimos = collection.Find(_ => true).ToList();

            return new OkObjectResult(emprestimos);
        }

        [Function("CreateEmprestimo")]
        public async Task<IActionResult> Get([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            var client = new MongoClient(System.Environment.GetEnvironmentVariable("MongoDBAtlasConnectionString"));
            var database = client.GetDatabase("biblioteca");
            var collection = database.GetCollection<Emprestimo>("emprestimos");

            string reqBody = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation("CREATE EMPRESTIMO - http trigger");

            // Convertendo a string JSON para o objeto Emprestimo
            var novoEmprestimo = JsonConvert.DeserializeObject<Emprestimo>(reqBody);

            // Insere o novo empréstimo no MongoDB
            await collection.InsertOneAsync(novoEmprestimo);

            return new OkObjectResult(novoEmprestimo);
        }
    }
}
