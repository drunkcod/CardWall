using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CardWall.Models
{
    public interface IBuilder<TKey, TValue>
    {
        bool TryBuild(TKey input, out TValue output);
    }

    public class DictionaryBuilderAdapter<TKey, TValue> : IBuilder<TKey, TValue>
    {
        readonly IDictionary<TKey, TValue> inner;

        public DictionaryBuilderAdapter(IDictionary<TKey, TValue> inner) {
            this.inner = inner;
        }

        bool IBuilder<TKey, TValue>.TryBuild(TKey input, out TValue output) {
            return inner.TryGetValue(input, out output);
        }
    }
}