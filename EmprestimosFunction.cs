using BibliotecaFunctionApp.Models;
using BibliotecaFunctionApp.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace BibliotecaFunctionApp
{
    public class EmprestimosFunction
    {
        private readonly ILogger<EmprestimosFunction> _logger;
        private readonly IMongoCollection<Emprestimo> collection;
        private readonly HttpClient httpClient;

        public EmprestimosFunction(ILogger<EmprestimosFunction> logger)
        {
            _logger = logger;
            httpClient = new HttpClient();
            var client = new MongoClient(System.Environment.GetEnvironmentVariable("MongoDBAtlasConnectionString"));
            var database = client.GetDatabase("biblioteca");
            collection = database.GetCollection<Emprestimo>("emprestimos");
        }

        [Function("GetEmprestimos")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogInformation("GET EMPRESTIMOS - http trigger");

            var emprestimos = collection.Find(_ => true).ToList();

            return new OkObjectResult(emprestimos);
        }

        [Function("CreateEmprestimo")]
        public async Task<IActionResult> Get([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            string reqBody = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation("CREATE EMPRESTIMO - http trigger");

            var novoEmprestimo = JsonConvert.DeserializeObject<Emprestimo>(reqBody);

            string apiUrl = "http://localhost:5175/reserva/" + novoEmprestimo.ReservaId;

            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(apiUrl);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return new BadRequestObjectResult("Reserva inválida");
            }

            await collection.InsertOneAsync(novoEmprestimo);

            return new OkObjectResult(novoEmprestimo);
        }

        [Function("GetEmprestimoById")]
        public IActionResult GetById([HttpTrigger(AuthorizationLevel.Function, "get", Route = "emprestimos/{id}")] HttpRequest req, string id)
        {
            _logger.LogInformation("GET BY ID EMPRESTIMO - http trigger");

            if (!ObjectId.TryParse(id, out var objectId))
            {
                return new BadRequestObjectResult("ID inválido.");
            }

            var result = collection.Find(emp => emp.Id == objectId).FirstOrDefault();

            if (result != null)
            {
                return new OkObjectResult(result);
            }

            return new NotFoundObjectResult("Empréstimo não encontrado");
        }

        [Function("UpdateStatusEmprestimo")]
        public async Task<IActionResult> UpdateStatus([HttpTrigger(AuthorizationLevel.Function, "patch", Route = "emprestimos/{id}/{status}")] HttpRequest req, string id, string status)
        {
            _logger.LogInformation("UPDATE EMPRESTIMO - http trigger");

            if (!Enum.TryParse(status, out StatusEmprestimo statusEnum))
            {
                return new BadRequestObjectResult("Status inválido.");
            }

            if (!ObjectId.TryParse(id, out var objectId))
            {
                return new BadRequestObjectResult("ID inválido.");
            }

            var filter = Builders<Emprestimo>.Filter.Eq(emp => emp.Id, objectId);

            var update = Builders<Emprestimo>.Update.Set(emp => emp.StatusEmprestimo, statusEnum);

            var result = await collection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                return new NotFoundObjectResult("Empréstimo não encontrado.");
            }

            return new OkObjectResult("Status do empréstimo atualizado com sucesso.");

        }

        [Function("DeleteEmprestimo")]
        public async Task<IActionResult> Delete([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "emprestimos/{id}")] HttpRequest req, string id)
        {
            _logger.LogInformation("DELETE EMPRESTIMO - http trigger");

            if (!ObjectId.TryParse(id, out var objectId))
            {
                return new BadRequestObjectResult("ID inválido.");
            }

            var filter = Builders<Emprestimo>.Filter.Eq(emp => emp.Id, objectId);

            var result = await collection.DeleteOneAsync(filter);

            if (result.DeletedCount == 0)
            {
                return new NotFoundObjectResult("Empréstimo não encontrado");
            }

            return new NoContentResult();


        }
    }
}
