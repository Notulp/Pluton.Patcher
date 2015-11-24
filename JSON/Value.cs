﻿using System;

namespace Pluton.Patcher.JSON
{
    public class Value
    {
        public Array Array {
            get;
            set;
        }

        public bool Boolean {
            get;
            set;
        }

        public double Number {
            get;
            set;
        }

        public Object Obj {
            get;
            set;
        }

        public Value Parent {
            get;
            set;
        }

        public string Str {
            get;
            set;
        }

        public ValueType Type {
            get;
            private set;
        }

        public Value (Value value)
        {
            Type = value.Type;
            switch (this.Type) {
            case ValueType.String:
                Str = value.Str;
                break;
            case ValueType.Number:
                Number = value.Number;
                break;
            case ValueType.Object:
                if (value.Obj != null) {
                    Obj = new Object (value.Obj);
                }
                break;
            case ValueType.Array:
                Array = new Array (value.Array);
                break;
            case ValueType.Boolean:
                Boolean = value.Boolean;
                break;
            }
        }

        public Value (bool boolean)
        {
            Type = ValueType.Boolean;
            Boolean = boolean;
        }

        public Value (Array array)
        {
            Type = ValueType.Array;
            Array = array;
        }

        public Value (double number)
        {
            Type = ValueType.Number;
            Number = number;
        }

        public Value (string str)
        {
            Type = ValueType.String;
            Str = str;
        }

        public Value (ValueType type)
        {
            Type = type;
        }

        public Value (Object obj)
        {
            if (obj == null) {
                Type = ValueType.Null;
            }
            else {
                Type = ValueType.Object;
                Obj = obj;
            }
        }

        public override string ToString ()
        {
            switch (Type) {
            case ValueType.String:
                return "\"" + Str + "\"";
            case ValueType.Number:
                return Number.ToString ();
            case ValueType.Object:
                return Obj.ToString ();
            case ValueType.Array:
                return Array.ToString ();
            case ValueType.Boolean:
                return (!Boolean) ? "false" : "true";
            case ValueType.Null:
                return "null";
            default:
                return "null";
            }
        }

        public static implicit operator Value (Array array) {
            return new Value (array);
        }

        public static implicit operator Value (string str) {
            return new Value (str);
        }

        public static implicit operator Value (double number) {
            return new Value (number);
        }

        public static implicit operator Value (Object obj) {
            return new Value (obj);
        }

        public static implicit operator Value (bool boolean) {
            return new Value (boolean);
        }
    }
}
