using System.Diagnostics;

//Not optimized
public static class UnitTests
{
    public static void ExecuteAll()
    {
        NullMatrix();
        VerticalRange();
        HorizontalRangeLength();
        HorizontalRangeNull();
        HorizontalRangeEqual();
        HorizontalRangeLetter();
    }

    public static void PrintErrormessage(string message)
    {
        Console.WriteLine($"Validation failed: {new StackTrace().GetFrame(1)?.GetMethod()?.Name} - {message}");
    }

    public static void NullMatrix()
    {
        try
        {
            var wordFinder = new WordFinder(null);
        }
        catch (Exception e)
        {

            if (!(e is ArgumentNullException))
                PrintErrormessage("is not ArgumentNullException");

            if (!e.Message.Contains($"Required parameter was not provided: matrix is null"))
                PrintErrormessage($"Exception message not expected: {e.Message}");
        }
    }

    public static void VerticalRange()
    {
        try
        {
            var matrix = new List<string>();
            var wordFinder = new WordFinder(matrix);
        }
        catch (Exception e)
        {
            if (!(e is ArgumentOutOfRangeException))
                PrintErrormessage("is not ArgumentOutOfRangeException");

            if (!e.Message.Contains($"Matrix vertical length must be between 1 and 64, but it is"))
                PrintErrormessage($"Exception message not expected: {e.Message}");
        }
    }

    public static void HorizontalRangeLength()
    {
        try
        {
            var matrix = new List<string>();
            matrix.Add("");
            var wordFinder = new WordFinder(matrix);
        }
        catch (Exception e)
        {
            if (!(e is ArgumentOutOfRangeException))
                PrintErrormessage("is not ArgumentOutOfRangeException");

            if (!e.Message.Contains($"Matrix horizontal length must be equal in all lines"))
                PrintErrormessage($"Exception message not expected: {e.Message}");
        }
    }

    public static void HorizontalRangeNull()
    {
        try
        {
            var matrix = new List<string>();
            matrix.Add("abcdc");
            matrix.Add(null);
            var wordFinder = new WordFinder(matrix);
        }
        catch (Exception e)
        {
            if (!(e is ArgumentOutOfRangeException))
                PrintErrormessage("is not ArgumentOutOfRangeException");

            if (!e.Message.Contains($"Matrix horizontal length must be equal in all lines"))
                PrintErrormessage($"Exception message not expected: {e.Message}");
        }
    }

    public static void HorizontalRangeEqual()
    {
        try
        {
            var matrix = new List<string>();
            matrix.Add("abcdc");
            matrix.Add("abcdcef");
            var wordFinder = new WordFinder(matrix);
        }
        catch (Exception e)
        {
            if (!(e is ArgumentOutOfRangeException))
                PrintErrormessage("is not ArgumentOutOfRangeException");

            if (!e.Message.Contains($"Matrix horizontal length must be equal in all lines"))
                PrintErrormessage($"Exception message not expected: {e.Message}");
        }
    }

    public static void HorizontalRangeLetter()
    {
        try
        {
            var matrix = new List<string>();
            matrix.Add("abc99dc");
            var wordFinder = new WordFinder(matrix);
        }
        catch (Exception e)
        {
            if (!(e is ArgumentOutOfRangeException))
                PrintErrormessage("is not ArgumentOutOfRangeException");

            if (!e.Message.Contains($"Matrix must have only letters"))
                PrintErrormessage($"Exception message not expected: {e.Message}");
        }
    }
}