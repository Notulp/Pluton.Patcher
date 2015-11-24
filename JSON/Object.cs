using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Pluton.Patcher.JSON
{
    public class Object : IEnumerable<KeyValuePair<string, Pluton.Patcher.JSON.Value>>, IEnumerable
    {
        
        private readonly IDictionary<string, Value> values = new Dictionary<string, Value>();


        public Value this[string key] {
            get {
                return GetValue(key);
            }
            set {
                values[key] = value;
            }
        }

        public Object() {}

        public Object(Object other)
        {
            values = new Dictionary<string, Value>();
            if (other != null) {
                foreach (KeyValuePair<string, Value> current in other.values) {
                    values[current.Key] = new Value(current.Value);
                }
            }
        }

        private static Object Fail(char expected, int position)
        {
            return Object.Fail(new string(expected, 1), position);
        }

        private static Object Fail(string expected, int position)
        {
            Console.WriteLine("Fail: " + expected + " @ " + position);
            return null;
        }

        public static Object Parse(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                return null;
            }
            Value value = null;
            List<string> list = new List<string>();
            Object.ParsingState parsingState = Object.ParsingState.Object;
            for (int i = 0; i < jsonString.Length; i++)
            {
                i = Object.SkipWhitespace(jsonString, i);
                switch (parsingState)
                {
                case Object.ParsingState.Object:
                    {
                        if (jsonString[i] != '{')
                        {
                            return Object.Fail('{', i);
                        }
                        Value value2 = new Object();
                        if (value != null)
                        {
                            value2.Parent = value;
                        }
                        value = value2;
                        parsingState = Object.ParsingState.Key;
                        break;
                    }
                case Object.ParsingState.Array:
                    {
                        if (jsonString[i] != '[')
                        {
                            return Object.Fail('[', i);
                        }
                        Value value3 = new Array();
                        if (value != null)
                        {
                            value3.Parent = value;
                        }
                        value = value3;
                        parsingState = Object.ParsingState.Value;
                        break;
                    }
                case Object.ParsingState.EndObject:
                    {
                        if (jsonString[i] != '}')
                        {
                            return Object.Fail('}', i);
                        }
                        if (value.Parent == null)
                        {
                            return value.Obj;
                        }
                        ValueType type = value.Parent.Type;
                        if (type != ValueType.Object)
                        {
                            if (type != ValueType.Array)
                            {
                                return Object.Fail("valid object", i);
                            }
                            value.Parent.Array.Add(new Value(value.Obj));
                        }
                        else
                        {
                            value.Parent.Obj.values[list.Pop<string>()] = new Value(value.Obj);
                        }
                        value = value.Parent;
                        parsingState = Object.ParsingState.ValueSeparator;
                        break;
                    }
                case Object.ParsingState.EndArray:
                    {
                        if (jsonString[i] != ']')
                        {
                            return Object.Fail(']', i);
                        }
                        if (value.Parent == null)
                        {
                            return value.Obj;
                        }
                        ValueType type = value.Parent.Type;
                        if (type != ValueType.Object)
                        {
                            if (type != ValueType.Array)
                            {
                                return Object.Fail("valid object", i);
                            }
                            value.Parent.Array.Add(new Value(value.Array));
                        }
                        else
                        {
                            value.Parent.Obj.values[list.Pop<string>()] = new Value(value.Array);
                        }
                        value = value.Parent;
                        parsingState = Object.ParsingState.ValueSeparator;
                        break;
                    }
                case Object.ParsingState.Key:
                    if (jsonString[i] == '}')
                    {
                        i--;
                        parsingState = Object.ParsingState.EndObject;
                    }
                    else
                    {
                        string text = Object.ParseString(jsonString, ref i);
                        if (text == null)
                        {
                            return Object.Fail("key string", i);
                        }
                        list.Add(text);
                        parsingState = Object.ParsingState.KeyValueSeparator;
                    }
                    break;
                case Object.ParsingState.Value:
                    {
                        char c = jsonString[i];
                        if (c == '"')
                        {
                            parsingState = Object.ParsingState.String;
                        }
                        else
                        {
                            if (!char.IsDigit(c) && c != '-')
                            {
                                char c2 = c;
                                switch (c2)
                                {
                                case '[':
                                    parsingState = Object.ParsingState.Array;
                                    goto IL_2E3;
                                case '\\':
                                    goto IL_26C;
                                case ']':
                                    if (value.Type == ValueType.Array)
                                    {
                                        parsingState = Object.ParsingState.EndArray;
                                        goto IL_2E3;
                                    }
                                    return Object.Fail("valid array", i);
                                }
                                IL_26C:
                                if (c2 != 'f')
                                {
                                    if (c2 == 'n')
                                    {
                                        parsingState = Object.ParsingState.Null;
                                        goto IL_2E3;
                                    }
                                    if (c2 != 't')
                                    {
                                        if (c2 != '{')
                                        {
                                            return Object.Fail("beginning of value", i);
                                        }
                                        parsingState = Object.ParsingState.Object;
                                        goto IL_2E3;
                                    }
                                }
                                parsingState = Object.ParsingState.Boolean;
                                goto IL_2E3;
                            }
                            parsingState = Object.ParsingState.Number;
                        }
                        IL_2E3:
                        i--;
                        break;
                    }
                case Object.ParsingState.KeyValueSeparator:
                    if (jsonString[i] != ':')
                    {
                        return Object.Fail(':', i);
                    }
                    parsingState = Object.ParsingState.Value;
                    break;
                case Object.ParsingState.ValueSeparator:
                    {
                        char c2 = jsonString[i];
                        if (c2 != ',')
                        {
                            if (c2 != ']')
                            {
                                if (c2 != '}')
                                {
                                    return Object.Fail(", } ]", i);
                                }
                                parsingState = Object.ParsingState.EndObject;
                                i--;
                            }
                            else
                            {
                                parsingState = Object.ParsingState.EndArray;
                                i--;
                            }
                        }
                        else
                        {
                            parsingState = ((value.Type != ValueType.Object) ? Object.ParsingState.Value : Object.ParsingState.Key);
                        }
                        break;
                    }
                case Object.ParsingState.String:
                    {
                        string text2 = Object.ParseString(jsonString, ref i);
                        if (text2 == null)
                        {
                            return Object.Fail("string value", i);
                        }
                        ValueType type = value.Type;
                        if (type != ValueType.Object)
                        {
                            if (type != ValueType.Array)
                            {
                                return null;
                            }
                            value.Array.Add(text2);
                        }
                        else
                        {
                            value.Obj.values[list.Pop<string>()] = new Value(text2);
                        }
                        parsingState = Object.ParsingState.ValueSeparator;
                        break;
                    }
                case Object.ParsingState.Number:
                    {
                        double num = Object.ParseNumber(jsonString, ref i);
                        if (double.IsNaN(num))
                        {
                            return Object.Fail("valid number", i);
                        }
                        ValueType type = value.Type;
                        if (type != ValueType.Object)
                        {
                            if (type != ValueType.Array)
                            {
                                return null;
                            }
                            value.Array.Add(num);
                        }
                        else
                        {
                            value.Obj.values[list.Pop<string>()] = new Value(num);
                        }
                        parsingState = Object.ParsingState.ValueSeparator;
                        break;
                    }
                case Object.ParsingState.Boolean:
                    if (jsonString[i] == 't')
                    {
                        if (jsonString.Length < i + 4 || jsonString[i + 1] != 'r' || jsonString[i + 2] != 'u' || jsonString[i + 3] != 'e')
                        {
                            return Object.Fail("true", i);
                        }
                        ValueType type = value.Type;
                        if (type != ValueType.Object)
                        {
                            if (type != ValueType.Array)
                            {
                                return null;
                            }
                            value.Array.Add(new Value(true));
                        }
                        else
                        {
                            value.Obj.values[list.Pop<string>()] = new Value(true);
                        }
                        i += 3;
                    }
                    else
                    {
                        if (jsonString.Length < i + 5 || jsonString[i + 1] != 'a' || jsonString[i + 2] != 'l' || jsonString[i + 3] != 's' || jsonString[i + 4] != 'e')
                        {
                            return Object.Fail("false", i);
                        }
                        ValueType type = value.Type;
                        if (type != ValueType.Object)
                        {
                            if (type != ValueType.Array)
                            {
                                return null;
                            }
                            value.Array.Add(new Value(false));
                        }
                        else
                        {
                            value.Obj.values[list.Pop<string>()] = new Value(false);
                        }
                        i += 4;
                    }
                    parsingState = Object.ParsingState.ValueSeparator;
                    break;
                case Object.ParsingState.Null:
                    if (jsonString[i] == 'n')
                    {
                        if (jsonString.Length < i + 4 || jsonString[i + 1] != 'u' || jsonString[i + 2] != 'l' || jsonString[i + 3] != 'l')
                        {
                            return Object.Fail("null", i);
                        }
                        ValueType type = value.Type;
                        if (type != ValueType.Object)
                        {
                            if (type != ValueType.Array)
                            {
                                return null;
                            }
                            value.Array.Add(new Value(ValueType.Null));
                        }
                        else
                        {
                            value.Obj.values[list.Pop<string>()] = new Value(ValueType.Null);
                        }
                        i += 3;
                    }
                    parsingState = Object.ParsingState.ValueSeparator;
                    break;
                }
            }
            return null;
        }

        private static double ParseNumber(string str, ref int startPosition)
        {
            if (startPosition >= str.Length || (!char.IsDigit(str[startPosition]) && str[startPosition] != '-')) {
                return Double.NaN;
            }
            int num = startPosition + 1;
            while (num < str.Length && str[num] != ',' && str[num] != ']' && str[num] != '}') {
                num++;
            }
            double result;
            if (!double.TryParse(str.Substring(startPosition, num - startPosition), NumberStyles.Float, CultureInfo.InvariantCulture, out result)) {
                return Double.NaN;
            }
            startPosition = num - 1;
            return result;
        }

        private static string ParseString(string str, ref int startPosition)
        {
            if (str[startPosition] != '"' || startPosition + 1 >= str.Length) {
                Object.Fail('"', startPosition);
                return null;
            }
            int num = str.IndexOf('"', startPosition + 1);
            if (num <= startPosition) {
                Object.Fail('"', startPosition + 1);
                return null;
            }
            while (str[num - 1] == '\'') {
                num = str.IndexOf('"', num + 1);
                if (num <= startPosition) {
                    Object.Fail('"', startPosition + 1);
                    return null;
                }
            }
            string result = string.Empty;
            if (num > startPosition + 1) {
                result = str.Substring(startPosition + 1, num - startPosition - 1);
            }
            startPosition = num;
            return result;
        }

        private static int SkipWhitespace(string str, int pos)
        {
            while (pos < str.Length && char.IsWhiteSpace(str[pos])) {
                pos++;
            }
            return pos;
        }

        public void Add(KeyValuePair<string, Value> pair)
        {
            values[pair.Key] = pair.Value;
        }

        public void Add(string key, Value value)
        {
            values[key] = value;
        }

        public void Clear()
        {
            values.Clear();
        }

        public bool ContainsKey(string key)
        {
            return values.ContainsKey(key);
        }

        public Array GetArray(string key)
        {
            Value value = GetValue(key);
            if (value == null) {
                return new Array();
            }
            return value.Array;
        }

        public bool GetBoolean(string key, bool bDefault = false)
        {
            Value value = GetValue(key);
            if (value == null) {
                return bDefault;
            }
            if (value.Type == ValueType.Boolean) {
                return value.Boolean;
            }
            if (value.Type == ValueType.Number) {
                return value.Number != 0;
            }
            return bDefault;
        }

        public IEnumerator<KeyValuePair<string, Value>> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        public float GetFloat(string key, float iDefault = 0)
        {
            return (float)GetNumber(key, (double)iDefault);
        }

        public int GetInt(string key, int iDefault = 0)
        {
            return (int)GetNumber(key, (double)iDefault);
        }

        public double GetNumber(string key, double iDefault = 0)
        {
            Value value = GetValue(key);
            if (value == null) {
                return iDefault;
            }
            if (value.Type == ValueType.Number) {
                return value.Number;
            }
            if (value.Type == ValueType.String) {
                double result = iDefault;
                if (double.TryParse(value.Str, out result)) {
                    return result;
                }
            }
            return iDefault;
        }

        public Object GetObject(string key)
        {
            Value value = GetValue(key);
            if (value == null) {
                return new Object();
            }
            return value.Obj;
        }

        public string GetString(string key, string strDEFAULT = "")
        {
            Value value = GetValue(key);
            if (value == null) {
                return strDEFAULT;
            }
            string str = value.Str;
            return str.Replace("\\/", "/");
        }

        public Value GetValue(string key)
        {
            Value result;
            values.TryGetValue(key, out result);
            return result;
        }

        public void Remove(string key)
        {
            if (values.ContainsKey(key)) {
                values.Remove(key);
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append('{');
            foreach (KeyValuePair<string, Value> current in values) {
                stringBuilder.Append("\"" + current.Key + "\"");
                stringBuilder.Append(':');
                stringBuilder.Append(current.Value.ToString());
                stringBuilder.Append(',');
            }
            if (values.Count > 0) {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            stringBuilder.Append('}');
            return stringBuilder.ToString();
        }

        private enum ParsingState
        {
            Object,
            Array,
            EndObject,
            EndArray,
            Key,
            Value,
            KeyValueSeparator,
            ValueSeparator,
            String,
            Number,
            Boolean,
            Null
        }
    }
}

