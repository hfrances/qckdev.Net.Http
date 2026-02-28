#if NETCOREAPP3_1_OR_GREATER
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace qckdev.Net.Http.Test
{
    [TestClass]
    public class CorrelationIdObservabilityTest
    {
        private const string CorrelationHeader = "X-Correlation-ID";

        [TestMethod]
        public async Task Handler_ShouldInjectCorrelationId_WhenSendingRequest()
        {
            HttpRequestMessage capturedRequest = null;
            var innerHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            innerHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequest = request)
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            using var client = new HttpClient(new CorrelationIdHandler { InnerHandler = innerHandler.Object });
            Activity.Current = null;

            await client.GetAsync("https://api.example.local/ping");

            capturedRequest.Should().NotBeNull();
            capturedRequest.Headers.Contains(CorrelationHeader).Should().BeTrue();
            capturedRequest.Headers.TryGetValues(CorrelationHeader, out var values).Should().BeTrue();
            values.Should().ContainSingle();
            values.Single().Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public async Task Handler_ShouldUseCurrentActivityId_WhenPresent()
        {
            HttpRequestMessage capturedRequest = null;
            var innerHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            innerHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequest = request)
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            using var client = new HttpClient(new CorrelationIdHandler { InnerHandler = innerHandler.Object });
            using var activity = new Activity("handler-persistence").Start();
            var expectedId = activity.Id;

            await client.GetAsync("https://api.example.local/ping");

            capturedRequest.Should().NotBeNull();
            capturedRequest.Headers.TryGetValues(CorrelationHeader, out var values).Should().BeTrue();
            values.Single().Should().Be(expectedId);
        }

        [TestMethod]
        public async Task Handler_ShouldThrowFetchFailedException_WhenNetworkFails()
        {
            var innerHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            innerHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("socket failure"));

            using var client = new HttpClient(new CorrelationIdHandler { InnerHandler = innerHandler.Object });

            Func<Task> act = async () => await client.GetAsync("https://api.example.local/ping");

            var ex = await act.Should().ThrowAsync<qckdev.Net.FetchFailedException>();
            ex.Which.InnerException.Should().BeOfType<HttpRequestException>();
            ex.Which.InnerException.Message.Should().Contain("socket failure");
        }

        [TestMethod]
        public async Task Middleware_ShouldExtractIncomingCorrelationId_AndSetResponseHeader()
        {
            var logger = new Mock<ILogger<CorrelationIdMiddleware>>(MockBehavior.Strict);
            logger.Setup(l => l.BeginScope(It.IsAny<Dictionary<string, object>>()))
                .Returns(Mock.Of<IDisposable>());

            var context = BuildContext();
            context.Request.Headers[CorrelationHeader] = "upstream-correlation";

            var sut = new CorrelationIdMiddleware(_ => Task.CompletedTask, logger.Object);

            await sut.InvokeAsync(context);

            context.Response.Headers[CorrelationHeader].ToString().Should().Be("upstream-correlation");
        }

        [TestMethod]
        public async Task Middleware_ShouldGenerateGuid_WhenHeaderIsMissing()
        {
            var logger = new Mock<ILogger<CorrelationIdMiddleware>>(MockBehavior.Strict);
            logger.Setup(l => l.BeginScope(It.IsAny<Dictionary<string, object>>()))
                .Returns(Mock.Of<IDisposable>());

            var context = BuildContext();
            var sut = new CorrelationIdMiddleware(_ => Task.CompletedTask, logger.Object);

            await sut.InvokeAsync(context);

            context.Response.Headers.ContainsKey(CorrelationHeader).Should().BeTrue();
            Guid.TryParse(context.Response.Headers[CorrelationHeader], out _).Should().BeTrue();
        }

        [TestMethod]
        public async Task Middleware_ShouldStartLogScope_WithCorrelationId()
        {
            var logger = new Mock<ILogger<CorrelationIdMiddleware>>(MockBehavior.Strict);
            logger.Setup(l => l.BeginScope(It.IsAny<Dictionary<string, object>>()))
                .Returns(Mock.Of<IDisposable>());

            var context = BuildContext();
            context.Request.Headers[CorrelationHeader] = "scope-test-id";
            var sut = new CorrelationIdMiddleware(_ => Task.CompletedTask, logger.Object);

            await sut.InvokeAsync(context);

            logger.Verify(
                l => l.BeginScope(It.Is<Dictionary<string, object>>(state =>
                    state.ContainsKey(CorrelationHeader) &&
                    (string)state[CorrelationHeader] == "scope-test-id")),
                Times.Once);
        }

        [TestMethod]
        public async Task Middleware_ShouldThrowFetchFailedException_WhenDownstreamFails()
        {
            var logger = new Mock<ILogger<CorrelationIdMiddleware>>(MockBehavior.Strict);
            logger.Setup(l => l.BeginScope(It.IsAny<Dictionary<string, object>>()))
                .Returns(Mock.Of<IDisposable>());

            var context = BuildContext();
            RequestDelegate next = _ => throw new HttpRequestException("upstream timeout");
            var sut = new CorrelationIdMiddleware(next, logger.Object);

            Func<Task> act = async () => await sut.InvokeAsync(context);

            var ex = await act.Should().ThrowAsync<qckdev.Net.FetchFailedException>();
            ex.Which.InnerException.Should().BeOfType<HttpRequestException>();
            ex.Which.InnerException.Message.Should().Contain("upstream timeout");
        }

        private static DefaultHttpContext BuildContext()
        {
            var context = new DefaultHttpContext();
            context.Request.Scheme = "https";
            context.Request.Host = new HostString("api.example.local");
            context.Request.Path = "/orders/42";
            return context;
        }

        public sealed class CorrelationIdHandler : DelegatingHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString("D");

                if (!request.Headers.Contains(CorrelationHeader))
                {
                    request.Headers.Add(CorrelationHeader, correlationId);
                }

                try
                {
                    return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                }
                catch (HttpRequestException ex)
                {
                    throw new qckdev.Net.FetchFailedException(
                        request.Method.Method,
                        request.RequestUri,
                        request.Headers.ToDictionary(h => h.Key, h => h.Value),
                        request.Content?.Headers?.ContentType?.ToString(),
                        null,
                        null,
                        "Correlation outbound request failed.",
                        null,
                        null,
                        ex);
                }
            }
        }

        public sealed class CorrelationIdMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly ILogger<CorrelationIdMiddleware> _logger;

            public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
            {
                _next = next;
                _logger = logger;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                var correlationId = context.Request.Headers.TryGetValue(CorrelationHeader, out var incoming)
                    && !string.IsNullOrWhiteSpace(incoming.ToString())
                    ? incoming.ToString()
                    : Guid.NewGuid().ToString("D");

                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    [CorrelationHeader] = correlationId
                }))
                {
                    context.Response.Headers[CorrelationHeader] = correlationId;
                    try
                    {
                        await _next(context).ConfigureAwait(false);
                    }
                    catch (qckdev.Net.FetchFailedException)
                    {
                        throw;
                    }
                    catch (HttpRequestException ex)
                    {
                        var uri = new Uri($"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}");

                        throw new qckdev.Net.FetchFailedException(
                            "INCOMING",
                            uri,
                            new Dictionary<string, IEnumerable<string>>
                            {
                                [CorrelationHeader] = new[] { correlationId }
                            },
                            null,
                            null,
                            null,
                            "Correlation middleware downstream request failed.",
                            null,
                            null,
                            ex);
                    }
                }
            }
        }
    }
}
#endif
