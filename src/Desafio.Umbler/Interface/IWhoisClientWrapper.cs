using System.Threading.Tasks;
using Whois.NET;

namespace Desafio.Umbler.Interface
{
    public interface IWhoisClientWrapper
    {
        Task<WhoisResponse> QueryAsync(string domain);
    }
}
