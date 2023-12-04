using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Tlmo.Server.Validators;

public class HasLowercaseAttribute : ValidationAttribute
{
  public HasLowercaseAttribute(string? errorMessage = null)
  {
    ErrorMessage = errorMessage ??
                   "The field {0} must be a string and contain at least one lowercase letter";
  }

  public override bool IsValid(object? value)
  {
    return value is string password && new Regex($@"[a-z]").Match(password).Success;
  }
  
  public override string FormatErrorMessage(string name)
  {
    return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, new object[] { name });
  }
}