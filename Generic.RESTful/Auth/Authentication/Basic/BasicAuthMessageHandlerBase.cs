using Generic.RESTful;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RESTFul
{
    public abstract class BasicAuthMessageHandlerBase : DelegatingHandlerBase
    {
        private const string BasicAuthResponseHeaderValue = "Basic";

        public IPrincipalProvider PrincipalProvider { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            AuthenticationHeaderValue authValue = request.Headers.Authorization;
            if (authValue != null && !String.IsNullOrWhiteSpace(authValue.Parameter))
            {
                Credentials parsedCredentials = ParseAuthorizationHeader(authValue.Parameter);
                if (parsedCredentials != null)
                {
                    Thread.CurrentPrincipal = this.PrincipalProvider.CreatePrincipal(parsedCredentials);

                    this.SetHttpContextUser(Thread.CurrentPrincipal);
                }
            }
            return base.SendAsync(request, cancellationToken)
                .ContinueWith(task =>
                {
                    var response = task.Result;
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                        && !response.Headers.Contains(AuthResponseHeader))
                    {
                        response.Headers.Add(AuthResponseHeader, BasicAuthResponseHeaderValue);
                    }
                    return response;
                }, cancellationToken);
        }

        private static Credentials ParseAuthorizationHeader(string authHeader)
        {
            string[] credentials = Encoding.ASCII.GetString(Convert.FromBase64String(authHeader)).Split(':');
            if (credentials.Length != 2 || string.IsNullOrEmpty(credentials[0])
                || string.IsNullOrEmpty(credentials[1])) return null;
            return new Credentials()
            {
                Username = credentials[0],
                Password = credentials[1],
            };
        }

    }
}
