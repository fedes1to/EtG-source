using System;
using UnityEngine;

namespace FullSerializer
{
	public static class fsConfig
	{
		public static Type[] SerializeAttributes = new Type[2]
		{
			typeof(SerializeField),
			typeof(fsPropertyAttribute)
		};

		public static Type[] IgnoreSerializeAttributes = new Type[2]
		{
			typeof(NonSerializedAttribute),
			typeof(fsIgnoreAttribute)
		};

		private static fsMemberSerialization _defaultMemberSerialization = fsMemberSerialization.Default;

		public static bool SerializeNonAutoProperties = false;

		public static bool SerializeNonPublicSetProperties = true;

		public static bool IsCaseSensitive = true;

		public static string CustomDateTimeFormatString = null;

		public static bool Serialize64BitIntegerAsString = false;

		public static bool SerializeEnumsAsInteger = false;

		public static fsMemberSerialization DefaultMemberSerialization
		{
			get
			{
				return _defaultMemberSerialization;
			}
			set
			{
				_defaultMemberSerialization = value;
				fsMetaType.ClearCache();
			}
		}
	}
}
