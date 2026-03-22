using FluentAssertions;
using LongArithmetic.Core;
using LongArithmetic.Core.Models;

namespace LongArithmetic.Tests;

[TestFixture]
public class BigMathTests
{
    #region Add Tests
    
    [Test]
    [Category("Add")]
    public void Add_SimpleNumbers_ReturnsCorrectSum()
    {
        var a = new BigNumber("123");
        var b = new BigNumber("456");
 
        var result = BigMath.Add(a, b);

        result.Value.Should().Be("579");
    }

    [Test]
    [Category("Add")]
    public void Add_WithCarry_AppliesCarryCorrectly()
    {
        var a = new BigNumber("999");
        var b = new BigNumber("1");

        var result = BigMath.Add(a, b);

        result.Value.Should().Be("1000");
    }

    [Test]
    [Category("Add")]
    public void Add_DifferentLengths_PadsAndAdds()
    {
        var a = new BigNumber("9");
        var b = new BigNumber("1000");

        var result = BigMath.Add(a, b);

        result.Value.Should().Be("1009");
    }

    [Test]
    [Category("Add")]
    public void Add_WithLeadingZeros_Trimmed()
    {
        var a = new BigNumber("0001");
        var b = new BigNumber("0002");

        var result = BigMath.Add(a, b);

        result.Value.Should().Be("3");
    }

    [Test]
    [Category("Add")]
    [TestCase("1", "1", "2")]
    [TestCase("9", "1", "10")]
    [TestCase("99", "1", "100")]
    [TestCase("0", "0", "0")]
    [TestCase("123", "456", "579")]
    [TestCase("999999", "1", "1000000")]
    public void Add_Parametrized(string aVal, string bVal, string expected)
    {
        var a = new BigNumber(aVal);
        var b = new BigNumber(bVal);

        var result = BigMath.Add(a, b);

        result.Value.Should().Be(expected);
    }

    [Test]
    [Category("Add")]
    public void Add_VeryLargeNumbers()
    {
        var a = new BigNumber(new string('9', 1000));
        var b = new BigNumber("1");

        var result = BigMath.Add(a, b);

        result.Value.Should().Be("1" + new string('0', 1000));
    }

    [Test]
    [Category("Add")]
    public void Add_CommutativityVerification()
    {
        var a = new BigNumber("123");
        var b = new BigNumber("456");

        var result1 = BigMath.Add(a, b);
        var result2 = BigMath.Add(b, a);

        result1.Value.Should().Be(result2.Value);
    }

    #endregion

    #region Subtract Tests

    [Test]
    [Category("Subtract")]
    public void Subtract_SimpleNumbers_ReturnsCorrectDifference()
    {
        var a = new BigNumber("456");
        var b = new BigNumber("123");

        var result = BigMath.Subtract(a, b);

        result.Value.Should().Be("333");
    }

    [Test]
    [Category("Subtract")]
    public void Subtract_WithBorrow_AppliesBorrowCorrectly()
    {
        var a = new BigNumber("100");
        var b = new BigNumber("1");

        var result = BigMath.Subtract(a, b);

        result.Value.Should().Be("99");
    }

    [Test]
    [Category("Subtract")]
    public void Subtract_EqualNumbers_ReturnsZero()
    {
        var a = new BigNumber("123");
        var b = new BigNumber("123");

        var result = BigMath.Subtract(a, b);

        result.Value.Should().Be("0");
    }

    [Test]
    [Category("Subtract")]
    public void Subtract_FirstSmaller_ReturnsNegative()
    {
        var a = new BigNumber("100");
        var b = new BigNumber("200");

        var result = BigMath.Subtract(a, b);

        result.Value.Should().StartWith("-");
    }

    [Test]
    [Category("Subtract")]
    [TestCase("10", "1", "9")]
    [TestCase("100", "1", "99")]
    [TestCase("1000", "999", "1")]
    [TestCase("0", "0", "0")]
    [TestCase("5", "5", "0")]
    public void Subtract_Parametrized(string aVal, string bVal, string expected)
    {
        var a = new BigNumber(aVal);
        var b = new BigNumber(bVal);

        var result = BigMath.Subtract(a, b);

        result.Value.Should().Be(expected);
    }

    [Test]
    [Category("Subtract")]
    public void Subtract_VeryLargeNumbers()
    {
        var a = new BigNumber("1" + new string('0', 500));
        var b = new BigNumber("1");

        var result = BigMath.Subtract(a, b);

        result.Value.Should().Be(new string('9', 500));
    }

    [Test]
    [Category("Subtract")]
    public void Subtract_MultipleBorrows()
    {
        var a = new BigNumber("10000");
        var b = new BigNumber("1111");

        var result = BigMath.Subtract(a, b);

        result.Value.Should().Be("8889");
    }

    #endregion

    #region Multiply Tests

    [Test]
    [Category("Multiply")]
    public void Multiply_SimpleNumbers_ReturnsCorrectProduct()
    {
        var a = new BigNumber("12");
        var b = new BigNumber("34");

        var result = BigMath.Multiply(a, b);

        result.Value.Should().Be("408");
    }

    [Test]
    [Category("Multiply")]
    public void Multiply_ByZero_ReturnsZero()
    {
        var a = new BigNumber("999");
        var b = new BigNumber("0");

        var result = BigMath.Multiply(a, b);

        result.Value.Should().Be("0");
    }

    [Test]
    [Category("Multiply")]
    public void Multiply_ByOne_ReturnsSelf()
    {
        var a = new BigNumber("12345");
        var b = new BigNumber("1");

        var result = BigMath.Multiply(a, b);

        result.Value.Should().Be("12345");
    }

    [Test]
    [Category("Multiply")]
    public void Multiply_ClassicMethod_ReturnsCorrectResult()
    {
        var a = new BigNumber("123");
        var b = new BigNumber("456");

        var result = BigMath.Multiply(a, b, "classic");

        result.Value.Should().Be("56088");
    }

    [Test]
    [Category("Multiply")]
    public void Multiply_KaratsubaMethod_ReturnsCorrectResult()
    {
        var a = new BigNumber("123");
        var b = new BigNumber("456");

        var result = BigMath.Multiply(a, b, "karatsuba");

        result.Value.Should().Be("56088");
    }

    [Test]
    [Category("Multiply")]
    public void Multiply_KaratsubaVsClassic_ProduceSameResult()
    {
        var a = new BigNumber(new string('9', 40));
        var b = new BigNumber(new string('8', 40));

        var resultClassic = BigMath.Multiply(a, b, "classic");
        var resultKaratsuba = BigMath.Multiply(a, b, "karatsuba");

        resultClassic.Value.Should().Be(resultKaratsuba.Value);
    }

    [Test]
    [Category("Multiply")]
    [TestCase("5", "2", "10")]
    [TestCase("9", "9", "81")]
    [TestCase("123", "456", "56088")]
    [TestCase("0", "999", "0")]
    [TestCase("1", "1", "1")]
    public void Multiply_Parametrized(string aVal, string bVal, string expected)
    {
        var a = new BigNumber(aVal);
        var b = new BigNumber(bVal);

        var result = BigMath.Multiply(a, b);

        result.Value.Should().Be(expected);
    }

    [Test]
    [Category("Multiply")]
    public void Multiply_VeryLargeNumbers()
    {
        var a = new BigNumber(new string('9', 100));
        var b = new BigNumber("2");

        var result = BigMath.Multiply(a, b);

        result.Value.Should().NotBeNull();
        result.Value.Length.Should().BeGreaterThan(100);
    }

    [Test]
    [Category("Multiply")]
    public void Multiply_Commutativity()
    {
        var a = new BigNumber("789");
        var b = new BigNumber("456");

        var result1 = BigMath.Multiply(a, b);
        var result2 = BigMath.Multiply(b, a);

        result1.Value.Should().Be(result2.Value);
    }

    #endregion

    #region Divide Tests

    [Test]
    [Category("Divide")]
    public void Divide_ExactDivision_ReturnsCorrectQuotient()
    {
        var a = new BigNumber("100");
        var b = new BigNumber("10");

        var result = BigMath.Divide(a, b);

        result.Value.Should().Be("10");
    }

    [Test]
    [Category("Divide")]
    public void Divide_WithRemainder_TruncatesRemainder()
    {
        var a = new BigNumber("23");
        var b = new BigNumber("5");

        var result = BigMath.Divide(a, b);

        result.Value.Should().Be("4");
    }

    [Test]
    [Category("Divide")]
    public void Divide_ByOne_ReturnsItself()
    {
        var a = new BigNumber("12345");
        var b = new BigNumber("1");

        var result = BigMath.Divide(a, b);

        result.Value.Should().Be("12345");
    }

    [Test]
    [Category("Divide")]
    public void Divide_BySelf_ReturnsOne()
    {
        var a = new BigNumber("987");
        var b = new BigNumber("987");

        var result = BigMath.Divide(a, b);

        result.Value.Should().Be("1");
    }

    [Test]
    [Category("Divide")]
    public void Divide_SmallerByLarger_ReturnsZero()
    {
        var a = new BigNumber("5");
        var b = new BigNumber("10");

        var result = BigMath.Divide(a, b);

        result.Value.Should().Be("0");
    }

    [Test]
    [Category("Divide")]
    public void Divide_ByZero_ThrowsDivideByZeroException()
    {
        var a = new BigNumber("100");
        var b = new BigNumber("0");

        Assert.Throws<DivideByZeroException>(() => BigMath.Divide(a, b));
    }

    [Test]
    [Category("Divide")]
    public void Divide_ClassicMethod_ReturnsCorrectResult()
    {
        var a = new BigNumber("100");
        var b = new BigNumber("7");

        var result = BigMath.Divide(a, b, "classic");

        result.Value.Should().Be("14");
    }

    [Test]
    [Category("Divide")]
    public void Divide_BinaryMethod_ReturnsCorrectResult()
    {
        var a = new BigNumber("100");
        var b = new BigNumber("7");

        var result = BigMath.Divide(a, b, "binary");

        result.Value.Should().Be("14");
    }

    [Test]
    [Category("Divide")]
    [TestCase("10", "1", "10")]
    [TestCase("100", "10", "10")]
    [TestCase("23", "5", "4")]
    [TestCase("999", "999", "1")]
    [TestCase("5", "10", "0")]
    public void Divide_Parametrized(string aVal, string bVal, string expected)
    {
        var a = new BigNumber(aVal);
        var b = new BigNumber(bVal);

        var result = BigMath.Divide(a, b);

        result.Value.Should().Be(expected);
    }

    [Test]
    [Category("Divide")]
    public void Divide_VeryLargeNumbers()
    {
        var a = new BigNumber(new string('9', 500));
        var b = new BigNumber("10");

        var result = BigMath.Divide(a, b);

        result.Value.Should().NotBeNull();
    }

    #endregion

    #region Pow Tests

    [Test]
    [Category("Pow")]
    public void Pow_ExponentZero_ReturnsOne()
    {
        var a = new BigNumber("999");
        var exp = 0;

        var result = BigMath.Pow(a, exp);

        result.Value.Should().Be("1");
    }

    [Test]
    [Category("Pow")]
    public void Pow_ExponentOne_ReturnsSelf()
    {
        var a = new BigNumber("123");
        var exp = 1;

        var result = BigMath.Pow(a, exp);

        result.Value.Should().Be("123");
    }

    [Test]
    [Category("Pow")]
    public void Pow_BaseZero_ReturnsZero()
    {
        var a = new BigNumber("0");
        var exp = 5;

        var result = BigMath.Pow(a, exp);

        result.Value.Should().Be("0");
    }

    [Test]
    [Category("Pow")]
    public void Pow_BaseOne_ReturnsOne()
    {
        var a = new BigNumber("1");
        var exp = 100;

        var result = BigMath.Pow(a, exp);

        result.Value.Should().Be("1");
    }

    [Test]
    [Category("Pow")]
    public void Pow_BinaryMethod_ReturnsCorrectResult()
    {
        var a = new BigNumber("2");
        var exp = 10;

        var result = BigMath.Pow(a, exp, "binary");

        result.Value.Should().Be("1024");
    }

    [Test]
    [Category("Pow")]
    public void Pow_IterativeMethod_ReturnsCorrectResult()
    {
        var a = new BigNumber("2");
        var exp = 10;

        var result = BigMath.Pow(a, exp, "iterative");

        result.Value.Should().Be("1024");
    }

    [Test]
    [Category("Pow")]
    public void Pow_BinaryVsIterative_ProduceSameResult()
    {
        var a = new BigNumber("3");
        var exp = 15;

        var resultBinary = BigMath.Pow(a, exp, "binary");
        var resultIterative = BigMath.Pow(a, exp, "iterative");

        resultBinary.Value.Should().Be(resultIterative.Value);
    }

    [Test]
    [Category("Pow")]
    public void Pow_NegativeExponent_ThrowsArgumentException()
    {
        var a = new BigNumber("5");
        var exp = -1;

        Assert.Throws<ArgumentException>(() => BigMath.Pow(a, exp));
    }

    [Test]
    [Category("Pow")]
    [TestCase("2", 0, "1")]
    [TestCase("2", 1, "2")]
    [TestCase("2", 3, "8")]
    [TestCase("10", 2, "100")]
    [TestCase("3", 3, "27")]
    public void Pow_Parametrized(string baseVal, int exp, string expected)
    {
        var a = new BigNumber(baseVal);

        var result = BigMath.Pow(a, exp);

        result.Value.Should().Be(expected);
    }

    [Test]
    [Category("Pow")]
    public void Pow_LargeExponent()
    {
        var a = new BigNumber("2");
        var exp = 50;

        var result = BigMath.Pow(a, exp);

        result.Value.Should().Be("1125899906842624");
    }

    #endregion

    #region Boundary Value Analysis Tests

    [Test]
    [Category("BVA")]
    public void BVA_Add_9Plus1()
    {
        var result = BigMath.Add(new BigNumber("9"), new BigNumber("1"));
        result.Value.Should().Be("10");
    }

    [Test]
    [Category("BVA")]
    public void BVA_Add_99Plus1()
    {
        var result = BigMath.Add(new BigNumber("99"), new BigNumber("1"));
        result.Value.Should().Be("100");
    }

    [Test]
    [Category("BVA")]
    public void BVA_Subtract_ZeroMinusZero()
    {
        var result = BigMath.Subtract(new BigNumber("0"), new BigNumber("0"));
        result.Value.Should().Be("0");
    }

    [Test]
    [Category("BVA")]
    public void BVA_Divide_OneByOne()
    {
        var result = BigMath.Divide(new BigNumber("1"), new BigNumber("1"));
        result.Value.Should().Be("1");
    }

    [Test]
    [Category("BVA")]
    public void BVA_Multiply_NineByNine()
    {
        var result = BigMath.Multiply(new BigNumber("9"), new BigNumber("9"));
        result.Value.Should().Be("81");
    }

    [Test]
    [Category("BVA")]
    public void BVA_Pow_TwoByTwo()
    {
        var result = BigMath.Pow(new BigNumber("2"), 2);
        result.Value.Should().Be("4");
    }

    [Test]
    [Category("BVA")]
    public void BVA_Divide_DecimalRemainder()
    {
        var result = BigMath.Divide(new BigNumber("7"), new BigNumber("2"));
        result.Value.Should().Be("3");
    }

    #endregion

    #region Equivalence Partitioning Tests

    [Test]
    [Category("EquivalencePartitioning")]
    public void EP_SingleDigitNumbers_Add()
    {
        var result = BigMath.Add(new BigNumber("3"), new BigNumber("5"));
        result.Value.Should().Be("8");
    }

    [Test]
    [Category("EquivalencePartitioning")]
    public void EP_MultiDigitNumbers_Multiply()
    {
        var result = BigMath.Multiply(new BigNumber("123"), new BigNumber("456"));
        result.Value.Should().Be("56088");
    }

    [Test]
    [Category("EquivalencePartitioning")]
    public void EP_VeryLongNumbers_Divide()
    {
        var largeNum = new BigNumber(new string('9', 100));
        var divisor = new BigNumber("3");
        var result = BigMath.Divide(largeNum, divisor);
        result.Value.Should().NotBeNull();
    }

    [Test]
    [Category("EquivalencePartitioning")]
    public void EP_ZeroInOperation()
    {
        var result = BigMath.Multiply(new BigNumber("0"), new BigNumber("99999"));
        result.Value.Should().Be("0");
    }

    #endregion

    #region Branch Coverage Tests

    [Test]
    [Category("BranchCoverage")]
    public void Branch_Add_WithoutCarry()
    {
        var result = BigMath.Add(new BigNumber("123"), new BigNumber("456"));
        result.Value.Should().Be("579");
    }

    [Test]
    [Category("BranchCoverage")]
    public void Branch_Add_WithCarry()
    {
        var result = BigMath.Add(new BigNumber("999"), new BigNumber("999"));
        result.Value.Should().Be("1998");
    }

    [Test]
    [Category("BranchCoverage")]
    public void Branch_Subtract_WithoutBorrow()
    {
        var result = BigMath.Subtract(new BigNumber("999"), new BigNumber("11"));
        result.Value.Should().Be("988");
    }

    [Test]
    [Category("BranchCoverage")]
    public void Branch_Subtract_WithBorrow()
    {
        var result = BigMath.Subtract(new BigNumber("100"), new BigNumber("99"));
        result.Value.Should().Be("1");
    }

    [Test]
    [Category("BranchCoverage")]
    public void Branch_Multiply_SingleDigitBranch()
    {
        var result = BigMath.Multiply(new BigNumber("5"), new BigNumber("3"));
        result.Value.Should().Be("15");
    }

    [Test]
    [Category("BranchCoverage")]
    public void Branch_Divide_ExactDivision()
    {
        var result = BigMath.Divide(new BigNumber("20"), new BigNumber("5"));
        result.Value.Should().Be("4");
    }

    [Test]
    [Category("BranchCoverage")]
    public void Branch_Divide_WithRemainder()
    {
        var result = BigMath.Divide(new BigNumber("21"), new BigNumber("5"));
        result.Value.Should().Be("4");
    }

    [Test]
    [Category("BranchCoverage")]
    public void Branch_Pow_BinaryEvenExponent()
    {
        var result = BigMath.Pow(new BigNumber("3"), 4);
        result.Value.Should().Be("81");
    }

    [Test]
    [Category("BranchCoverage")]
    public void Branch_Pow_BinaryOddExponent()
    {
        var result = BigMath.Pow(new BigNumber("3"), 3);
        result.Value.Should().Be("27");
    }

    #endregion

    #region Data Flow Coverage Tests

    [Test]
    [Category("DataFlow")]
    public void DataFlow_Add_CarryPropagation()
    {
        var result = BigMath.Add(new BigNumber("999"), new BigNumber("1"));
        result.Value.Should().Be("1000");
    }

    [Test]
    [Category("DataFlow")]
    public void DataFlow_Subtract_BorrowPropagation()
    {
        var result = BigMath.Subtract(new BigNumber("1000"), new BigNumber("1"));
        result.Value.Should().Be("999");
    }

    [Test]
    [Category("DataFlow")]
    public void DataFlow_Multiply_IndexCalculation()
    {
        var result = BigMath.Multiply(new BigNumber("12"), new BigNumber("34"));
        result.Value.Should().Be("408");
    }

    #endregion

    #region Assertion Variety Tests

    [Test]
    [Category("Assertions")]
    public void AssertEqual_Verify()
    {
        var result = BigMath.Add(new BigNumber("1"), new BigNumber("1"));
        Assert.That(result.Value, Is.EqualTo("2"));
    }

    [Test]
    [Category("Assertions")]
    public void AssertNotEqual_Verify()
    {
        var result1 = BigMath.Add(new BigNumber("1"), new BigNumber("1"));
        var result2 = BigMath.Add(new BigNumber("1"), new BigNumber("2"));
        Assert.That(result1.Value, Is.Not.EqualTo(result2.Value));
    }

    [Test]
    [Category("Assertions")]
    public void AssertTrue_Verify()
    {
        var result = BigMath.Divide(new BigNumber("10"), new BigNumber("2"));
        Assert.That(result.Value.Length > 0, Is.True);
    }

    [Test]
    [Category("Assertions")]
    public void AssertFalse_Verify()
    {
        var result = BigMath.Multiply(new BigNumber("0"), new BigNumber("999"));
        Assert.That(result.Value == "999", Is.False);
    }

    [Test]
    [Category("Assertions")]
    public void AssertThrows_Verify()
    {
        Assert.Throws<DivideByZeroException>(() => BigMath.Divide(new BigNumber("5"), new BigNumber("0")));
    }

    [Test]
    [Category("Assertions")]
    public void AssertNotNull_Verify()
    {
        var result = BigMath.Add(new BigNumber("1"), new BigNumber("1"));
        Assert.That(result.Value, Is.Not.Null);
    }

    [Test]
    [Category("Assertions")]
    public void AssertContains_Verify()
    {
        var result = BigMath.Multiply(new BigNumber("123"), new BigNumber("456"));
        Assert.That(result.Value, Does.Contain("8"));
    }

    [Test]
    [Category("Assertions")]
    public void AssertInRange_Verify()
    {
        var result = BigMath.Add(new BigNumber("1"), new BigNumber("1"));
        Assert.That(result.Value.Length, Is.InRange(1, 10));
    }

    #endregion
}
