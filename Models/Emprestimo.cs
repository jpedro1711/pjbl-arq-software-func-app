using BibliotecaFunctionApp.Models.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BibliotecaFunctionApp.Models
{
    public class Emprestimo
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("ReservaId")]
        public int ReservaId { get; set; }
        [BsonElement("DataDevolucao")]
        public DateTime DataDevolucao { get; set; }
        [BsonElement("StatusEmprestimo")]
        public StatusEmprestimo StatusEmprestimo { get; set; } = StatusEmprestimo.Ativo;
    }
}
