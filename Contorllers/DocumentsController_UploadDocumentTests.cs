using Xunit;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using DocuwareManagementWebAPI.Controllers;
using DocuwareManagementWebAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System;
using DocuWare.Platform.ServerClient;
using DocuwareManagementWebAPI.Models;

namespace DocuwareManagementWebAPI.Tests.Controllers
{
    public class DocumentsController_UploadDocumentTests
    {
        private readonly IDocuWareService _fakeDocuWareService;
        private readonly ILogger<DocumentsController> _fakeLogger;
        private readonly DocumentsController _controller;

        public DocumentsController_UploadDocumentTests()
        {
            // Create fakes using FakeItEasy
            _fakeDocuWareService = A.Fake<IDocuWareService>();
            _fakeLogger = A.Fake<ILogger<DocumentsController>>();

            // Create the controller instance with the fake services
            _controller = new DocumentsController(_fakeDocuWareService, _fakeLogger);
        }

        [Fact]
        public async Task UploadDocument_ShouldReturnOk_WhenDocumentIsUploadedSuccessfully()
        {
            // Arrange
            var fakeFile = A.Fake<IFormFile>();
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write("Test content");
            writer.Flush();
            stream.Position = 0;

            A.CallTo(() => fakeFile.OpenReadStream()).Returns(stream);
            A.CallTo(() => fakeFile.FileName).Returns("test.pdf");
            A.CallTo(() => fakeFile.Length).Returns(1024);

            // Create a fake document (assuming the method returns a Document object)
            var fakeDocument = new Document
            {
                Id = 123  // Use the actual properties of the Document class
            };

            // Set up the service call to return the fake document
            A.CallTo(() => _fakeDocuWareService.UploadDocumentAsync(
                    A<string>.Ignored,
                    A<string>.Ignored,
                    A<DateTime>.Ignored,
                    A<IFormFile>.Ignored))
                .Returns(Task.FromResult(fakeDocument));

            // Act
            var result = await _controller.UploadDocument("TestCompany", "TestContact", DateTime.Now, fakeFile);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var responseValue = okResult.Value as UploadDocumentResponse;

            // Assert response
            responseValue.Should().NotBeNull();
            responseValue.Message.Should().Be("Document uploaded successfully");
            responseValue.DocumentId.Should().Be(123);
        }


        [Fact]
        public async Task UploadDocument_ShouldReturnBadRequest_WhenFileIsMissing()
        {
            // Act
            var result = await _controller.UploadDocument("TestCompany", "TestContact", DateTime.Now, null);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UploadDocument_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var fakeFile = A.Fake<IFormFile>();
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write("Test content");
            writer.Flush();
            stream.Position = 0;

            A.CallTo(() => fakeFile.OpenReadStream()).Returns(stream);
            A.CallTo(() => fakeFile.Length).Returns(1024); // Make sure the file is not empty

            A.CallTo(() => _fakeDocuWareService.UploadDocumentAsync(A<string>.Ignored, A<string>.Ignored, A<DateTime>.Ignored, A<IFormFile>.Ignored))
                .Throws(new Exception("Error during upload"));

            // Act
            var result = await _controller.UploadDocument("TestCompany", "TestContact", DateTime.Now, fakeFile);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);
        }

    }
}
