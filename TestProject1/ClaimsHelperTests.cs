using CoreCommon.AuthModel.RefreshToken;
using CoreCommon.HelperCommon;
using System.Security.Claims;
using Xunit;

namespace TestProject1
{
    public class ClaimsHelperTests
    {
        // Shared across all tests in this class
        private readonly ClaimsPrincipal _principal;

        public ClaimsHelperTests()
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "42"),
            new Claim(ClaimTypes.Name, "Alice"),
            new Claim("role", "Admin"),
            new Claim("roleId", "1"),
        };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _principal = new ClaimsPrincipal(identity);
        }
        // Test 1
        // Scenario : principal has NameIdentifier claim = "42"
        // Expected : GetUserId returns "42"


        [Fact]
        public void GetUserId_WithValidClaim_ReturnsUserId()
        {



            // Act
            // TODO: ClaimsHelper.GetUserId(principal)
            var result = ClaimsHelper.GetUserId(_principal);
            // Assert
            // TODO: result == "42"
            Assert.Equal("42", result);
            //throw new NotImplementedException();
        }
        // Test 2
        // Scenario : principal has NO NameIdentifier claim
        // Expected : GetUserId returns null
        [Fact]
        public void GetUserId_WithMissingClaim_ReturnsNull()
        {
            // Arrange
            // TODO: build a ClaimsPrincipal with NO NameIdentifier claim
            // (add a different claim like Name only)
            var claims = new List<Claim>
                {

                    new Claim(ClaimTypes.Name, "Alice"),
                    new Claim("role", "Admin"),
                    new Claim("roleId", "1"),
                };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var _principal1 = new ClaimsPrincipal(identity);

            // Act
            // TODO: ClaimsHelper.GetUserId(principal)
            var result = ClaimsHelper.GetUserId(_principal1);
            // Assert
            // TODO: result is null
            Assert.Null(result);
            //throw new NotImplementedException();
        }
        // Test 3
        // Scenario : principal has "role" claim = "Admin"
        // Expected : GetUserRole returns "Admin"
        [Fact]
        public void GetUserRole_WithRoleClaim_ReturnsRole()
        {


            // Act
            var resut = ClaimsHelper.GetUserRole(_principal);

            // Assert
            // TODO: result == "Admin"
            Assert.Equal("Admin", resut);
            //throw new NotImplementedException();
        }

        // Test 4
        // Scenario : principal has "roleId" claim = "5"
        // Expected : GetUserRoleId returns "5"
        [Fact]
        public void GetUserRoleId_WithRoleIdClaim_ReturnsRoleId()
        {
            var claims = new List<Claim>
                {
                     new Claim(ClaimTypes.NameIdentifier, "42"),
                    new Claim(ClaimTypes.Name, "Alice"),
                    new Claim("role", "Admin"),
                    new Claim("roleId", "5"),
                };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var _principal2 = new ClaimsPrincipal(identity);

            // Act + Assert
            // TODO: GetUserRoleId returns "5"
            var result = ClaimsHelper.GetUserRoleId(_principal2);
            Assert.Equal("5", result);
            //throw new NotImplementedException();
        }

        // Test 5
        // Scenario : principal has ClaimTypes.Name = "Alice"
        // Expected : GetUsername returns "Alice"
        [Fact]
        public void GetUsername_WithNameClaim_ReturnsUsername()
        {
            // Arrange + Act + Assert
            // TODO: build principal, call GetUsername, assert "Alice"
            var result = ClaimsHelper.GetUsername(_principal);

            Assert.Equal("Alice", result);
            //throw new NotImplementedException();
        }

        // Test 6
        // Scenario : GetCurrentUserByClaim with full set of claims
        // Expected : returned CurrentUserInfo has all fields populated
        [Fact]
        public void GetCurrentUserByClaim_WithAllClaims_ReturnsPopulatedObject()
        {
            // Arrange
            // TODO: build ClaimsPrincipal with:
            //   NameIdentifier = "1"
            //   Name = "Alice"
            //   role = "Admin"
            //   roleId = "2"
            var claims = new List<Claim>
                {
                new Claim (ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Name, "Alice"),
                    new Claim("role", "Admin"),
                    new Claim("roleId", "2"),
                };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var _principal1 = new ClaimsPrincipal(identity);
            // Act
            // TODO: new ClaimsHelper().GetCurrentUserByClaim(principal)

            var helper = new ClaimsHelper();
            var result = helper.GetCurrentUserByClaim(_principal1);

            // Assert
            // TODO: result.UserID == "1"
            // TODO: result.UserName == "Alice"
            // TODO: result.Role == "Admin"
            Assert.Equal("1", result.UserID);
            Assert.Equal("Alice", result.UserName);
            Assert.Equal("Admin", result.Role);
            //throw new NotImplementedException();
        }

        // Test 1
        // Scenario : new RefreshTokenRequest() with no values set
        // Expected : RefreshToken and accessToken default to empty string (not null)
        [Fact]
        public void RefreshTokenRequest_DefaultValues_AreEmptyStrings()
        {
            // Arrange + Act
            // TODO: var request = new RefreshTokenRequest()
            var request = new RefreshTokenRequest();
            // Assert
            // TODO: request.RefreshToken == string.Empty
            // TODO: request.accessToken == string.Empty
            Assert.Equal(string.Empty, request.RefreshToken);
            Assert.Equal(string.Empty, request.accessToken);
            //throw new NotImplementedException();
        }

        // Test 2
        // Scenario : assign values to both properties
        // Expected : values are stored correctly
        [Fact]
        public void RefreshTokenRequest_AssignValues_PropertiesAreSet()
        {
            // Arrange
            // TODO: define refreshToken and accessToken strings
            var request = new RefreshTokenRequest();
            request.RefreshToken = "sample_refresh_token";
            request.accessToken = "sample_access_token";


            // Assert
            // TODO: assert both properties match assigned values
            Assert.Equal("sample_refresh_token", request.RefreshToken);
            Assert.Equal("sample_access_token", request.accessToken);
            //throw new NotImplementedException();
        }

    }
}
