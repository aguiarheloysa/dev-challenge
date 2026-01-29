using System.Collections.Generic;

namespace Desafio.Umbler.Models
{
    public class DomainQueryResult
    {
        public string Name { get; set; } = string.Empty;
        public bool Available { get; set; }
        public string? WhoisContent { get; set; }
    }
}
