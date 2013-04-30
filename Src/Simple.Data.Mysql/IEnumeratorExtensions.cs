using System;
using System.Collections.Generic;

namespace Simple.Data.Mysql
{
    internal static class IEnumeratorExtensions
    {
        public static T GetNext<T>(this IEnumerator<T> enumerator)
        {
            if (enumerator == null) throw new NullReferenceException();
            if (enumerator.MoveNext())
            {
                return enumerator.Current;
            }
            return default(T);
        }

        public static IEnumerable<T> GetNextUntil<T>(this IEnumerator<T> enumerator, Predicate<T> predicate)
        {
            var items = new List<T>();
            enumerator.ForEachUntil(items.Add, predicate);
            return items;
        }

        public static IEnumerator<T> ForEachUntil<T>(this IEnumerator<T> enumerator, Action<T> action, Predicate<T> predicate)
        {
            var sequenceEnded = false;
            return enumerator.ForEachUntil(action, predicate, out sequenceEnded);
        }

        public static IEnumerator<T> ForEachUntil<T>(this IEnumerator<T> enumerator, Action<T> action, Predicate<T> predicate, out Boolean sequenceEnded)
        {
            if (enumerator == null) throw new NullReferenceException();
            if (action == null) throw new ArgumentNullException("action");
            if (predicate == null) throw new ArgumentNullException("predicte");
            sequenceEnded = false;
            while (!predicate(enumerator.Current) && !sequenceEnded)
            {
                action(enumerator.Current);
                sequenceEnded = !enumerator.MoveNext();
            }
            return enumerator;
        }

        public static IEnumerator<T> MoveUntil<T>(this IEnumerator<T> enumerator, Predicate<T> predicate)
        {
            var sequenceEnded = false;
            return enumerator.MoveUntil(predicate, out sequenceEnded);
        }

        public static IEnumerator<T> MoveUntil<T>(this IEnumerator<T> enumerator, Predicate<T> predicate, out Boolean sequenceEnded)
        {
            return enumerator.ForEachUntil(_ => { }, predicate, out sequenceEnded);
        }
    }
}
