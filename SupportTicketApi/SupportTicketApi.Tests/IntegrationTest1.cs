using Microsoft.Extensions.Logging;

namespace SupportTicketApi.Tests
{
    public class IntegrationTest1
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        [Test]
        public async Task GetWebResourceRootReturnsOkStatusCode()
        {
            // Arrange
            var cancellationToken = TestContext.CurrentContext.CancellationToken;
            var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.SupportTicketApi_AppHost>();
            appHost.Services.AddLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
                // Override the logging filters from the app's configuration
                logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
                logging.AddFilter("Aspire.", LogLevel.Debug);
            });
            appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
            {
                clientBuilder.AddStandardResilienceHandler();
            });

            await using var app = await appHost.BuildAsync(cancellationToken);//.WaitAsync(DefaultTimeout, cancellationToken);
            await app.StartAsync(cancellationToken);//.WaitAsync(DefaultTimeout, cancellationToken);

            // Act
            using var httpClient = app.CreateHttpClient("api");
            await app.ResourceNotifications.WaitForResourceHealthyAsync("api", cancellationToken);//.WaitAsync(DefaultTimeout, cancellationToken);
            using var response = await httpClient.GetAsync("/api/SupportTickets", cancellationToken);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}
