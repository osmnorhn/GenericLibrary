using System.Net.Http;
using System.Security.Principal;

namespace Generic.RESTful
{
    public abstract class DelegatingHandlerBase : DelegatingHandler
    {
        protected const string AuthResponseHeader = "WWW-Authenticate";

        protected abstract void SetHttpContextUser(IPrincipal principal);
        //if (null != HttpContext.Current)
        //  HttpContext.Current.User = Thread.CurrentPrincipal;
    }
}
