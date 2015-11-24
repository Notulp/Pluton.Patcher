using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Pluton.Patcher.JSON
{
    public class Array : IEnumerable<Value>, IEnumerable
    {
        private readonly List<Value> values = new List<Value> ();

        public int Length => values.Count;

        public Value this [int index] {
            get {
                return values [index];
            }
            set {
                values [index] = value;
            }
        }

        public Array () {}

        public Array (JSON.Array other)
        {
            foreach (Value current in other.values) {
                values.Add (new Value (current));
            }
        }

        public static JSON.Array Parse (string jsonString)
        {
            JSON.Object obj = JSON.Object.Parse ("{ \"array\" :" + jsonString + '}');
            return (obj != null) ? obj.GetValue ("array").Array : null;
        }

        public void Add (Value value) => values.Add (value);

        public void Clear () => values.Clear ();

        IEnumerator IEnumerable.GetEnumerator () => values.GetEnumerator ();

        public IEnumerator<Value> GetEnumerator ()
        {
            return values.GetEnumerator ();
        }

        public void Remove (int index)
        {
            if (index >= 0 && index < values.Count) {
                values.RemoveAt (index);
            }
        }

        public override string ToString ()
        {
            StringBuilder stringBuilder = new StringBuilder ();
            stringBuilder.Append ('[');
            foreach (Value current in values) {
                stringBuilder.Append (current.ToString ());
                stringBuilder.Append (',');
            }
            if (values.Count > 0) {
                stringBuilder.Remove (stringBuilder.Length - 1, 1);
            }
            stringBuilder.Append (']');
            return stringBuilder.ToString ();
        }

        public static Array operator + (Array lhs, Array rhs) {
            Array array = new Array (lhs);
            foreach (Value current in rhs.values) {
                array.Add (current);
            }
            return array;
        }
    }
}

