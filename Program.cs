UnitTests.ExecuteAll();
MainTest.Execute();

public class MainTest
{
    public static void Execute()
    {
        var matrix = CreateMatrix();
        var wordstream = CreateWordstream();

        WordFinder wf = new WordFinder(matrix);
        wf.ThreadingMode = ThreadingMode.Synchronous; // Synchronous, Task (TPL), Thread
        wf.ThreadCount = 3; // Number of tasks/threads (1 - 10)

        var result = wf.Find(wordstream);

        foreach (var top in result)
            Console.WriteLine($"{top}");

        List<string> CreateMatrix() => "abcdc,fgwio,chill,pqnsd,uvdxy,chill,chill".Split(',').ToList<string>();
        List<string> CreateWordstream() => "cold,wind,snow,chill,wind".Split(',').ToList<string>();

        Console.ReadLine();
    }
}
