using Desafio.Umbler.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Desafio.Umbler.Repositories
{
    public class DomainRepository
    {
        private readonly DatabaseContext _context;

        public DomainRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Domain> GetByNameAsync(string name)
        {
            return await _context.Domains.FirstOrDefaultAsync(d => d.Name == name);
        }

        public async Task CreateAsync(Domain domain)
        {
            if (domain == null) return;

            _context.Domains.Add(domain);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Domain domain)
        {
            if (domain == null) return;

            _context.Domains.Update(domain);
            await _context.SaveChangesAsync();
        }
    }
}
