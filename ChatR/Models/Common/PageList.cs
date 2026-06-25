namespace ChatR.Models.Common;

public class PageList<T>
{
    public List<T> Items { get; set; }
    public int Total { get; set; }

    public PageList(List<T> items, int total)
    {
        Items = items;
        Total = total;
    }
}
