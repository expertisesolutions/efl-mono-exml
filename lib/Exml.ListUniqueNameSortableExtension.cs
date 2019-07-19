using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace Exml
{

public interface IUniqueNameSortable
{
    string Name { get; }
}

internal static class ListUniqueNameSortableExtension
{
    public static void AddByName<T>(this List<T> list, T item) where T : IUniqueNameSortable
    {
        int index = ~ list.BinarySearch(item, new NameSortableComparer<T>());
        Debug.Assert(index >= 0);
        list.Insert(index, item);
    }

    public static T GetByName<T>(this List<T> list, string name) where T : IUniqueNameSortable
    {
        int index = list.BinarySearch(item, new NameSortableComparer<T>());
        if (index < 0)
        {
            return default(T);
        }

        return list[index];
    }
}

internal class NameSortableComparer<T> : IComparer<T> where T : IUniqueNameSortable
{
    public int Compare(T x, T y)
    {
        return x.Name.Compare(y.Name);
    }
}

}
