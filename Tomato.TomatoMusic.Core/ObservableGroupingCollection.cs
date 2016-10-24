using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomato.TomatoMusic
{
    public class ObservableGroupingCollection<TKey, TElement> : ReadOnlyObservableCollection<IGrouping<TKey, TElement>>
    {
        private readonly IReadOnlyList<TElement> _source;
        private readonly IComparer<TElement> _orderer;
        private readonly IComparer<TKey> _keyOrderer;
        private readonly Func<TElement, TKey> _keySelector;

        public ObservableGroupingCollection(IReadOnlyList<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey> keyOrderer, IComparer<TElement> orderer)
            : base(new ObservableCollection<IGrouping<TKey, TElement>>())
        {
            _source = source;
            _keySelector = keySelector;
            _keyOrderer = keyOrderer;
            _orderer = orderer;
            var eventSource = _source as INotifyCollectionChanged;
            if (eventSource != null)
                eventSource.CollectionChanged += source_CollectionChanged;
            Reset();
        }

        private void source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems.Count != 0)
                        Add(e.NewItems.Cast<TElement>());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems.Count != 0)
                        Remove(e.OldItems.Cast<TElement>());
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems.Count != 0)
                        Replace(e.OldItems.Cast<TElement>(), e.NewItems.Cast<TElement>());
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Reset();
                    break;
                default:
                    break;
            }
        }

        private void Replace(IEnumerable<TElement> oldItems, IEnumerable<TElement> newItems)
        {
            foreach (var item in oldItems.Select(o => new { ToAdd = false, Value = o })
                .Concat(oldItems.Select(o => new { ToAdd = true, Value = o })).GroupBy(o => _keySelector(o.Value)))
            {
                var group = GetOrAddGroup(item.Key);
                item.Where(o => !o.ToAdd).Sink(o => group.Remove(o.Value));
                item.Where(o => o.ToAdd).Sink(o => group.Add(o.Value));
                if (group.Count == 0)
                    base.Items.Remove(group);
            }
        }

        private void Remove(IEnumerable<TElement> items)
        {
            foreach (var item in items.GroupBy(o => _keySelector(o)))
            {
                var group = GetOrAddGroup(item.Key);
                item.Sink(o => group.Remove(o));
                if (group.Count == 0)
                    base.Items.Remove(group);
            }
        }

        private void Add(IEnumerable<TElement> items)
        {
            foreach (var item in items.GroupBy(o => _keySelector(o)))
            {
                var group = GetOrAddGroup(item.Key);
                item.Sink(group.Add);
            }
        }

        private Grouping GetOrAddGroup(TKey key)
        {
            var idx = base.Items.BinarySearch(key, (v, s) => _keyOrderer.Compare(v, s.Key));
            if (idx < 0)
            {
                var group = new Grouping(key, _orderer, Enumerable.Empty<TElement>());
                base.Items.Insert(~idx, group);
                return group;
            }
            return (Grouping)base.Items[idx];
        }

        private void Reset()
        {
            base.Items.Clear();
            _source.GroupBy(_keySelector).OrderBy(o => o.Key, _keyOrderer)
                .Select(o => new Grouping(o.Key, _orderer, o)).Sink(base.Items.Add);
        }

        class Grouping : ObservableCollection<TElement>, IGrouping<TKey, TElement>
        {
            private readonly IComparer<TElement> _orderer;
            public TKey Key { get; }

            public Grouping(TKey key, IComparer<TElement> orderer, IEnumerable<TElement> items)
                : base(items.OrderBy(o => o, orderer))
            {
                Key = key;
                _orderer = orderer;
            }

            public new void Add(TElement value)
            {
                var idx = base.Items.BinarySearch(value, _orderer);
                base.Items.Insert(idx < 0 ? ~idx : idx, value);
            }
        }
    }

    public static class IListExtensions
    {
        /// <summary>
        /// Performs a binary search on the specified collection.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <typeparam name="TSearch">The type of the searched item.</typeparam>
        /// <param name="list">The list to be searched.</param>
        /// <param name="value">The value to search for.</param>
        /// <param name="comparer">The comparer that is used to compare the value with the list items.</param>
        /// <returns></returns>
        public static int BinarySearch<TItem, TSearch>(this IList<TItem> list, TSearch value, Func<TSearch, TItem, int> comparer)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }

            int lower = 0;
            int upper = list.Count - 1;

            while (lower <= upper)
            {
                int middle = lower + (upper - lower) / 2;
                int comparisonResult = comparer(value, list[middle]);
                if (comparisonResult < 0)
                {
                    upper = middle - 1;
                }
                else if (comparisonResult > 0)
                {
                    lower = middle + 1;
                }
                else
                {
                    return middle;
                }
            }

            return ~lower;
        }

        /// <summary>
        /// Performs a binary search on the specified collection.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="list">The list to be searched.</param>
        /// <param name="value">The value to search for.</param>
        /// <returns></returns>
        public static int BinarySearch<TItem>(this IList<TItem> list, TItem value)
        {
            return BinarySearch(list, value, Comparer<TItem>.Default);
        }

        /// <summary>
        /// Performs a binary search on the specified collection.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="list">The list to be searched.</param>
        /// <param name="value">The value to search for.</param>
        /// <param name="comparer">The comparer that is used to compare the value with the list items.</param>
        /// <returns></returns>
        public static int BinarySearch<TItem>(this IList<TItem> list, TItem value, IComparer<TItem> comparer)
        {
            return list.BinarySearch(value, comparer.Compare);
        }
    }
}
