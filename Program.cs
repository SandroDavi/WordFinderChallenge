/*  Developer Challenge - Word Finder - for QU Beyond Matrix

The code created was tested in LINQPad 8 using .NET 8, in Visual Studio 2022 in .NET 7 and can be found in Fiddler (.NET 9)
at the link below (private, accessible only through this link):
- https://dotnetfiddle.net/5IKH3B

In the email I attached two files, one containing the requested class, another more complete one that also contains the tests
and a version for LINQPad.

Some comments were kept in the code to highlight observations of interest.



The analysis follows:

Based on the dimensions provided on the input data, I concluded that the best strategy would be to index the Matrix, allowing
fast searches when iterating over the Wordstream.

Preprocessing the matrix: as shown below in the method header, the volume of data justifies its indexing, it was decided to use
the Trie data structure for this, all horizontal and vertical substrings were indexed. Due to the low volume of data in the matrix,
I understood that it would be better to improve the readability of the code by using Linq.

Find Method: Implemented iteration through all elements, executing the search in the Trie. In addition to synchronous execution,
multithreaded execution modes were also implemented, with options for Task (TPL) or Thread, however, their use seems unnecessary
since concurrent access to the structure that stores the Top Words cancels out the speed gains.

NOTE: It was confirmed that Threads are working correctly through tests. To reproduce these tests in a simplified way, add "Sleep(100)"
in the EvaluateWord method and adjust the Wordstream to 100 words, then run with different ThreadingMode and ThreadCount settings,
the performance gain can be observed in the processing time.

Storage of Most Frequent Words (TopWords):
To manage the most frequent words, I used a PriorityQueue and a HashSet, including a control to avoid including words below the
minimum count. The result is sorted by the most accessed. This implementation will keep only the 10 most accessed words in memory,
and together with the implemented minimum count control, it will avoid excessive maintenance in the PriorityQueue, thus with low
memory usage and high processing efficiency.

IMPORTANT: Considering the volume of data possible in the Wordstream, the search performance in the Trie, and the minimum count
control of TopWords, I understood that it would be better to process repeated words again in the Wordstream, avoiding the creation
of another queue that could consume a lot of memory.

FOR DISCUSSION: One point that I would bring to the team for discussion is the use of two structures in parallel to control TopWords.

Unit Tests: A simplified version of unit tests was included to test basic validations and support development.

Main Test: Tests for the main functionality were also implemented, including measurement of execution time. In addition to the most
basic tests, more advanced tests with up to 10 million records were performed. Eventually, not all advanced tests will be included
in the version provided, allowing for better code readability, however they will be available upon request.

About the suggested interface: considering that the suggested interface was encapsulated in a class, and because the constructor
participates in this contract, I understood that I should not use interface implementation or abstract class implementation in
respect of the provided model.

This code has been set to be case insensitive, but could easily be adjusted or parameterized.
*/

using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;

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
    }
}

public class WordFinder
{
    public ThreadingMode ThreadingMode { get; set; } = ThreadingMode.Synchronous; // Synchronous, Task (TPL), Thread
    public int ThreadCount { get; set; } = 3; // Number of tasks/threads (1 - 10)

    Trie trie = null;
    TopWords topWords = null;

    //Since converting the input matrix is not a heavy process according to established limits, I figured
    //  it would be better to use a more readable code. But it allows optimizations by reading the matrix
    //  only once, and avoiding LINQ (Count, etc.).

    // Time Complexity:
    //   Let l = number of lines, n = total items = l².      - 64² => n=4096
    //   Inserting all words into the trie takes O(l³) time: -
    //   - We have l lines,                                  -
    //   - Each line has l substrings to insert,             -
    //   - Each insertion can cost up to O(l).               - O(64)
    //   Thus, O(l*l*l) = O(l³).                             - 262,144
    //   Since l = √n, we have O(n*√n).                      -
    //
    // For this analysis the Space Complexity will be the same as Time Complexity
    public WordFinder(IEnumerable<string> matrix)
    {
        var sw = Stopwatch.StartNew();

        ValidateMatrix(matrix);

        var verticalStrings = Enumerable.Range(0, matrix.First().Length)
            .Select(col => new string(matrix.Select(row => row[col]).ToArray()))
            .ToList();

        trie = new Trie();
        topWords = new TopWords();

        //Case insensitive
        foreach (var line in matrix)
            for (var i = 0; i < line.Length; i++)
                trie.Add(line.Substring(i).ToLower());

        foreach (var line in verticalStrings)
            for (var i = 0; i < line.Length; i++)
                trie.Add(line.Substring(i).ToLower());

        sw.Stop();
        Console.WriteLine($"WordFinder.ctor: {sw.Elapsed.TotalMilliseconds:#,##0.000} ms");
    }

    public IEnumerable<string> Find(IEnumerable<string> wordstream)
    {
        var sw = Stopwatch.StartNew();

        if (trie == null)
            throw new InvalidOperationException("[WordFinder.Find]The class was not initialized properly (trie == null)");

        switch (ThreadingMode)
        {
            case ThreadingMode.Synchronous:
                foreach (var word in wordstream)
                    EvaluateWord(word);
                break;

            case ThreadingMode.Task:
                FindParallel_Task(wordstream, ThreadCount);
                break;

            case ThreadingMode.Thread:
                FindParallel_Thread(wordstream, ThreadCount);
                break;

            default:
                throw new Exception("Invalid operation mode");
        }

        sw.Stop();
        Console.WriteLine($"WordFinder.Find: {sw.Elapsed.TotalMilliseconds:#,##0.000} ms");

        return topWords.GetTop().Select(_ => _.word);
    }

    public void FindParallel_Thread(IEnumerable<string> wordstream, int threadCount)
    {
        ConcurrentQueue<string> wordQueue = new ConcurrentQueue<string>(wordstream);

        List<Thread> threads = new List<Thread>();

        for (int i = 0; i < threadCount; i++)
        {
            Thread thread = new Thread(() =>
            {
                while (wordQueue.TryDequeue(out string word))
                    EvaluateWord(word);
            });

            threads.Add(thread);
        }

        foreach (var thread in threads)
            thread.Start();

        foreach (var thread in threads)
            thread.Join();
    }

    public void FindParallel_Task(IEnumerable<string> wordstream, int threadCount)
    {
        ConcurrentQueue<string> wordQueue = new ConcurrentQueue<string>(wordstream);
        List<Task> tasks = new List<Task>();

        for (int i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                while (wordQueue.TryDequeue(out string word))
                    EvaluateWord(word);
            }));
        }

        Task.WaitAll(tasks.ToArray());
    }

    public void EvaluateWord(string word)
    {
        //To test threads. It will allow you to confirm that the multithreaded mode implementation
        //  is working properly, see the instructions in the introductory analysis.
        //Thread.Sleep(100);

        var wordCount = trie.StartsWith(word.ToLower());

        if (wordCount > 0)
            topWords.Add(word.ToLower(), wordCount);

        return;
    }

    //Not optimized as previously commented
    void ValidateMatrix(IEnumerable<string> matrix)
    {
        if (matrix == null)
            throw new ArgumentNullException("[WordFinder.ctor]Required parameter was not provided: matrix is null");

        if (!ValidRange(matrix.Count()))
            throw new ArgumentOutOfRangeException($"[WordFinder.ctor]Matrix vertical length must be between 1 and 64, but it is {matrix.Count()}");

        //StackTrace could be used to get the method name
        if (!matrix.All(line => line != null && ValidRange(line.Length) && line.Length == matrix.First().Length))
            throw new ArgumentOutOfRangeException($"[WordFinder.ctor]Matrix horizontal length must be equal in all lines, between 1 and 64");

        if (!matrix.SelectMany(line => line).All(c => Char.IsLetter(c)))
            throw new ArgumentOutOfRangeException($"[WordFinder.ctor]Matrix must have only letters");

        bool ValidRange(int count) => count is >= 1 and <= 64;
    }

    void ValidateFind()
    {
        if (ValidRange((int)ThreadingMode, 0, 2))
            throw new ArgumentOutOfRangeException("[WordFinder.Find]ThreadingMode must be between 0 and 2");

        if (ValidRange(ThreadCount, 1, 10))
            throw new ArgumentOutOfRangeException("[WordFinder.Find]ThreadCount must be between 1 and 10");

        bool ValidRange(int count, int min, int max) => count >= min && count <= max;
    }
}

public enum ThreadingMode
{
    Synchronous = 0,
    Task = 1,
    Thread = 2
}

public class TopWords
{
    const int capacity = 10;
    readonly PriorityQueue<(string word, int count), int> queue = new PriorityQueue<(string word, int count), int>();
    readonly HashSet<string> itemSet = new HashSet<string>();
    int minimumCount = 0; // optimization
    static object objectLock = new object();

    public void Add(string word, int count)
    {
        if (count > minimumCount)
            lock (objectLock)
            {
                if (!itemSet.Contains(word))
                {
                    queue.Enqueue((word, count), count);
                    itemSet.Add(word);

                    if (queue.Count > capacity)
                    {
                        var removed = queue.Dequeue();
                        itemSet.Remove(removed.word);
                        minimumCount = removed.count;
                    }
                }
            }
    }

    public List<(string word, int count)> GetTop()
    {
        var items = queue.UnorderedItems.Select(x => x.Element).ToList();
        return items.OrderByDescending(o => o.count).ToList();
    }
}

class TrieNode
{
    public Dictionary<char, TrieNode> children { get; } = new();
    public int count;
}

class Trie
{
    private readonly TrieNode root = new();

    public void Add(string word)
    {
        var currentNode = root;

        foreach (var c in word)
        {
            if (!currentNode.children.ContainsKey(c))
                currentNode.children[c] = new TrieNode();

            currentNode = currentNode.children[c];
            currentNode.count++;
        }
    }

    public int StartsWith(string word)
    {
        var currentNode = root;

        foreach (var c in word)
            if (!currentNode.children.TryGetValue(c, out currentNode))
                return 0;

        return currentNode.count;
    }
}

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
