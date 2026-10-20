using BlackDigital.Domain.Security;
using System.Reflection;

namespace BlackDigital.Domain.Test.Security
{
    public class AuthorizeAttributeTest
    {
        [Authorize]
        private class ProtectedService
        {
            [Authorize(Roles = new[] { "Admin", "Manager" }, Policy = "Category.Write")]
            public void ProtectedAction()
            {
            }
        }

        private class UnprotectedService
        {
            public void Action()
            {
            }
        }

        [Authorize]
        private class BaseService
        {
        }

        private class DerivedService : BaseService
        {
        }

        [Fact]
        public void Constructor_WithoutArguments_ShouldHaveNullRolesAndPolicy()
        {
            // Act
            var attribute = new AuthorizeAttribute();

            // Assert
            Assert.Null(attribute.Roles);
            Assert.Null(attribute.Policy);
        }

        [Fact]
        public void Roles_WithMultipleValues_ShouldStoreAllRoles()
        {
            // Act
            var attribute = new AuthorizeAttribute { Roles = new[] { "Admin", "Manager" } };

            // Assert
            Assert.Equal(new[] { "Admin", "Manager" }, attribute.Roles);
        }

        [Fact]
        public void Policy_WithValidValue_ShouldStorePolicy()
        {
            // Act
            var attribute = new AuthorizeAttribute { Policy = "Category.Write" };

            // Assert
            Assert.Equal("Category.Write", attribute.Policy);
        }

        [Fact]
        public void RolesAndPolicy_WithBothValues_ShouldStoreBoth()
        {
            // Act
            var attribute = new AuthorizeAttribute
            {
                Roles = new[] { "Admin" },
                Policy = "Category.Write"
            };

            // Assert
            Assert.Equal(new[] { "Admin" }, attribute.Roles);
            Assert.Equal("Category.Write", attribute.Policy);
        }

        [Fact]
        public void Usage_ShouldTargetClassInterfaceAndMethod()
        {
            // Arrange
            var usage = typeof(AuthorizeAttribute).GetCustomAttribute<AttributeUsageAttribute>();

            // Assert
            Assert.NotNull(usage);
            Assert.True(usage!.ValidOn.HasFlag(AttributeTargets.Class));
            Assert.True(usage.ValidOn.HasFlag(AttributeTargets.Interface));
            Assert.True(usage.ValidOn.HasFlag(AttributeTargets.Method));
            Assert.True(usage.Inherited);
        }

        [Fact]
        public void AppliedToClass_ShouldBeDetectedByReflection()
        {
            // Act
            var attribute = typeof(ProtectedService).GetCustomAttribute<AuthorizeAttribute>();

            // Assert
            Assert.NotNull(attribute);
            Assert.Null(attribute!.Roles);
            Assert.Null(attribute.Policy);
        }

        [Fact]
        public void AppliedToMethod_ShouldBeDetectedByReflection()
        {
            // Arrange
            var method = typeof(ProtectedService).GetMethod(nameof(ProtectedService.ProtectedAction));

            // Act
            var attribute = method!.GetCustomAttribute<AuthorizeAttribute>();

            // Assert
            Assert.NotNull(attribute);
            Assert.Equal(new[] { "Admin", "Manager" }, attribute!.Roles);
            Assert.Equal("Category.Write", attribute.Policy);
        }

        [Fact]
        public void AppliedToBaseClass_ShouldBeDetectedOnDerivedClass()
        {
            // Act
            var attribute = typeof(DerivedService).GetCustomAttribute<AuthorizeAttribute>(inherit: true);

            // Assert
            Assert.NotNull(attribute);
        }

        [Fact]
        public void NotApplied_ShouldNotBeDetectedByReflection()
        {
            // Arrange
            var method = typeof(UnprotectedService).GetMethod(nameof(UnprotectedService.Action));

            // Act
            var classAttribute = typeof(UnprotectedService).GetCustomAttribute<AuthorizeAttribute>();
            var methodAttribute = method!.GetCustomAttribute<AuthorizeAttribute>();

            // Assert
            Assert.Null(classAttribute);
            Assert.Null(methodAttribute);
        }
    }
}
