using System.Security.Principal;

namespace Generic.RESTful
{
    public interface IPrincipalProvider
    {
        IPrincipal CreatePrincipal(Credentials credentials);
    }
}
