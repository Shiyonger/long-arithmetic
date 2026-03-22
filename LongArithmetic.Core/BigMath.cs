using System;
using System.Text;
using LongArithmetic.Core.Models;
using static LongArithmetic.Core.Helpers.Helpers;

namespace LongArithmetic.Core;

public static class BigMath
{
    private const int KaratsubaThreshold = 1024;

    public static BigNumber Add(BigNumber a, BigNumber b)
    {
        var aStr = Trim(a.Value);
        var bStr = Trim(b.Value);
        var maxLength = Math.Max(aStr.Length, bStr.Length);
        var result = new char[maxLength + 1];
        var carry = 0;

        for (var i = 0; i < maxLength; i++)
        {
            var aIndex = aStr.Length - 1 - i;
            var bIndex = bStr.Length - 1 - i;
            var num1 = aIndex >= 0 ? aStr[aIndex] - '0' : 0;
            var num2 = bIndex >= 0 ? bStr[bIndex] - '0' : 0;
            var sum = num1 + num2 + carry;

            result[maxLength - i] = (char)('0' + (sum % 10));
            carry = sum / 10;
        }

        result[0] = (char)('0' + carry);
        return new BigNumber(Trim(new string(result)));
    }

    public static BigNumber Subtract(BigNumber a, BigNumber b)
    {
        var aStr = Trim(a.Value);
        var bStr = Trim(b.Value);
        var resultNegative = false;

        var comparison = Compare(new BigNumber(aStr), new BigNumber(bStr));
        if (comparison < 0)
        {
            (aStr, bStr) = (bStr, aStr);
            resultNegative = true;
        }
        else if (comparison == 0)
        {
            return new BigNumber("0");
        }

        var result = new char[aStr.Length];
        var borrow = 0;

        for (var i = aStr.Length - 1; i >= 0; i--)
        {
            var num1 = aStr[i] - '0' - borrow;
            var bIndex = bStr.Length - aStr.Length + i;
            var num2 = bIndex >= 0 ? bStr[bIndex] - '0' : 0;

            if (num1 < num2)
            {
                num1 += 10;
                borrow = 1;
            }
            else
            {
                borrow = 0;
            }

            result[i] = (char)('0' + (num1 - num2));
        }

        var sanitized = Trim(new string(result));
        return resultNegative ? new BigNumber("-" + sanitized) : new BigNumber(sanitized);
    }

    public static BigNumber Multiply(BigNumber a, BigNumber b, string method = "auto")
    {
        if (method == "classic")
            return new BigNumber(MultiplyClassic(a.Value, b.Value));
        if (method == "karatsuba")
            return new BigNumber(MultiplyKaratsuba(a.Value, b.Value));

        if (a.Value.Length < KaratsubaThreshold || b.Value.Length < KaratsubaThreshold)
            return new BigNumber(MultiplyClassic(a.Value, b.Value));
        return new BigNumber(MultiplyKaratsuba(a.Value, b.Value));
    }

    private static string MultiplyClassic(string a, string b)
    {
        a = Trim(a);
        b = Trim(b);

        if (a == "0" || b == "0") return "0";

        var result = new int[a.Length + b.Length];
        for (var i = a.Length - 1; i >= 0; i--)
        {
            for (var j = b.Length - 1; j >= 0; j--)
            {
                var mul = (a[i] - '0') * (b[j] - '0');
                var sum = mul + result[i + j + 1];
                result[i + j + 1] = sum % 10;
                result[i + j] += sum / 10;
            }
        }

        var sb = new StringBuilder(result.Length);
        foreach (var d in result)
        {
            sb.Append((char)('0' + d));
        }

        return Trim(sb.ToString());
    }

    private static string MultiplyKaratsuba(string first, string second)
    {
        first = Trim(first);
        second = Trim(second);

        if (first == "0" || second == "0") return "0";
        if (first.Length <= KaratsubaThreshold || second.Length <= KaratsubaThreshold)
            return MultiplyClassic(first, second);

        var maxLength = Math.Max(first.Length, second.Length);
        if ((maxLength & 1) == 1) maxLength++;

        first = first.PadLeft(maxLength, '0');
        second = second.PadLeft(maxLength, '0');

        var half = maxLength / 2;
        var a = Trim(first[..half]);
        var b = Trim(first[half..]);
        var c = Trim(second[..half]);
        var d = Trim(second[half..]);

        var ac = MultiplyKaratsuba(a, c);
        var bd = MultiplyKaratsuba(b, d);
        var abCd = MultiplyKaratsuba(Add(a, b).Value, Add(c, d).Value);
        var adPlusBc = Subtract(Subtract(abCd, ac), bd).Value;

        return Add(Add(ShiftLeft(ac, half * 2), ShiftLeft(adPlusBc, half)), bd).Value;
    }

    public static BigNumber Divide(BigNumber dividend, BigNumber divisor, string method = "classic")
    {
        if (divisor.Value == "0") throw new DivideByZeroException();
        if (Compare(dividend.Value, divisor.Value) < 0) return new BigNumber("0");

        if (method == "binary")
            return new BigNumber(DivideBinarySearch(dividend.Value, divisor.Value));

        var result = new StringBuilder();
        var current = "";
        foreach (var digit in dividend.Value)
        {
            current += digit;
            current = Trim(current);
            var quotientDigit = 0;
            for (var d = 9; d >= 0; d--)
            {
                var product = MultiplyByDigit(divisor.Value, d);
                if (Compare(product, current) <= 0)
                {
                    quotientDigit = d;
                    current = Subtract(current, product);
                    break;
                }
            }

            result.Append(quotientDigit);
        }

        return new BigNumber(Trim(result.ToString()));
    }

    private static string DivideBinarySearch(string dividend, string divisor)
    {
        dividend = Trim(dividend);
        divisor = Trim(divisor);
        if (divisor == "0") throw new DivideByZeroException();
        if (Compare(dividend, divisor) < 0) return "0";

        var left = new BigNumber("0");
        var right = new BigNumber(dividend);
        var result = new BigNumber("0");

        while (Compare(left, right) <= 0)
        {
            var sum = Add(left, right);
            var mid = new BigNumber(DivideByTwo(sum.Value));

            if (Compare(mid, left) == 0 && Compare(left, right) < 0)
            {
                mid = Add(left, "1");
            }

            var product = Multiply(mid, divisor).Value;
            var cmp = Compare(product, dividend);

            if (cmp == 0) return mid.Value;
            if (cmp < 0)
            {
                result = mid;
                left = Add(mid, "1");
            }
            else
            {
                right = Subtract(mid, "1");
            }
        }

        return result.Value;
    }

    public static BigNumber Pow(BigNumber baseValue, int exponent, string method = "binary")
    {
        if (exponent < 0) throw new ArgumentException("Negative exponent not supported");
        var b = Trim(baseValue.Value);
        if (exponent == 0) return new BigNumber("1");
        if (b == "0") return new BigNumber("0");

        if (method == "iterative")
        {
            var result = b;
            for (var i = 1; i < exponent; i++) result = Multiply(result, b).Value;
            return new BigNumber(result);
        }

        var resStr = "1";
        var currentBase = b;
        var e = exponent;
        while (e > 0)
        {
            if ((e & 1) == 1) resStr = Multiply(resStr, currentBase).Value;
            currentBase = Multiply(currentBase, currentBase).Value;
            e >>= 1;
        }

        return new BigNumber(resStr);
    }

    private static string ShiftLeft(string value, int zeroCount)
    {
        value = Trim(value);
        if (value == "0") return "0";
        return value + new string('0', zeroCount);
    }
}
