using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.Generic;
using System;
using McMaster.Extensions.CommandLineUtils;

namespace Sp0.Core
{
  public class PagingOptions
  {
    public PagingOptions(CommandLineApplication app)
    {
      All = app.OptionalOption<bool?>(
        "-a|--all",
        "If set, fetch all pages instead of a single one. Does not return paging meta data.",
        "unset",
        CommandOptionType.NoValue
      );
      Limit = app.OptionalOption<int?>(
        "-l|--limit",
        "The maximum number of items to return",
        "Maximum amount possible for the request",
        CommandOptionType.SingleValue
      );
      Offset = app.OptionalOption<int?>(
        "--offset",
        "The index of the first item to return",
        "first item (0)",
        CommandOptionType.SingleValue
      );
    }

    public CommandOption All { get; }
    public CommandOption<int?> Limit { get; }
    public CommandOption<int?> Offset { get; }
  }
}
