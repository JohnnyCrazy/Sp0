using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sp0.Core
{
  public interface ILoginService
  {
    Uri GenerateLoginURI(Uri address, string clientId, IList<string> scopes, string state);
    Task<string?> WaitForLogin(Uri address, int port, TimeSpan timeout, string state);
  }
}
