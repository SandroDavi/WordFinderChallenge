using System.Data;

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