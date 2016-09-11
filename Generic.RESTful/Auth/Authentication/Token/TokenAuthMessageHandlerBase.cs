using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Generic.RESTful
{
    public abstract class TokenAuthMessageHandlerBase : DelegatingHandlerBase
    {
        private const string TokenAuthResponseHeaderValue = "Token";

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            AuthenticationHeaderValue authValue = request.Headers.Authorization;
            if (authValue != null && !String.IsNullOrWhiteSpace(authValue.Parameter))
            {
                string guidStr = Encoding.ASCII.GetString(Convert.FromBase64String(authValue.Parameter));

                Guid token;
                if (Guid.TryParse(guidStr, out token))
                {
                    User user;
                    if (this.Table.TryAuthenticateToken(token, out user) && this.AuthenticateUser(request, user))
                    {
                        var identity = new GenericIdentity(token.ToString(), "Token");
                        IPrincipal principal = new GenericPrincipal(identity, new[] { "User" });
                        Thread.CurrentPrincipal = principal;

                        this.SetHttpContextUser(Thread.CurrentPrincipal);
                    }
                }
            }
            return base.SendAsync(request, cancellationToken)
                .ContinueWith(task =>
                {
                    var response = task.Result;
                    if (response.StatusCode == HttpStatusCode.Unauthorized
                        && !response.Headers.Contains(AuthResponseHeader))
                    {
                        response.Headers.Add(AuthResponseHeader, TokenAuthResponseHeaderValue);
                    }
                    return response;
                }, cancellationToken);
        }

        protected abstract ITokenTable CreateTokenTable();

        protected abstract bool AuthenticateUser(HttpRequestMessage request, User user);

        private static readonly object syncRoot = new object();
        private ITokenTable table;
        protected ITokenTable Table
        {
            get
            {
                if (null == this.table)
                {
                    lock (syncRoot)
                    {
                        if (null == this.table)
                        {
                            this.table = this.CreateTokenTable();
                            if (null == this.table)
                                throw new NotImplementedException("TokenAuthMessageHandlerBase.CreateTokenTable");
                        }
                    }
                }

                return this.table;
            }
        }
       
    }
}