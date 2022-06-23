using System;
using UnityEngine;

namespace FullInspector
{
	public interface tkIControl
	{
		Type ContextType { get; }

		object Edit(Rect rect, object obj, object context, fiGraphMetadata metadata);

		float GetHeight(object obj, object context, fiGraphMetadata metadata);

		void InitializeId(ref int nextId);
	}
}
