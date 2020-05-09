using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace QueryProviderTest{
    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement> {

        TKey key;

        IEnumerable<TElement> group;

        public Grouping(TKey key, IEnumerable<TElement> group) {

         this.key = key;

            this.group = group;

        }

        public TKey Key {

            get { return this.key; }

        }

        public IEnumerator<TElement> GetEnumerator() {

            return this.group.GetEnumerator();

        }

       IEnumerator IEnumerable.GetEnumerator() {

            return this.group.GetEnumerator();

        }

    }
}