using System.Text.RegularExpressions;

namespace LinkUpPro.Domain.ValueObjects;

/// <summary>
/// Represents the password strength criteria defined by the functional document.
/// </summary>
public sealed record PasswordStrength(
    int Score,
    PasswordStrengthLevel Level,
    PasswordStrengthCriteria CriteriaMet)
{
    public bool IsStrong => Level == PasswordStrengthLevel.Strong;

    public static PasswordStrength Calculate(string? plainText)
    {
        var password = plainText ?? string.Empty;
        var score = 0;
        var criteria = PasswordStrengthCriteria.None;

        if (password.Length >= 8)
        {
            score++;
            criteria |= PasswordStrengthCriteria.MinimumLength;
        }

        if (Regex.IsMatch(password, "[A-Z]"))
        {
            score++;
            criteria |= PasswordStrengthCriteria.UppercaseLetter;
        }

        if (Regex.IsMatch(password, "[a-z]"))
        {
            score++;
            criteria |= PasswordStrengthCriteria.LowercaseLetter;
        }

        if (Regex.IsMatch(password, "[0-9]"))
        {
            score++;
            criteria |= PasswordStrengthCriteria.Digit;
        }

        if (Regex.IsMatch(password, "[^a-zA-Z0-9]"))
        {
            score++;
            criteria |= PasswordStrengthCriteria.SpecialCharacter;
        }

        var level = score switch
        {
            <= 2 => PasswordStrengthLevel.Weak,
            <= 4 => PasswordStrengthLevel.Medium,
            _ => PasswordStrengthLevel.Strong,
        };

        return new PasswordStrength(score, level, criteria);
    }
}

public enum PasswordStrengthLevel
{
    Weak = 1,
    Medium = 2,
    Strong = 3,
}

[Flags]
public enum PasswordStrengthCriteria
{
    None = 0,
    MinimumLength = 1,
    UppercaseLetter = 2,
    LowercaseLetter = 4,
    Digit = 8,
    SpecialCharacter = 16,
}
