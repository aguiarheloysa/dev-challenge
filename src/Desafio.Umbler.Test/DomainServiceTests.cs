using Desafio.Umbler.Interface;
using Desafio.Umbler.Models;
using Desafio.Umbler.Repositories;
using Desafio.Umbler.Service;
using DnsClient;
using DnsClient.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Whois.NET;

namespace Desafio.Umbler.Test
{
    [TestClass]
    public class DomainServiceTests
    {
        private Mock<ILookupClient> _mockLookupClient;
        private Mock<IWhoisClientWrapper> _mockWhoisClient;
        private DbContextOptions<DatabaseContext> _dbOptions;

        [TestInitialize]
        public void Setup()
        {
            _mockLookupClient = new Mock<ILookupClient>();
            _mockWhoisClient = new Mock<IWhoisClientWrapper>();
            _dbOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB for each test
                .Options;
        }

        [TestMethod]
        public async Task GetDomainAsync_Returns_From_Cache_If_Valid()
        {
            // Arrange
            var existingDomain = new Domain
            {
                Name = "cached.com",
                Ip = "1.1.1.1",
                UpdatedAt = DateTime.UtcNow,
                Ttl = 600, // Valid TTL
                HostedAt = "CachedHost",
                WhoIs = "CachedData"
            };

            using (var context = new DatabaseContext(_dbOptions))
            {
                context.Domains.Add(existingDomain);
                await context.SaveChangesAsync();
            }

            using (var context = new DatabaseContext(_dbOptions))
            {
                var repo = new DomainRepository(context);
                var service = new DomainService(repo, _mockLookupClient.Object, _mockWhoisClient.Object);

                // Act
                var result = await service.GetDomainAsync("cached.com");

                // Assert
                Assert.AreEqual(existingDomain.Ip, result.Ip);
                Assert.AreEqual(existingDomain.HostedAt, result.HostedAt);
                // Verify external services were NOT called
                _mockLookupClient.Verify(x => x.QueryAsync(It.IsAny<string>(), It.IsAny<QueryType>(), It.IsAny<QueryClass>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [TestMethod]
        public async Task GetDomainAsync_Fetches_External_If_Not_Found()
        {
            // Arrange
            var domainName = "new.com";
            var ipAddress = "2.2.2.2";

            // Mock DNS response
            var mockDnsResponse = new Mock<IDnsQueryResponse>();
            var resourceRecord = new ARecord(new ResourceRecordInfo(domainName, ResourceRecordType.A, QueryClass.IN, 300, 300), IPAddress.Parse(ipAddress));
            mockDnsResponse.Setup(x => x.Answers).Returns(new List<DnsResourceRecord> { resourceRecord });

            _mockLookupClient.Setup(x => x.QueryAsync(domainName, QueryType.A, It.IsAny<QueryClass>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockDnsResponse.Object);

            // Mock Whois responses
            var whoisResponseDomain = new WhoisResponse { Raw = "Whois Data" };
            var whoisResponseIp = new WhoisResponse { OrganizationName = "New Host" };

            _mockWhoisClient.Setup(x => x.QueryAsync(domainName)).ReturnsAsync(whoisResponseDomain);
            _mockWhoisClient.Setup(x => x.QueryAsync(ipAddress)).ReturnsAsync(whoisResponseIp);

            using (var context = new DatabaseContext(_dbOptions))
            {
                var repo = new DomainRepository(context);
                var service = new DomainService(repo, _mockLookupClient.Object, _mockWhoisClient.Object);

                // Act
                var result = await service.GetDomainAsync(domainName);

                // Assert
                Assert.AreEqual(ipAddress, result.Ip);
                Assert.AreEqual("New Host", result.HostedAt);
                
                // Verify DB was updated
                var savedDomain = await context.Domains.FirstOrDefaultAsync(d => d.Name == domainName);
                Assert.IsNotNull(savedDomain);
                Assert.AreEqual(ipAddress, savedDomain.Ip);
            }
        }

        [TestMethod]
        public async Task GetDomainAsync_Updates_Expired_Cache()
        {
            // Arrange
            var domainName = "expired.com";
            var oldIp = "1.0.0.0";
            var newIp = "2.0.0.0";

            var expiredDomain = new Domain
            {
                Name = domainName,
                Ip = oldIp,
                UpdatedAt = DateTime.UtcNow.AddMinutes(-60), // Old time
                Ttl = 30, // Expired
                HostedAt = "OldHost",
                WhoIs = "OldWhoIs"
            };

            using (var context = new DatabaseContext(_dbOptions))
            {
                context.Domains.Add(expiredDomain);
                await context.SaveChangesAsync();
            }

            // Mock new DNS response
            var mockDnsResponse = new Mock<IDnsQueryResponse>();
            var resourceRecord = new ARecord(new ResourceRecordInfo(domainName, ResourceRecordType.A, QueryClass.IN, 300, 300), IPAddress.Parse(newIp));
            mockDnsResponse.Setup(x => x.Answers).Returns(new List<DnsResourceRecord> { resourceRecord });

            _mockLookupClient.Setup(x => x.QueryAsync(domainName, QueryType.A, It.IsAny<QueryClass>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockDnsResponse.Object);

             // Mock Whois responses
            var whoisResponseDomain = new WhoisResponse { Raw = "New Whois Data" };
            var whoisResponseIp = new WhoisResponse { OrganizationName = "New Host" };

            _mockWhoisClient.Setup(x => x.QueryAsync(domainName)).ReturnsAsync(whoisResponseDomain);
            _mockWhoisClient.Setup(x => x.QueryAsync(newIp)).ReturnsAsync(whoisResponseIp);

            using (var context = new DatabaseContext(_dbOptions))
            {
                var repo = new DomainRepository(context);
                var service = new DomainService(repo, _mockLookupClient.Object, _mockWhoisClient.Object);

                // Act
                var result = await service.GetDomainAsync(domainName);

                // Assert
                Assert.AreEqual(newIp, result.Ip);
                
                // Verify DB was updated
                var savedDomain = await context.Domains.AsNoTracking().FirstOrDefaultAsync(d => d.Name == domainName);
                Assert.AreEqual(newIp, savedDomain.Ip);
            }
        }
    }
}
