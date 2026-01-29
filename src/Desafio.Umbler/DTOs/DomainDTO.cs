using System;
using System.ComponentModel.DataAnnotations;
namespace Desafio.Umbler.DTOs
{
    public class DomainViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Ip { get; set; }
        [Required]
        public string HostedAt { get; set; }
        [Required]
        public string WhoIs { get; set; }
    }
}
