namespace MoogleEngine;

public class MyList<T>
{

}

public class SearchItem : IComparable<SearchItem>
{
    public SearchItem(string title, string snippet, float score)
    {

        this.Title = title;
        this.Snippet = snippet;
        this.Score = score;
    }

    public string Title { get; private set; }

    public string Snippet { get; private set; }

    public float Score { get; private set; }

    public int CompareTo(SearchItem? other)
    {
        if(other is null) return 0;
        if (this.Score < other.Score)return 1;
        else if (this.Score == other?.Score)return 0;
        else return -1;
    }
}
