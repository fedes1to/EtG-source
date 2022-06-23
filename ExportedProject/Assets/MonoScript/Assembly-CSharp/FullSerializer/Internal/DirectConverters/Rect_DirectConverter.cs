using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullSerializer.Internal.DirectConverters
{
	public class Rect_DirectConverter : fsDirectConverter<Rect>
	{
		protected override fsResult DoSerialize(Rect model, Dictionary<string, fsData> serialized)
		{
			fsResult success = fsResult.Success;
			success += SerializeMember(serialized, "xMin", model.xMin);
			success += SerializeMember(serialized, "yMin", model.yMin);
			success += SerializeMember(serialized, "xMax", model.xMax);
			return success + SerializeMember(serialized, "yMax", model.yMax);
		}

		protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref Rect model)
		{
			fsResult success = fsResult.Success;
			float value = model.xMin;
			success += DeserializeMember<float>(data, "xMin", out value);
			model.xMin = value;
			float value2 = model.yMin;
			success += DeserializeMember<float>(data, "yMin", out value2);
			model.yMin = value2;
			float value3 = model.xMax;
			success += DeserializeMember<float>(data, "xMax", out value3);
			model.xMax = value3;
			float value4 = model.yMax;
			success += DeserializeMember<float>(data, "yMax", out value4);
			model.yMax = value4;
			return success;
		}

		public override object CreateInstance(fsData data, Type storageType)
		{
			return default(Rect);
		}
	}
}
