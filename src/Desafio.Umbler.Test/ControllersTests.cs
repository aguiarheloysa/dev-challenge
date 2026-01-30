using Desafio.Umbler.Controllers;
using Desafio.Umbler.Interface;
using Desafio.Umbler.Models;
using Desafio.Umbler.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using System;

namespace Desafio.Umbler.Test
{
    [TestClass]
    public class ControllersTest
    {
        [TestMethod]
        public void Home_Index_returns_View()
        {
            //arrange 
            var controller = new HomeController();

            //act
            var response = controller.Index();
            var result = response as ViewResult;

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Home_Error_returns_View_With_Model()
        {
            //arrange 
            var controller = new HomeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            //act
            var response = controller.Error();
            var result = response as ViewResult;
            var model = result.Model as ErrorViewModel;

            //assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task Domain_Search_Success()
        {
            //arrange
            var mockService = new Mock<IDomainService>();
            var expectedDomain = new DomainViewModel { Name = "test.com", Ip = "1.2.3.4", HostedAt = "TestHost", WhoIs = "TestWhoIs" };
            
            mockService.Setup(s => s.GetDomainAsync("test.com"))
                       .ReturnsAsync(expectedDomain);

            var controller = new DomainController(mockService.Object);

            //act
            var response = await controller.Get("test.com");
            var result = response as OkObjectResult;
            var obj = result.Value as DomainViewModel;

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.IsNotNull(obj);
            Assert.AreEqual("test.com", obj.Name);
            Assert.AreEqual("1.2.3.4", obj.Ip);
        }

        [TestMethod]
         public async Task Domain_Search_Invalid_Domain()
        {
            //arrange
            var mockService = new Mock<IDomainService>();
            var controller = new DomainController(mockService.Object);

            //act
            var response = await controller.Get("invalid-domain");
            var result = response as BadRequestObjectResult;

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("Domínio inválido. Utilize o formato exemplo.com", result.Value);
        }

        [TestMethod]
        public async Task Domain_Search_Service_Throws_Exception()
        {
            //arrange
            var mockService = new Mock<IDomainService>();
            mockService.Setup(s => s.GetDomainAsync("error.com"))
                       .ThrowsAsync(new InvalidOperationException("Service Error"));

            var controller = new DomainController(mockService.Object);

            //act
            var response = await controller.Get("error.com");
            var result = response as NotFoundObjectResult;

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
             Assert.AreEqual("Service Error", result.Value);
        }
    }
}