using System;
using System.Security.Principal;

namespace Generic.RESTful
{
    public abstract class BasePrincipalProvider : IPrincipalProvider
    {
        protected abstract bool CheckUser(Credentials credentials);//RDBMS, Redis or .Net Dictionary

        public virtual IPrincipal CreatePrincipal(Credentials credentials)
        {
            if (null == credentials || String.IsNullOrWhiteSpace(credentials.Username) || String.IsNullOrWhiteSpace(credentials.Password))
                return null;

            if (!this.CheckUser(credentials))
                return null;

            var identity = new GenericIdentity(credentials.Username);
            IPrincipal principal = new GenericPrincipal(identity, new[] { "User" });
            return principal;
        }
    }
}
