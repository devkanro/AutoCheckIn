// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: NotEmptyValidationRule.cs
// Version: 20160411

using System.Globalization;
using System.Windows.Controls;

namespace AutoCheckIn
{
    public class NotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return string.IsNullOrWhiteSpace((value ?? "").ToString())
                ? new ValidationResult(false, "不能为空。")
                : ValidationResult.ValidResult;
        }
    }
}