using System.Text;
using LongArithmetic.Core.Models;
using static LongArithmetic.Core.Helpers.Helpers;

namespace LongArithmetic.Core;

public static class BigMath
{
    public static BigNumber Add(BigNumber a, BigNumber b)
    {
        var aStr = Trim(a.Value);
        var bStr = Trim(b.Value);

        if (aStr.Length > bStr.Length) (aStr, bStr) = (bStr, aStr);

        aStr = aStr.PadLeft(bStr.Length, '0');

        var result = new StringBuilder();
        var carry = 0;
        for (var i = aStr.Length - 1; i >= 0; i--)
        {
            var num1 = aStr[i] - '0';
            var num2 = bStr[i] - '0';
            var sum = num1 + num2 + carry;
            var res = sum % 10;
            carry = sum / 10;
            result.Insert(0, (char)('0' + res));
        }

        if (carry != 0)
            result.Insert(0, carry);

        return new BigNumber(Trim(result.ToString()));
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

        bStr = bStr.PadLeft(aStr.Length, '0');

        var sb = new StringBuilder();
        var carry = 0;
        for (var i = aStr.Length - 1; i >= 0; i--)
        {
            var nextCarry = false;
            var num1 = aStr[i] - '0';
            var num2 = bStr[i] - '0';
            if (num1 - carry < num2)
            {
                num1 += 10;
                nextCarry = true;
            }

            var res = (num1 - num2 - carry);
            sb.Insert(0, (char)('0' + res));
            carry = nextCarry ? 1 : 0;
        }

        var sanitized = Trim(sb.ToString());
        if (resultNegative) return new BigNumber("-" + sanitized);
        return new BigNumber(sanitized);
    }

    public static BigNumber Multiply(BigNumber a, BigNumber b, string method = "auto")
    {
        if (method == "classic")
            return new BigNumber(MultiplyClassic(a.Value, b.Value));
        if (method == "karatsuba")
            return new BigNumber(MultiplyKaratsuba(a.Value, b.Value));

        if (a.Value.Length < 32 || b.Value.Length < 32)
            return new BigNumber(MultiplyClassic(a.Value, b.Value));
        return new BigNumber(MultiplyKaratsuba(a.Value, b.Value));
    }

    private static string MultiplyClassic(BigNumber a, BigNumber b)
    {
        var result = new int[a.Value.Length + b.Value.Length];
        for (var i = a.Value.Length - 1; i >= 0; i--)
        {
            for (var j = b.Value.Length - 1; j >= 0; j--)
            {
                var mul = (a.Value[i] - '0') * (b.Value[j] - '0');
                var sum = mul + result[i + j + 1];
                result[i + j + 1] = sum % 10;
                result[i + j] += sum / 10;
            }
        }

        var sb = new StringBuilder();
        foreach (var d in result) sb.Append(d);
        return Trim(sb.ToString());
    }

    private static string MultiplyKaratsuba(BigNumber first, BigNumber second)
    {
        first = Trim(first);
        second = Trim(second);

        if (first.Value.Length == 1) return MultiplyByDigit(second, int.Parse(first));
        if (second.Value.Length == 1) return MultiplyByDigit(first, int.Parse(second));

        var cutPos = GetCutPosition(first, second);
        var a = GetFirstPart(first, cutPos);
        var b = GetSecondPart(first, cutPos);
        var c = GetFirstPart(second, cutPos);
        var d = GetSecondPart(second, cutPos);
        var ac = MultiplyKaratsuba(a, c);
        var bd = MultiplyKaratsuba(b, d);
        var abCd = MultiplyKaratsuba(Add(a, b), Add(c, d));
        var term0 = Subtract(Subtract(abCd, ac), bd);
        var padding = b.Length + d.Length;
        var term1 = term0 + new string('0', padding / 2);
        var term2 = ac + new string('0', padding);
        return Add(Add(term1, term2), bd);
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
}
