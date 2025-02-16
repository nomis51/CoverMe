namespace TestApp.Core;

public class Feature2
{
    public int Sum(int a, int b)
    {
        if (a == 2)
        {
            return b + 2;
        }

        return a + b;
    }

    public int Subtract(int a, int b)
    {
        return a - b;
    }

    public int Multiply(int a, int b)
    {
        return a * b;
    }
}