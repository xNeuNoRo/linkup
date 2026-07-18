using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;

/// <summary>
/// Valida que el número telefónico pertenezca al formato de República Dominicana:
/// 809-555-1234, 829-555-1234 o 849-555-1234.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DominicanPhoneAttribute : ValidationAttribute
{
    public DominicanPhoneAttribute()
        : base("Debe ingresar un número telefónico válido de República Dominicana (ej. 809-555-1234).")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return true;
            return System.Text.RegularExpressions.Regex.IsMatch(
                s,
                @"^(809|829|849)-\d{3}-\d{4}$"
            );
        }
        return false;
    }
}
