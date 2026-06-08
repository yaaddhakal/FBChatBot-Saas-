using Xunit;

namespace TestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void Addition_ShouldReturnCorrectResult()
        {
            Assert.Equal(2, 1 + 1);
        }
        [Fact]
        public void Multiply_ShouldReturnCorrectResult()
        {
            var a = 3;
            var b = 4;
            Assert.Equal(12, (a * b));
        }
    }
}
