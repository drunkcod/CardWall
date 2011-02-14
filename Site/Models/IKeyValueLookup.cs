using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CardWall.Models
{
    public interface IKeyValueLookup<TKey, TValue>
    {
        bool TryGetValue(TKey key, out TValue value);
    }

    public class DictionaryKeyValueLookup<TKey, TValue> : IKeyValueLookup<TKey, TValue>
    {
        readonly IDictionary<TKey, TValue> inner;

        public DictionaryKeyValueLookup(IDictionary<TKey, TValue> inner) {
            this.inner = inner;
        }

        bool IKeyValueLookup<TKey, TValue>.TryGetValue(TKey key, out TValue value) {
            return inner.TryGetValue(key, out value);
        }
    }
}