using BlackDigital.Domain.Infrastructures;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace BlackDigital.Domain.Test.Infrastructures
{
    public class HtmlTemplateEmailServiceTest
    {
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ILogger<HtmlTemplateEmailService>> _mockLogger;
        private readonly Mock<IHostEnvironment> _mockEnvironment;
        private readonly string _contentRoot;
        private readonly string _templateDir;

        public HtmlTemplateEmailServiceTest()
        {
            _mockEmailService = new Mock<IEmailService>();
            _mockLogger = new Mock<ILogger<HtmlTemplateEmailService>>();
            _mockEnvironment = new Mock<IHostEnvironment>();

            _contentRoot = Path.Combine(Path.GetTempPath(), "bd_domain_email_templates_" + Guid.NewGuid().ToString("N"));
            _templateDir = Path.Combine(_contentRoot, "templates", "email");
            Directory.CreateDirectory(_templateDir);

            _mockEnvironment.SetupGet(e => e.ContentRootPath).Returns(_contentRoot);
        }

        // Dispose temp dir after all tests in this class run
        ~HtmlTemplateEmailServiceTest()
        {
            try
            {
                if (Directory.Exists(_contentRoot))
                    Directory.Delete(_contentRoot, true);
            }
            catch { /* ignore cleanup failures */ }
        }

        private HtmlTemplateEmailService CreateService()
            => new(_mockEmailService.Object, _mockLogger.Object, _mockEnvironment.Object);

        private string WriteTemplate(string fileName, string content)
        {
            var path = Path.Combine(_templateDir, fileName);
            File.WriteAllText(path, content);
            return path;
        }

        // ---------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------

        [Fact]
        public void Constructor_ShouldSetTemplateBasePathFromContentRoot()
        {
            var service = CreateService();

            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithNullEnvironment_ShouldNotThrow()
        {
            // When environment is null, the service falls back to Directory.GetCurrentDirectory()
            var service = new HtmlTemplateEmailService(_mockEmailService.Object, _mockLogger.Object, null!);

            Assert.NotNull(service);
        }

        // ---------------------------------------------------------------
        // SendTemplateEmailAsync
        // ---------------------------------------------------------------

        [Fact]
        public async Task SendTemplateEmailAsync_WithNullEmail_ShouldThrow()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.SendTemplateEmailAsync((TemplateEmail)null!));
        }

        [Fact]
        public async Task SendTemplateEmailAsync_WithValidTemplate_ShouldProcessAndSend()
        {
            WriteTemplate("welcome.html", "<h1>Hello {{ name }}, your code is {{ code }}</h1>");
            var service = CreateService();

            var email = new TemplateEmail()
                .UseTemplate("welcome")
                .AddParameter("name", "John")
                .AddParameter("code", "ABC123");

            await service.SendTemplateEmailAsync(email);

            _mockEmailService.Verify(s => s.SendEmailAsync(It.Is<Email>(e =>
                e.Body == "<h1>Hello John, your code is ABC123</h1>")), Times.Once);
        }

        [Fact]
        public async Task SendTemplateEmailAsync_WithHtmlExtensionInTemplateName_ShouldLoadCorrectly()
        {
            WriteTemplate("welcome.html", "<p>Hi {{ name }}</p>");
            var service = CreateService();

            var email = new TemplateEmail().UseTemplate("welcome.html").AddParameter("name", "John");

            await service.SendTemplateEmailAsync(email);

            _mockEmailService.Verify(s => s.SendEmailAsync(It.Is<Email>(e => e.Body == "<p>Hi John</p>")), Times.Once);
        }

        [Fact]
        public async Task SendTemplateEmailAsync_WithUnmatchedParameters_ShouldRemovePlaceholders()
        {
            WriteTemplate("test.html", "Hello {{ name }}, welcome to {{ site }}");
            var service = CreateService();

            var email = new TemplateEmail().UseTemplate("test").AddParameter("name", "John");
            // "site" is not provided

            await service.SendTemplateEmailAsync(email);

            _mockEmailService.Verify(s => s.SendEmailAsync(It.Is<Email>(e => e.Body == "Hello John, welcome to ")), Times.Once);
        }

        [Fact]
        public async Task SendTemplateEmailAsync_WithNoParameters_ShouldRemoveAllPlaceholders()
        {
            WriteTemplate("test.html", "Hello {{ name }}, code: {{ code }}");
            var service = CreateService();

            var email = new TemplateEmail().UseTemplate("test");

            await service.SendTemplateEmailAsync(email);

            _mockEmailService.Verify(s => s.SendEmailAsync(It.Is<Email>(e => e.Body == "Hello , code: ")), Times.Once);
        }

        [Fact]
        public async Task SendTemplateEmailAsync_ShouldPreserveWhitespaceInPlaceholders()
        {
            WriteTemplate("test.html", "A={{ a }} B={{b}} C={{  c  }}");
            var service = CreateService();

            var email = new TemplateEmail().UseTemplate("test")
                .AddParameter("a", "1")
                .AddParameter("b", "2")
                .AddParameter("c", "3");

            await service.SendTemplateEmailAsync(email);

            _mockEmailService.Verify(s => s.SendEmailAsync(It.Is<Email>(e => e.Body == "A=1 B=2 C=3")), Times.Once);
        }

        [Fact]
        public async Task SendTemplateEmailAsync_WithBuilderFunc_ShouldProcessAndSend()
        {
            WriteTemplate("test.html", "Value: {{ x }}");
            var service = CreateService();

            await service.SendTemplateEmailAsync(e => e
                .UseTemplate("test")
                .AddParameter("x", "42"));

            _mockEmailService.Verify(s => s.SendEmailAsync(It.Is<Email>(e => e.Body == "Value: 42")), Times.Once);
        }

        // ---------------------------------------------------------------
        // LoadTemplateAsync - error cases
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task SendTemplateEmailAsync_WithNullOrWhitespaceTemplateName_ShouldThrow(string? templateName)
        {
            var service = CreateService();
            var email = new TemplateEmail().UseTemplate(templateName!);

            await Assert.ThrowsAsync<ArgumentException>(() => service.SendTemplateEmailAsync(email));
        }

        [Theory]
        [InlineData("../secret")]
        [InlineData("folder/template")]
        [InlineData("folder\\template")]
        [InlineData("..\\secret")]
        public async Task SendTemplateEmailAsync_WithPathTraversalInTemplateName_ShouldThrow(string templateName)
        {
            var service = CreateService();
            var email = new TemplateEmail().UseTemplate(templateName);

            await Assert.ThrowsAsync<ArgumentException>(() => service.SendTemplateEmailAsync(email));
        }

        [Fact]
        public async Task SendTemplateEmailAsync_WithNonExistentTemplate_ShouldThrowFileNotFoundException()
        {
            var service = CreateService();
            var email = new TemplateEmail().UseTemplate("does-not-exist");

            await Assert.ThrowsAsync<FileNotFoundException>(() => service.SendTemplateEmailAsync(email));
        }

        // ---------------------------------------------------------------
        // Logging
        // ---------------------------------------------------------------

        [Fact]
        public async Task SendTemplateEmailAsync_OnSuccess_ShouldLogInformation()
        {
            WriteTemplate("test.html", "<p>Hi</p>");
            var service = CreateService();

            var email = new TemplateEmail().UseTemplate("test");

            await service.SendTemplateEmailAsync(email);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Template email sent successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendTemplateEmailAsync_OnError_ShouldLogErrorAndRethrow()
        {
            // Non-existent template triggers an exception
            var service = CreateService();
            var email = new TemplateEmail().UseTemplate("missing");

            await Assert.ThrowsAsync<FileNotFoundException>(() => service.SendTemplateEmailAsync(email));

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
