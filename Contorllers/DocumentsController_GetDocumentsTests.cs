using Xunit;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using DocuwareManagementWebAPI.Controllers;
using DocuwareManagementWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocuWare.Platform.ServerClient;

namespace DocuwareManagementWebAPI.Tests.Controllers
{
    public class DocumentsController_GetDocumentsTests
    {
        private readonly IDocuWareService _fakeDocuWareService;
        private readonly ILogger<DocumentsController> _fakeLogger;
        private readonly DocumentsController _controller;

        public DocumentsController_GetDocumentsTests()
        {
            // Create fakes using FakeItEasy
            _fakeDocuWareService = A.Fake<IDocuWareService>();  // Fake the DocuWareService
            _fakeLogger = A.Fake<ILogger<DocumentsController>>();  // Fake the logger

            // Create the controller instance
            _controller = new DocumentsController(_fakeDocuWareService, _fakeLogger);
        }

        [Fact]
        public async Task GetDocuments_ShouldReturnOk_WhenDocumentsAreFound()
        {
            // Arrange: Set up the fake service to return a list of documents
            var documents = new List<Document> { new Document() };
            A.CallTo(() => _fakeDocuWareService.ListAllDocumentsAsync(A<int>.Ignored))
                .Returns(documents);

            // Act: Call the method
            var result = await _controller.GetDocuments();

            // Assert: Verify the result
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedDocuments = okResult.Value.Should().BeAssignableTo<List<Document>>().Subject;
            returnedDocuments.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetDocuments_ShouldReturnNotFound_WhenNoDocumentsAreFound()
        {
            // Arrange: Set up the fake service to return an empty list of documents
            A.CallTo(() => _fakeDocuWareService.ListAllDocumentsAsync(A<int>.Ignored))
                .Returns(new List<Document>());

            // Act: Call the method
            var result = await _controller.GetDocuments();

            // Assert: Verify the result is NotFound
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetDocuments_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange: Set up the fake service to throw an exception
            A.CallTo(() => _fakeDocuWareService.ListAllDocumentsAsync(A<int>.Ignored))
                .Throws(new System.Exception("Some error"));

            // Act: Call the method
            var result = await _controller.GetDocuments();

            // Assert: Verify the result is 500 Internal Server Error
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);
        }
    }
}
