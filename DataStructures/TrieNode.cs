public class TrieNode
{
    public Dictionary<char, TrieNode> children { get; } = new();
    public int count;
}