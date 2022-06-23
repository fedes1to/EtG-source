using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FullSerializer
{
	public sealed class fsData
	{
		private object _value;

		public static readonly fsData True = new fsData(true);

		public static readonly fsData False = new fsData(false);

		public static readonly fsData Null = new fsData();

		public fsDataType Type
		{
			get
			{
				if (_value == null)
				{
					return fsDataType.Null;
				}
				if (_value is double)
				{
					return fsDataType.Double;
				}
				if (_value is long)
				{
					return fsDataType.Int64;
				}
				if (_value is bool)
				{
					return fsDataType.Boolean;
				}
				if (_value is string)
				{
					return fsDataType.String;
				}
				if (_value is Dictionary<string, fsData>)
				{
					return fsDataType.Object;
				}
				if (_value is List<fsData>)
				{
					return fsDataType.Array;
				}
				throw new InvalidOperationException("unknown JSON data type");
			}
		}

		public bool IsNull
		{
			get
			{
				return _value == null;
			}
		}

		public bool IsDouble
		{
			get
			{
				return _value is double;
			}
		}

		public bool IsInt64
		{
			get
			{
				return _value is long;
			}
		}

		public bool IsBool
		{
			get
			{
				return _value is bool;
			}
		}

		public bool IsString
		{
			get
			{
				return _value is string;
			}
		}

		public bool IsDictionary
		{
			get
			{
				return _value is Dictionary<string, fsData>;
			}
		}

		public bool IsList
		{
			get
			{
				return _value is List<fsData>;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public double AsDouble
		{
			get
			{
				return Cast<double>();
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public long AsInt64
		{
			get
			{
				return Cast<long>();
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public bool AsBool
		{
			get
			{
				return Cast<bool>();
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public string AsString
		{
			get
			{
				return Cast<string>();
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public Dictionary<string, fsData> AsDictionary
		{
			get
			{
				return Cast<Dictionary<string, fsData>>();
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public List<fsData> AsList
		{
			get
			{
				return Cast<List<fsData>>();
			}
		}

		public fsData()
		{
			_value = null;
		}

		public fsData(bool boolean)
		{
			_value = boolean;
		}

		public fsData(double f)
		{
			_value = f;
		}

		public fsData(long i)
		{
			_value = i;
		}

		public fsData(string str)
		{
			_value = str;
		}

		public fsData(Dictionary<string, fsData> dict)
		{
			_value = dict;
		}

		public fsData(List<fsData> list)
		{
			_value = list;
		}

		public static fsData CreateDictionary()
		{
			return new fsData(new Dictionary<string, fsData>((!fsConfig.IsCaseSensitive) ? StringComparer.CurrentCultureIgnoreCase : StringComparer.CurrentCulture));
		}

		public static fsData CreateList()
		{
			return new fsData(new List<fsData>());
		}

		public static fsData CreateList(int capacity)
		{
			return new fsData(new List<fsData>(capacity));
		}

		internal void BecomeDictionary()
		{
			_value = new Dictionary<string, fsData>();
		}

		internal fsData Clone()
		{
			fsData fsData2 = new fsData();
			fsData2._value = _value;
			return fsData2;
		}

		private T Cast<T>()
		{
			if (_value is T)
			{
				return (T)_value;
			}
			throw new InvalidCastException(string.Concat("Unable to cast <", this, "> (with type = ", _value.GetType(), ") to type ", typeof(T)));
		}

		public override string ToString()
		{
			return fsJsonPrinter.CompressedJson(this);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as fsData);
		}

		public bool Equals(fsData other)
		{
			if (other == null || Type != other.Type)
			{
				return false;
			}
			switch (Type)
			{
			case fsDataType.Null:
				return true;
			case fsDataType.Double:
				return AsDouble == other.AsDouble || Math.Abs(AsDouble - other.AsDouble) < double.Epsilon;
			case fsDataType.Int64:
				return AsInt64 == other.AsInt64;
			case fsDataType.Boolean:
				return AsBool == other.AsBool;
			case fsDataType.String:
				return AsString == other.AsString;
			case fsDataType.Array:
			{
				List<fsData> asList = AsList;
				List<fsData> asList2 = other.AsList;
				if (asList.Count != asList2.Count)
				{
					return false;
				}
				for (int i = 0; i < asList.Count; i++)
				{
					if (!asList[i].Equals(asList2[i]))
					{
						return false;
					}
				}
				return true;
			}
			case fsDataType.Object:
			{
				Dictionary<string, fsData> asDictionary = AsDictionary;
				Dictionary<string, fsData> asDictionary2 = other.AsDictionary;
				if (asDictionary.Count != asDictionary2.Count)
				{
					return false;
				}
				foreach (string key in asDictionary.Keys)
				{
					if (!asDictionary2.ContainsKey(key))
					{
						return false;
					}
					if (!asDictionary[key].Equals(asDictionary2[key]))
					{
						return false;
					}
				}
				return true;
			}
			default:
				throw new Exception("Unknown data type");
			}
		}

		public static bool operator ==(fsData a, fsData b)
		{
			if (object.ReferenceEquals(a, b))
			{
				return true;
			}
			if ((object)a == null || (object)b == null)
			{
				return false;
			}
			if (a.IsDouble && b.IsDouble)
			{
				return Math.Abs(a.AsDouble - b.AsDouble) < double.Epsilon;
			}
			return a.Equals(b);
		}

		public static bool operator !=(fsData a, fsData b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}
	}
}
