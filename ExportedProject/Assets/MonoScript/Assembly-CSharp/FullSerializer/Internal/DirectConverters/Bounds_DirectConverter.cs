using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullSerializer.Internal.DirectConverters
{
	public class Bounds_DirectConverter : fsDirectConverter<Bounds>
	{
		protected override fsResult DoSerialize(Bounds model, Dictionary<string, fsData> serialized)
		{
			fsResult success = fsResult.Success;
			success += SerializeMember(serialized, "center", model.center);
			return success + SerializeMember(serialized, "size", model.size);
		}

		protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref Bounds model)
		{
			fsResult success = fsResult.Success;
			Vector3 value = model.center;
			success += DeserializeMember<Vector3>(data, "center", out value);
			model.center = value;
			Vector3 value2 = model.size;
			success += DeserializeMember<Vector3>(data, "size", out value2);
			model.size = value2;
			return success;
		}

		public override object CreateInstance(fsData data, Type storageType)
		{
			return default(Bounds);
		}
	}
}
