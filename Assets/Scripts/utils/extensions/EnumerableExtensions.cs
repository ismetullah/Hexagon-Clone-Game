using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EnumerableExtensions
{
    public static T PickRandom<T>(this ICollection<T> source)
    {
        int r = UnityEngine.Random.Range(0, source.Count);
        return source.ElementAt(r);
    }

    public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
    {
        return source.Shuffle().Take(count);
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        return source.OrderBy(x => Guid.NewGuid());
    }

    /**
     * Copy elements from another list.
     */
    public static void CopyFrom<T>(this List<T> newGrid, List<T> copyGrid)
    {
        newGrid.Clear();
        for (int x = 0; x < copyGrid.Count; x++)
        {
            newGrid.Add(copyGrid[x]);
        }
    }
}