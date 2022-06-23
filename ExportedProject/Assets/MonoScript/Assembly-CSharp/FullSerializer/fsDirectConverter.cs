using System;
using System.Collections.Generic;

namespace FullSerializer
{
	public abstract class fsDirectConverter : fsBaseConverter
	{
		public abstract Type ModelType { get; }
	}
	public abstract class fsDirectConverter<TModel> : fsDirectConverter
	{
		public override Type ModelType
		{
			get
			{
				return typeof(TModel);
			}
		}

		public sealed override fsResult TrySerialize(object instance, out fsData serialized, Type storageType)
		{
			Dictionary<string, fsData> dictionary = new Dictionary<string, fsData>();
			fsResult result = DoSerialize((TModel)instance, dictionary);
			serialized = new fsData(dictionary);
			return result;
		}

		public sealed override fsResult TryDeserialize(fsData data, ref object instance, Type storageType)
		{
			fsResult success = fsResult.Success;
			if ((success += CheckType(data, fsDataType.Object)).Failed)
			{
				return success;
			}
			TModel model = (TModel)instance;
			success += DoDeserialize(data.AsDictionary, ref model);
			instance = model;
			return success;
		}

		protected abstract fsResult DoSerialize(TModel model, Dictionary<string, fsData> serialized);

		protected abstract fsResult DoDeserialize(Dictionary<string, fsData> data, ref TModel model);
	}
}
