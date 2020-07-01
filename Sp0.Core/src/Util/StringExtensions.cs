using System;
using System.Text.RegularExpressions;

namespace Sp0.Core
{
  public static class StringExtensions
  {
    private static Regex quoteEscape = new Regex(@"'");

    public static string ShellEscape(this string str)
    {
      return quoteEscape.Replace(str, "''");
    }
  }
}
