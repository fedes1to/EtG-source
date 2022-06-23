using UnityEngine;

namespace FullInspector
{
	public interface ISerializationOperator
	{
		Object RetrieveObjectReference(int storageId);

		int StoreObjectReference(Object obj);
	}
}
