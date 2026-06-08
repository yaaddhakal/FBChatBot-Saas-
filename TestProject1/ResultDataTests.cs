using CoreCommon.HelperCommon.Enums;
using Xunit;

namespace TestProject1
{
    public class ResultDataTests
    {
        [Fact]
        public void Ok_ShouldReturnSuccessResult()
        {
            // Arrange
            var expectedData = "Hello";

            // Act
            var result = ResultData<string>.Ok(expectedData);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(expectedData, result.Data);
            Assert.Equal((int)ResultStatusCode.Ok, result.StatusCode);
            Assert.Equal(string.Empty, result.Error);
        }
        [Fact]
        public void Fail_ShouldReturnUnSuccessResult()
        {
            // Arrange
            var errorMessage = "Some error has come";

            // Act
            var result = ResultData<string>.Fail(errorMessage);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(errorMessage, result.Error);
            Assert.Equal((int)ResultStatusCode.InternalServerError, result.StatusCode);
            Assert.Null(result.Data);
        }

        public void Ok_ShouldReturnIntResult()
        {
            // Arrange
            var expectedData = 123;

            // Act
            var result = ResultData<int>.Ok(expectedData);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(expectedData, result.Data);
            Assert.Equal((int)ResultStatusCode.Ok, result.StatusCode);
            Assert.Equal(string.Empty, result.Error);
        }
        [Theory]
        [InlineData("Bad Request", ResultStatusCode.BadRequest)]
        [InlineData("Not Found", ResultStatusCode.NotFound)]
        [InlineData("Internal Server Error", ResultStatusCode.InternalServerError)]
        public void Fail_ShouldReturnCorrectStatusAndError(string errorMessage, ResultStatusCode statusCode)
        {
            //Act
            var result = ResultData<string>.Fail(errorMessage, statusCode);

            //accert
            Assert.False(result.Success);
            Assert.Equal(errorMessage, result.Error);
            Assert.Equal((int)statusCode, result.StatusCode);
            Assert.Null(result.Data);

        }
        [Theory]
        [InlineData("Test", ResultStatusCode.Ok)]
        [InlineData(123, ResultStatusCode.Ok)]
        public void Ok_ShouldReturnCorrectResult(object input, ResultStatusCode statusCode)
        {
            // Arrange

            // Act
            var result = ResultData<object>.Ok(input, statusCode);
            // Assert
            Assert.True(result.Success);
            Assert.Equal(input, result.Data);
            Assert.Equal((int)ResultStatusCode.Ok, result.StatusCode);
            Assert.Equal(string.Empty, result.Error);
            // Assert.Null(result.Data);
        }
    }
}
