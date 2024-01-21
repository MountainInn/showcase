using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;


static public class IEnumerableExt
{
    public static void DestroyAll<T>(this IEnumerable<T> source)
        where T : Component
    {
        foreach (var item in source) MonoBehaviour.Destroy(item.gameObject);
    }

    static public T GetRandom<T>(this IEnumerable<T> source)
    {
        int id = UnityEngine.Random.Range(0, source.Count());

        return source.ElementAt(id);
    }

    static public bool None<T>(this IEnumerable<T> source)
    {
        return source == null || !source.Any();
    }

    static public bool None<T>(this IEnumerable<T> source, Func<T, bool> pred)
    {
        return !source.Any(pred);
    }

    static public IEnumerable<T> Map<T>(this IEnumerable<T> source, Action<T> action)
    {
        if (source.Count() == 0)
        {
            return source;
        }

        foreach (var item in source)
        {
            if (item == null) continue;
            action.Invoke(item);
        }

        return source;
    }
}
