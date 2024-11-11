using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Dock
{
    
    public static class CollectionsExtensions
    {
        
        public static ReadOnlyCollection<TElement> AsReadOnly<TElement>(this IList<TElement> elements)
        {
            return new ReadOnlyCollection<TElement>(elements);
        }
        public static ReadOnlyCollection<TElement> ToReadOnlyCollection<TElement>(this IEnumerable<TElement> elements)
        {
            return elements.ToArray().AsReadOnly();
        }
        public static int SortedInsert<TElement>(this List<TElement> elements, TElement toInsert, IComparer<TElement> comparer = null)
        {
            var effectiveComparer = comparer ?? Comparer<TElement>.Default;

            if (Application.isEditor)
            {
                for (int iElement = 0; iElement < elements.Count - 1; iElement++)
                {
                    var element = elements[iElement];
                    var nextElement = elements[iElement + 1];

                    if (effectiveComparer.Compare(element, nextElement) > 0)
                    {
                        Debug.LogWarning("Elements must already be sorted to call this method.");
                        break;
                    }
                }
            }

            int searchResult = elements.BinarySearch(toInsert, effectiveComparer);

            int insertionIndex = searchResult >= 0
                ? searchResult
                : ~searchResult;

            elements.Insert(insertionIndex, toInsert);

            return insertionIndex;
        }

        public static void DisposeElements<TElement>(this IEnumerable<TElement> elements)
            where TElement : IDisposable
        {
            foreach (var element in elements)
            {
                if (element != null)
                {
                    element.Dispose();
                }
            }
        }

        public static void DisposeElements<TElement>(this IList<TElement> elements)
            where TElement : IDisposable
        {
            for (int iElement = 0; iElement < elements.Count; iElement++)
            {
                var element = elements[iElement];

                if (element != null)
                {
                    element.Dispose();
                }
            }
        }

        public static T[] ExportDictionaryValuesAsArray<T>(this Dictionary<uint, T> input)
        {
            T[] output = new T[input.Count];
            input.Values.CopyTo(output, 0);
            return output;
        }

        
    }
}