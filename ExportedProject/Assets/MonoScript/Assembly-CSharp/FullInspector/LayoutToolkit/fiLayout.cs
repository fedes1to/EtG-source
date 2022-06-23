using UnityEngine;

namespace FullInspector.LayoutToolkit
{
	public abstract class fiLayout
	{
		public abstract float Height { get; }

		public abstract bool RespondsTo(string id);

		public abstract Rect GetSectionRect(string id, Rect initial);
	}
}
