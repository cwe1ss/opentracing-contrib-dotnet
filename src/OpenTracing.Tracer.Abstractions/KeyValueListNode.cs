using System;
using System.Collections.Generic;
using System.Text;

namespace OpenTracing.Tracer
{
    public class KeyValueListNode<TValue>
    {
        public KeyValuePair<string, TValue> KeyValue;
        public KeyValueListNode<TValue> Next;
    }
}
