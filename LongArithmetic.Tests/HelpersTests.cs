using FluentAssertions;
using LongArithmetic.Core.Helpers;
using LongArithmetic.Core.Models;
using NUnit.Framework;

namespace LongArithmetic.Tests;

[TestFixture]
public class HelpersTests
{
    #region Trim Tests

    [Test]
    [Category("Trim")]
    public void Trim_WithLeadingZeros_RemovesAllZeros()
    {
        var input = "0000123";

        var result = Helpers.Trim(input);

        result.Should().Be("123");
    }

    [Test]
    [Category("Trim")]
    public void Trim_WithOnlyZeros_ReturnsZero()
    {
        var input = "00000";

        var result = Helpers.Trim(input);

        result.Should().Be("0");
    }

    [Test]
    [Category("Trim")]
    public void Trim_WithNoLeadingZeros_ReturnsSameString()
    {
        var input = "12345";

        var result = Helpers.Trim(input);

        result.Should().Be("12345");
    }

    [Test]
    [Category("Trim")]
    [TestCase("0", "0")]
    [TestCase("1", "1")]
    [TestCase("999", "999")]
    [TestCase("0000999", "999")]
    public void Trim_VariousCases(string input, string expected)
    {
        var result = Helpers.Trim(input);

        result.Should().Be(expected);
    }

    #endregion

    #region DivideByTwo Tests

    [Test]
    [Category("DivideByTwo")]
    public void DivideByTwo_WithEvenNumber_ReturnCorrectResult()
    {
        var input = "100";

        var result = Helpers.DivideByTwo(input);

        result.Should().Be("50");
    }

    [Test]
    [Category("DivideByTwo")]
    public void DivideByTwo_WithOddNumber_TruncatesResult()
    {
        var input = "99";

        var result = Helpers.DivideByTwo(input);

        result.Should().Be("49");
    }

    [Test]
    [Category("DivideByTwo")]
    public void DivideByTwo_WithOne_ReturnsZero()
    {
        var input = "1";

        var result = Helpers.DivideByTwo(input);

        result.Should().Be("0");
    }

    [Test]
    [Category("DivideByTwo")]
    [TestCase("2", "1")]
    [TestCase("10", "5")]
    [TestCase("999", "499")]
    [TestCase("1000", "500")]
    public void DivideByTwo_VariousCases(string input, string expected)
    {
        var result = Helpers.DivideByTwo(input);

        result.Should().Be(expected);
    }

    #endregion

    #region MultiplyByDigit Tests

    [Test]
    [Category("MultiplyByDigit")]
    public void MultiplyByDigit_ByZero_ReturnsZero()
    {
        var input = "12345";
        var digit = 0;

        var result = Helpers.MultiplyByDigit(input, digit);

        result.Should().Be("0");
    }

    [Test]
    [Category("MultiplyByDigit")]
    public void MultiplyByDigit_ByOne_ReturnsTrimmedInput()
    {
        var input = "01230";
        var digit = 1;

        var result = Helpers.MultiplyByDigit(input, digit);

        result.Should().Be("1230");
    }

    [Test]
    [Category("MultiplyByDigit")]
    public void MultiplyByDigit_WithCarry_AppliesCarryCorrectly()
    {
        var input = "999";
        var digit = 9;

        var result = Helpers.MultiplyByDigit(input, digit);

        result.Should().Be("8991");
    }

    [Test]
    [Category("MultiplyByDigit")]
    [TestCase("5", 2, "10")]
    [TestCase("9", 9, "81")]
    [TestCase("123", 2, "246")]
    [TestCase("50", 3, "150")]
    public void MultiplyByDigit_VariousCases(string input, int digit, string expected)
    {
        var result = Helpers.MultiplyByDigit(input, digit);

        result.Should().Be(expected);
    }

    [Test]
    [Category("MultiplyByDigit")]
    public void MultiplyByDigit_WithLargeNumber_HandlesCarriesCorrectly()
    {
        var result = Helpers.MultiplyByDigit("9999999999", 9);

        result.Should().Be("89999999991");
    }

    #endregion

    #region Compare Tests

    [Test]
    [Category("Compare")]
    public void Compare_SameLengthFirstInt_ReturnsNegativeOne()
    {
        var a = new BigNumber("100");
        var b = new BigNumber("200");

        var result = Helpers.Compare(a, b);

        result.Should().BeLessThan(0);
    }

    [Test]
    [Category("Compare")]
    public void Compare_SameLengthEqual_ReturnsZero()
    {
        var a = new BigNumber("123");
        var b = new BigNumber("123");

        var result = Helpers.Compare(a, b);

        result.Should().Be(0);
    }

    [Test]
    [Category("Compare")]
    public void Compare_DifferentLengths_ComparesLength()
    {
        var a = new BigNumber("9");
        var b = new BigNumber("10");

        var result = Helpers.Compare(a, b);

        result.Should().BeLessThan(0);
    }

    [Test]
    [Category("Compare")]
    [TestCase("5", "5", 0)]
    [TestCase("5", "10", -1)]
    [TestCase("100", "50", 1)]
    [TestCase("999", "1000", -1)]
    public void Compare_VariousCases(string aVal, string bVal, int expectedComparison)
    {
        var a = new BigNumber(aVal);
        var b = new BigNumber(bVal);

        var result = Helpers.Compare(a, b);

        if (expectedComparison == 0)
            result.Should().Be(0);
        else if (expectedComparison < 0)
            result.Should().BeLessThan(0);
        else
            result.Should().BeGreaterThan(0);
    }

    #endregion

    #region Swap Tests

    [Test]
    [Category("Swap")]
    public void Swap_ExchangesValues()
    {
        var a = new BigNumber("100");
        var b = new BigNumber("200");

        Helpers.Swap(ref a, ref b);

        a.Value.Should().Be("200");
        b.Value.Should().Be("100");
    }

    #endregion

    #region GetCutPosition Tests

    [Test]
    [Category("GetCutPosition")]
    public void GetCutPosition_BothSingleDigit_ReturnsOne()
    {
        var first = "5";
        var second = "7";

        var result = Helpers.GetCutPosition(first, second);

        result.Should().Be(1);
    }

    [Test]
    [Category("GetCutPosition")]
    public void GetCutPosition_EvenLength_ReturnsMidpoint()
    {
        var first = "1234";
        var second = "5678";

        var result = Helpers.GetCutPosition(first, second);

        result.Should().Be(2);
    }

    [Test]
    [Category("GetCutPosition")]
    public void GetCutPosition_OddLength_ReturnsRoundedUp()
    {
        var first = "12345";
        var second = "67890";

        var result = Helpers.GetCutPosition(first, second);

        result.Should().Be(3);
    }

    [Test]
    [Category("GetCutPosition")]
    public void GetCutPosition_DifferentLengths_UsesShorter()
    {
        var first = "1234";
        var second = "567";

        var result = Helpers.GetCutPosition(first, second);

        result.Should().Be(2);
    }

    #endregion

    #region GetFirstPart Tests

    [Test]
    [Category("GetParts")]
    public void GetFirstPart_RemovesLastNCharacters()
    {
        var str = "12345";
        var cutPos = 2;

        var result = Helpers.GetFirstPart(str, cutPos);

        result.Should().Be("123");
    }

    [Test]
    [Category("GetParts")]
    public void GetFirstPart_CutPositionOne_RemovesLastChar()
    {
        var str = "abc";
        var cutPos = 1;

        var result = Helpers.GetFirstPart(str, cutPos);

        result.Should().Be("ab");
    }

    #endregion

    #region GetSecondPart Tests

    [Test]
    [Category("GetParts")]
    public void GetSecondPart_ReturnLastNCharacters()
    {
        var str = "12345";
        var cutPos = 2;

        var result = Helpers.GetSecondPart(str, cutPos);

        result.Should().Be("45");
    }

    [Test]
    [Category("GetParts")]
    public void GetSecondPart_CutPositionOne_ReturnsLastChar()
    {
        var str = "abc";
        var cutPos = 1;

        var result = Helpers.GetSecondPart(str, cutPos);

        result.Should().Be("c");
    }

    #endregion

    #region Boundary Value Analysis Tests

    [Test]
    [Category("BVA")]
    public void BVA_Trim_WithSingleZero()
    {
        var result = Helpers.Trim("0");
        result.Should().Be("0");
    }

    [Test]
    [Category("BVA")]
    public void BVA_MultiplyByDigit_Boundary9x9()
    {
        var result = Helpers.MultiplyByDigit("9", 9);
        result.Should().Be("81");
    }

    [Test]
    [Category("BVA")]
    public void BVA_Compare_ZeroVsOne()
    {
        var a = new BigNumber("0");
        var b = new BigNumber("1");
        Helpers.Compare(a, b).Should().BeLessThan(0);
    }

    [Test]
    [Category("BVA")]
    public void BVA_DivideByTwo_OnePlusOne()
    {
        var result = Helpers.DivideByTwo("2");
        result.Should().Be("1");
    }

    #endregion

    #region Data Flow Coverage Tests

    [Test]
    [Category("DataFlow")]
    public void DataFlow_MultiplyByDigit_CarryPropagation()
    {
        var result = Helpers.MultiplyByDigit("999", 9);
        result.Should().Be("8991");
    }

    [Test]
    [Category("DataFlow")]
    public void DataFlow_DivideByTwo_CarryState()
    {
        var result = Helpers.DivideByTwo("123");
        result.Should().Be("61");
    }

    [Test]
    [Category("DataFlow")]
    public void DataFlow_Compare_SignalCorrectValue()
    {
        var a = new BigNumber("100");
        var b = new BigNumber("200");
        Helpers.Compare(a, b).Should().Be(-1);
    }

    #endregion
}
