using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullSerializer.Internal.DirectConverters
{
	public class Keyframe_DirectConverter : fsDirectConverter<Keyframe>
	{
		protected override fsResult DoSerialize(Keyframe model, Dictionary<string, fsData> serialized)
		{
			fsResult success = fsResult.Success;
			success += SerializeMember(serialized, "time", model.time);
			success += SerializeMember(serialized, "value", model.value);
			success += SerializeMember(serialized, "tangentMode", model.tangentMode);
			success += SerializeMember(serialized, "inTangent", model.inTangent);
			return success + SerializeMember(serialized, "outTangent", model.outTangent);
		}

		protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref Keyframe model)
		{
			fsResult success = fsResult.Success;
			float value = model.time;
			success += DeserializeMember<float>(data, "time", out value);
			model.time = value;
			float value2 = model.value;
			success += DeserializeMember<float>(data, "value", out value2);
			model.value = value2;
			int value3 = model.tangentMode;
			success += DeserializeMember<int>(data, "tangentMode", out value3);
			model.tangentMode = value3;
			float value4 = model.inTangent;
			success += DeserializeMember<float>(data, "inTangent", out value4);
			model.inTangent = value4;
			float value5 = model.outTangent;
			success += DeserializeMember<float>(data, "outTangent", out value5);
			model.outTangent = value5;
			return success;
		}

		public override object CreateInstance(fsData data, Type storageType)
		{
			return default(Keyframe);
		}
	}
}
