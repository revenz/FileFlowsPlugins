#if DEBUG

using System.Net;
using System.Text;
using System.Text.Json;

namespace FileFlows.Emby.Tests;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly List<(Func<HttpRequestMessage, bool> Match, HttpResponseMessage Response)> _responses = new();

    public RequestStub When(string urlPattern)
    {
        return new RequestStub(req => req.RequestUri!.ToString().StartsWith(urlPattern), this);
    }

    public RequestStub When(Func<HttpRequestMessage, bool> predicate)
    {
        return new RequestStub(predicate, this);
    }

    public class RequestStub
    {
        private readonly Func<HttpRequestMessage, bool> _match;
        private readonly MockHttpMessageHandler _handler;

        public RequestStub(Func<HttpRequestMessage, bool> match, MockHttpMessageHandler handler)
        {
            _match = match;
            _handler = handler;
        }

        public void Respond(HttpStatusCode statusCode, object content)
        {
            string json = JsonSerializer.Serialize(content);
            Respond(statusCode, new StringContent(json, Encoding.UTF8, "application/json"));
        }

        public void Respond(HttpStatusCode statusCode, string content)
        {
            Respond(statusCode, new StringContent(content, Encoding.UTF8, "application/json"));
        }

        public void Respond(HttpStatusCode statusCode, HttpContent content)
        {
            _handler._responses.Add((_match, new HttpResponseMessage(statusCode)
            {
                Content = content
            }));
        }
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        foreach (var (match, response) in _responses)
        {
            if (match(request))
                return Task.FromResult(response);
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent($"No mock response for {request.Method} {request.RequestUri}")
        });
    }
}

#endif
