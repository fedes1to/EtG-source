using System;

namespace FullSerializer.Internal
{
	public class fsPrimitiveConverter : fsConverter
	{
		public override bool CanProcess(Type type)
		{
			return type.Resolve().IsPrimitive || type == typeof(string) || type == typeof(decimal);
		}

		public override bool RequestCycleSupport(Type storageType)
		{
			return false;
		}

		public override bool RequestInheritanceSupport(Type storageType)
		{
			return false;
		}

		private static bool UseBool(Type type)
		{
			return type == typeof(bool);
		}

		private static bool UseInt64(Type type)
		{
			return type == typeof(sbyte) || type == typeof(byte) || type == typeof(short) || type == typeof(ushort) || type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong);
		}

		private static bool UseDouble(Type type)
		{
			return type == typeof(float) || type == typeof(double) || type == typeof(decimal);
		}

		private static bool UseString(Type type)
		{
			return type == typeof(string) || type == typeof(char);
		}

		public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType)
		{
			Type type = instance.GetType();
			if (fsConfig.Serialize64BitIntegerAsString && (type == typeof(long) || type == typeof(ulong)))
			{
				serialized = new fsData((string)Convert.ChangeType(instance, typeof(string)));
				return fsResult.Success;
			}
			if (UseBool(type))
			{
				serialized = new fsData((bool)instance);
				return fsResult.Success;
			}
			if (UseInt64(type))
			{
				serialized = new fsData((long)Convert.ChangeType(instance, typeof(long)));
				return fsResult.Success;
			}
			if (UseDouble(type))
			{
				serialized = new fsData((double)Convert.ChangeType(instance, typeof(double)));
				return fsResult.Success;
			}
			if (UseString(type))
			{
				serialized = new fsData((string)Convert.ChangeType(instance, typeof(string)));
				return fsResult.Success;
			}
			serialized = null;
			return fsResult.Fail("Unhandled primitive type " + instance.GetType());
		}

		public override fsResult TryDeserialize(fsData storage, ref object instance, Type storageType)
		{
			fsResult success = fsResult.Success;
			if (UseBool(storageType))
			{
				if ((success += CheckType(storage, fsDataType.Boolean)).Succeeded)
				{
					instance = storage.AsBool;
				}
				return success;
			}
			if (UseDouble(storageType) || UseInt64(storageType))
			{
				if (storage.IsDouble)
				{
					instance = Convert.ChangeType(storage.AsDouble, storageType);
				}
				else if (storage.IsInt64)
				{
					instance = Convert.ChangeType(storage.AsInt64, storageType);
				}
				else
				{
					if (!fsConfig.Serialize64BitIntegerAsString || !storage.IsString || (storageType != typeof(long) && storageType != typeof(ulong)))
					{
						return fsResult.Fail(string.Concat(GetType().Name, " expected number but got ", storage.Type, " in ", storage));
					}
					instance = Convert.ChangeType(storage.AsString, storageType);
				}
				return fsResult.Success;
			}
			if (UseString(storageType))
			{
				if ((success += CheckType(storage, fsDataType.String)).Succeeded)
				{
					instance = storage.AsString;
				}
				return success;
			}
			return fsResult.Fail(GetType().Name + ": Bad data; expected bool, number, string, but got " + storage);
		}
	}
}
