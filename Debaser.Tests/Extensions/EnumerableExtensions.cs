namespace Debaser.Tests.Extensions;

public static class EnumerableExtensions
{
    static readonly ThreadLocal<Random> Random = new ThreadLocal<Random>(() => new Random(DateTime.Now.GetHashCode()));

    public static List<TItem> InRandomOrder<TItem>(this IEnumerable<TItem> items)
    {
        var list = items.ToList();

        Action<int,int> swapItems = (index1, index2) =>
        {
            var item1 = list[index1];
            var item2 = list[index2];
            list[index1] = item2;
            list[index2] = item1;
        };

        for (var counter = 0; counter < list.Count * 2; counter++)
        {
            var index1 = Random.Value.Next(list.Count);
            var index2 = Random.Value.Next(list.Count);
            swapItems(index1, index2);
        }

        return list;
    }
}