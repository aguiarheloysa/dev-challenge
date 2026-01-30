using System;
using System.Threading.Tasks;
using Desafio.Umbler.DTOs;
using Desafio.Umbler.Models;
using Desafio.Umbler.Repositories;
using Desafio.Umbler.Interface;
using Whois.NET;
using DnsClient;
using System.Linq;

namespace Desafio.Umbler.Service
{
    public class DomainService : IDomainService
    {
        private readonly DomainRepository _repository;
        private readonly ILookupClient _lookupClient;
        private readonly IWhoisClientWrapper _whoisClient;

        public DomainService(DomainRepository repository, ILookupClient lookupClient, IWhoisClientWrapper whoisClient)
        {
            _repository = repository;
            _lookupClient = lookupClient;
            _whoisClient = whoisClient;
        }

        public async Task<DomainViewModel> GetDomainAsync(string domainName)
        {
            if (string.IsNullOrWhiteSpace(domainName))
                throw new ArgumentException("Domínio inválido");

            var domain = await _repository.GetByNameAsync(domainName);

            bool search = domain == null ||
                DateTime.UtcNow.Subtract(domain.UpdatedAt).TotalMinutes > domain.Ttl;

            if (search)
            {
                var newDomain = await searchDomain(domainName);

                if (newDomain == null)
                    throw new InvalidOperationException("Não foi possível obter o domínio");

                if (domain == null)
                    await _repository.CreateAsync(newDomain);
                else
                {
                    domain.Ip = newDomain.Ip;
                    domain.WhoIs = newDomain.WhoIs;
                    domain.UpdatedAt = DateTime.UtcNow;
                    domain.Ttl = newDomain.Ttl;
                    domain.HostedAt = newDomain.HostedAt;

                    await _repository.UpdateAsync(domain);
                }

                domain = newDomain;
            }

            return new DomainViewModel
            {
                Name = domain.Name,
                Ip = domain.Ip,
                HostedAt = domain.HostedAt,
                WhoIs = domain.WhoIs
            };
        }

        private async Task<Domain> searchDomain(string domainName)
        {
            try
            {
                var whois = await _whoisClient.QueryAsync(domainName);
                
                var result = await _lookupClient.QueryAsync(domainName, QueryType.A);
                var record = result.Answers.ARecords().FirstOrDefault();

                if (record == null) return null;

                var host = await _whoisClient.QueryAsync(record.Address.ToString());

                return new Domain
                {
                    Name = domainName,
                    Ip = record.Address.ToString(),
                    UpdatedAt = DateTime.UtcNow,
                    Ttl = record.TimeToLive,
                    WhoIs = whois.Raw,
                    HostedAt = host.OrganizationName
                };
            }
            catch
            {
                return null;
            }
        }
    }
}