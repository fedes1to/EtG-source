using UnityEngine;

namespace FullInspector.LayoutToolkit
{
	public class fiCenterVertical : fiLayout
	{
		private string _id;

		private fiLayout _centered;

		public override float Height
		{
			get
			{
				return _centered.Height;
			}
		}

		public fiCenterVertical(string id, fiLayout centered)
		{
			_id = id;
			_centered = centered;
		}

		public fiCenterVertical(fiLayout centered)
			: this(string.Empty, centered)
		{
		}

		public override bool RespondsTo(string sectionId)
		{
			return _id == sectionId || _centered.RespondsTo(sectionId);
		}

		public override Rect GetSectionRect(string sectionId, Rect initial)
		{
			float num = initial.height - _centered.Height;
			initial.y += num / 2f;
			initial.height -= num;
			initial = _centered.GetSectionRect(sectionId, initial);
			return initial;
		}
	}
}
