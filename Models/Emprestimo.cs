using BibliotecaFunctionApp.Models.Enums;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaFunctionApp.Models
{
    public class Emprestimo
    {
        [BsonId]
        public int Id { get; set; }
        [BsonElement("ReservaId")]
        public int ReservaId { get; set; }
        [BsonElement("DataDevolucao")]
        public DateTime DataDevolucao { get; set; }
        [BsonElement("StatusEmprestimo")]
        public StatusEmprestimo StatusEmprestimo { get; set; } = StatusEmprestimo.Ativo;
    }
}
