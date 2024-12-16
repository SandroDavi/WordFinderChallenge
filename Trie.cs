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