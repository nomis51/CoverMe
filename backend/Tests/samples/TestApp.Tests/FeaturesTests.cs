namespace TestApp.Tests;

public class FeaturesTests
{
    [Theory]
    [InlineData(1,2,3)]
    [InlineData(2,2,4)]
    public void Sum_ShouldSumTwoNumbers(int a, int b, int expected)
    {
       // arrange
       var sut = new Feature();
       
       // act
       var result = sut.Sum(a, b);
       
       // assert
       Assert.Equal(expected, result);
    }
}