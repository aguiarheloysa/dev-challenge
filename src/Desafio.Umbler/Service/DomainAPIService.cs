using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Desafio.Umbler.DTOs;

namespace Desafio.Umbler.Service
{
    public class DomainApiService
    {
        private readonly HttpClient _http;

        public DomainApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<DomainViewModel> GetDomainAsync(string domain)
        {
            var response = await _http.GetAsync($"api/domain/{domain}");

            if (response.StatusCode == HttpStatusCode.BadRequest)
                throw new ApplicationException("Domínio inválido.");

            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new ApplicationException("Domínio não encontrado.");

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException("Erro ao consultar domínio.");

            return await response.Content
                .ReadFromJsonAsync<DomainViewModel>();
        }
    }
}

