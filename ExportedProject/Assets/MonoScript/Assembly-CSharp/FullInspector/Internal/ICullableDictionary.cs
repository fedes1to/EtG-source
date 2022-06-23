using System.Collections.Generic;

namespace FullInspector.Internal
{
	public interface ICullableDictionary<TKey, TValue>
	{
		TValue this[TKey key] { get; set; }

		IEnumerable<KeyValuePair<TKey, TValue>> Items { get; }

		bool IsEmpty { get; }

		bool TryGetValue(TKey key, out TValue value);

		void BeginCullZone();

		void EndCullZone();
	}
}
