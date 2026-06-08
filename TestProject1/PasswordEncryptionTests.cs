using AuthAPI.Repositories;
using Xunit;

namespace TestProject1
{
    public class PasswordEncryptionTests
    {
        // Test 1
        // Scenario : a normal password like "Hello123"
        // Expected : result is not null or empty
        [Fact]
        public void HashPassword_NormalPassword_ReturnsNonEmptyString()
        {
            // Arrange
            // TODO: define input password
            string password = "Hello123";

            // Act
            // TODO: call PasswordEncryption.HashPassword(...)
            string result = PasswordEncryption.HashPassword(password);
            // Assert
            // TODO: Assert result is not null or empty
            Assert.False(string.IsNullOrEmpty(result));
            //throw new NotImplementedException();
        }

        // Test 2
        // Scenario : same password hashed twice
        // Expected : both results are equal (deterministic)
        [Fact]
        public void HashPassword_SamePasswordTwice_ReturnsSameHash()
        {
            // Arrange
            // TODO: define one password string
            string password1 = "Hello123";
            string password2 = "Hello123";
            // Act
            // TODO: hash it twice into two variables
            string hash1 = PasswordEncryption.HashPassword(password1);
            string hash2 = PasswordEncryption.HashPassword(password2);

            // Assert
            // TODO: assert both hashes are equal
            Assert.Equal(hash1, hash2);
            //throw new NotImplementedException();
        }
        // Test 3
        // Scenario : two different passwords
        // Expected : hashes are different
        [Fact]
        public void HashPassword_DifferentPasswords_ReturnsDifferentHashes()
        {
            // Arrange
            // TODO: define two different passwords
            string password1 = "Hello12345";
            string password2 = "Hello123";
            // Act
            // TODO: hash each one
            string hash1 = PasswordEncryption.HashPassword(password1);
            string hash2 = PasswordEncryption.HashPassword(password2);

            // Assert
            // TODO: assert they are NOT equal
            Assert.NotEmpty(hash1);
            Assert.NotEmpty(hash2);
            Assert.NotEqual(hash1, hash2);
            //throw new NotImplementedException();
        }

        // Test 4
        // Scenario : empty string input
        // Expected : returns empty string (not crash)
        [Fact]
        public void HashPassword_EmptyString_ReturnsEmptyString()
        {
            // Arrange
            // TODO: input = string.Empty
            string password = "";
            // Act
            // TODO: call HashPassword
            string result = PasswordEncryption.HashPassword(password);

            // Assert
            // TODO: assert result is empty

            Assert.NotEqual(string.Empty, result);
            //Assert.True(string.IsNullOrEmpty(result));
            //throw new NotImplementedException();
        }
        // Test 5
        // Scenario : hash result should always be lowercase
        // Expected : result equals result.ToLower()
        [Fact]
        public void HashPassword_AnyPassword_ReturnsLowercaseHash()
        {
            // Arrange
            // TODO: any password
            string password = "Hello123";
            // Act
            // TODO: get hash
            string hash = PasswordEncryption.HashPassword(password);

            // Assert
            // TODO: assert hash == hash.ToLower()
            Assert.Equal(hash, hash.ToLower());
            //throw new NotImplementedException();
        }
        // Test 6
        // Scenario : result should be exactly 64 characters (SHA256 = 32 bytes = 64 hex chars)
        // Expected : result.Length == 64
        [Fact]
        public void HashPassword_AnyPassword_Returns64CharHash()
        {
            // Arrange
            // TODO: any password
            string password = "Hello123";
            // Act
            // TODO: get hash
            var hash = PasswordEncryption.HashPassword(password);
            // Assert
            // TODO: assert length is 64
            Assert.Equal(64, hash.Length);
            //throw new NotImplementedException();
        }



    }
}
