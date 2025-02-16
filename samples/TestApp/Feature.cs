namespace TestApp;

public class SomeObject
{
    public int Value { get; set; }
}

public class Feature
{
    public List<int> DoSomething(List<List<int>> input, Dictionary<string, List<int>> dict)
    {
        return input.SelectMany(x => x)
            .ToList();
    }
    
    public SomeObject GetSomeObject()
    {
        return new SomeObject
        {
            Value = 42
        };
    }
    
    public T? Do<T>(T input)
    {
        if (input is int) return default;
        
        return input;
    }

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