using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Sp0.Core
{
  public interface ICommand
  {
    void Register(CommandLineApplication app);
  }
}
