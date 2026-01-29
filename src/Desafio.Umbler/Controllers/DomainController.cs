using Desafio.Umbler.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Desafio.Umbler.Controllers
{
    [ApiController]
    [Route("api/domain")]
    public class DomainController : ControllerBase
    {
        private readonly IDomainService _service;

        public DomainController(IDomainService service)
        {
            _service = service;
        }

        [HttpGet("{domainName}")]
        public async Task<IActionResult> Get(string domainName)
        {
            
            if (!Regex.IsMatch(domainName, @"^[a-zA-Z0-9-]+\.[a-zA-Z]{2,}$"))
            {
                return BadRequest("Domínio inválido. Utilize o formato exemplo.com");
            }

            try
            {
                var response = await _service.GetDomainAsync(domainName);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Erro interno inesperado");
            }
        }
    }
}
