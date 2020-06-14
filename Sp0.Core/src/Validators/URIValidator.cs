using System;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace Sp0.Core
{
  class AbsoluteURIValidator : IOptionValidator
  {
    public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
    {
      // This validator only runs if there is a value
      if (!option.HasValue()) return ValidationResult.Success;
      var val = option.Value();

      if (!Uri.TryCreate(val, UriKind.Absolute, out Uri? result) || !result.IsAbsoluteUri)
      {
        return new ValidationResult($"--{option.LongName} must be a valid, absolute URI");
      }

      return ValidationResult.Success;
    }
  }
}
