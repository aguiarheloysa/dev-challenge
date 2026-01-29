using Desafio.Umbler.DTOs;
using System.Threading.Tasks;

namespace Desafio.Umbler.Interface 
{
        public interface IDomainService
        {
        Task<DomainViewModel> GetDomainAsync(string domainName);
    }
}
