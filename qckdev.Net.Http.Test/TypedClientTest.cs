#if NETCOREAPP3_1_OR_GREATER
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using qckdev.Net;

namespace qckdev.Net.Http.Test
{
    [TestClass]
    public class TypedClientTest
    {
        [TestMethod]
        public async Task GetUserAsync_ShouldReturnDeserializedObject_WhenResponseIs200()
        {
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.Method == HttpMethod.Get &&
                        r.RequestUri.AbsolutePath == "/users/1"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage request, CancellationToken _) => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StringContent("{\"id\":1,\"name\":\"Ash\"}", Encoding.UTF8, "application/json")
                });

            using var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://api.test") };
            var sut = new TestTypedClient(httpClient);

            var result = await sut.GetUserAsync(1);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Name.Should().Be("Ash");
            handler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task GetUserAsync_ShouldReturnNull_WhenResponseIs404()
        {
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage request, CancellationToken _) => new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    RequestMessage = request,
                    Content = new StringContent(string.Empty, Encoding.UTF8, "text/plain")
                });

            using var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://api.test") };
            var sut = new TestTypedClient(httpClient);

            var result = await sut.GetUserAsync(404);

            result.Should().BeNull();
            handler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task GetUserAsync_ShouldThrowFetchFailedException_WhenResponseIs401()
        {
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage request, CancellationToken _) => new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    RequestMessage = request,
                    Content = new StringContent(string.Empty, Encoding.UTF8, "text/plain")
                });

            using var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://api.test") };
            var sut = new TestTypedClient(httpClient);

            Func<Task> act = async () => await sut.GetUserAsync(401);

            var ex = await act.Should().ThrowAsync<qckdev.Net.FetchFailedException>();
            ex.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            handler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task GetUserAsync_ShouldPropagateException_WhenNetworkFails()
        {
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network unavailable"));

            using var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://api.test") };
            var sut = new TestTypedClient(httpClient);

            Func<Task> act = async () => await sut.GetUserAsync(500);

            var ex = await act.Should().ThrowAsync<qckdev.Net.FetchFailedException>();
            ex.Which.InnerException.Should().BeOfType<HttpRequestException>();
            ex.Which.InnerException.Message.Should().Contain("Network unavailable");
            handler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        private sealed class TestTypedClient
        {
            private readonly HttpClient _httpClient;

            public TestTypedClient(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }

            public async Task<UserDto> GetUserAsync(int id, CancellationToken cancellationToken = default)
            {
                var path = $"/users/{id}";

                try
                {
                    return await _httpClient.FetchAsync<UserDto, object>(HttpMethod.Get, path);
                }
                catch (qckdev.Net.FetchFailedException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
            }
        }

        private sealed class UserDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
#endif
