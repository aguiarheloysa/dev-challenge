using Desafio.Umbler.Interface;
using System.Threading.Tasks;
using Whois.NET;

namespace Desafio.Umbler.Service
{
    public class WhoisClientWrapper : IWhoisClientWrapper
    {
        public async Task<WhoisResponse> QueryAsync(string domain)
        {
            return await WhoisClient.QueryAsync(domain);
        }
    }
}
