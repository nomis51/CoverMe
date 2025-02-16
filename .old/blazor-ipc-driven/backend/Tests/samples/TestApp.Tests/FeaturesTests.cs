namespace TestApp.Tests;

public class FeaturesTests
{
    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(2, 2, 4)]
    public void Sum_ShouldSumTwoNumbers(int a, int b, int expected)
    {
        // arrange
        var sut = new Feature();

        // act
        var result = sut.Sum(a, b);

        // assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Do_ShouldReturnNullForInt()
    {
        // arrange
        var sut = new Feature();

        // act
        var result = sut.Do(3);

        // assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Do_ShouldReturnInput()
    {
        // arrange
        var sut = new Feature();

        // act
        var result = sut.Do(new object());

        // assert
        Assert.NotNull(result);
    }

    [Fact]
    public void GetSomeObject_ShouldReturnSomeObject()
    {
        // arrange
        var sut = new Feature();

        // act
        var result = sut.GetSomeObject();

        // assert
        Assert.NotNull(result);
    }

    [Fact]
    public void DoSomething_ShouldReturnInput()
    {
        // arrange
        var sut = new Feature();

        // act
        var result = sut.DoSomething(
            [],
            new Dictionary<string, List<int>>()
        );

        // assert
        Assert.NotNull(result);
    }
}