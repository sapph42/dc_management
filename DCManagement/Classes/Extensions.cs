﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCManagement.Classes; 
public static class Extensions {
    private static readonly Random _rand = new();
    public static TSource Random<TSource>(this IEnumerable<TSource> source) {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        IList<TSource> list = source as IList<TSource> ?? new List<TSource>(source);
        if (list.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements");
        else if (list.Count == 1)
            return list[0];
        int index = _rand.Next(list.Count);
        return list[index];
    }
}