using System;

namespace LongArithmetic.Core.Models;

public readonly struct BigNumber
{
    public string Value { get; }

    public BigNumber(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        Value = Helpers.Helpers.Trim(value);
    }

    public override string ToString() => Value;

    public static implicit operator string(BigNumber n) => n.Value;
    public static implicit operator BigNumber(string s) => new(s);
}
