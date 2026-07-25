using Liguria_Trasporti.Models;

namespace Liguria_Trasporti.Authentication;

public interface IAuthenticationService
{
    Task<string> RegisterAsync(string email, string password);
}