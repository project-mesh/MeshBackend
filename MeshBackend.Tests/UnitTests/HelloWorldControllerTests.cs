using System;
using Xunit;
using Moq;
using MeshBackend.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MeshBackend.Tests.UnitTests
{
    public class HelloWorldControllerTests
    {
        [Fact]
        public void GetReturnConstStringInJson()
        {
            // Arrange
            var logger = Mock.Of<ILogger<HelloWorldController>>();
            var controller = new HelloWorldController(logger);

            // Act
            JsonResult jsonResult = controller.Get();
            dynamic result = jsonResult.Value;
            // Assert
            String expected = "Project Mesh Web API";

            Assert.Equal(expected, result.text);
        }

        [Fact]
        public void PostReceiveAndReturnSameStringInJson()
        {
            // Arrange
            var logger = Mock.Of<ILogger<HelloWorldController>>();
            var controller = new HelloWorldController(logger);
            String input = "Random Message";

            // Act
            JsonResult jsonResult = controller.Post(input);
            dynamic result = jsonResult.Value;

            // Assert
            Assert.Equal(input, result.text);
        }
    }
}
