using CoreCommon.HelperCommon.Enums;
using Xunit;

namespace TestProject1
{
    public class MemberData
    {
        public static List<object[]> OkTestCases =
            new List<object[]>
            {
                new object[] { "Hello, World!", ResultStatusCode.Ok },
                new object[] { 12345, ResultStatusCode.Ok },
                new object[] { new { Name = "Alice", Age = 30 }, ResultStatusCode.Ok }
            };

        [Theory]
        [MemberData(nameof(OkTestCases))]
        public void Ok_ShouldHandleComplexObjects(object input, ResultStatusCode statusCode)
        {
            // Act
            var result = ResultData<object>.Ok(input, statusCode);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(input, result.Data);
            Assert.Equal((int)statusCode, result.StatusCode);
            Assert.Equal(string.Empty, result.Error);
        }
    }
}
