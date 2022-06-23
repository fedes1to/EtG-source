using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.LayoutToolkit
{
	public class fiHorizontalLayout : fiLayout, IEnumerable
	{
		private struct SectionItem
		{
			public string Id;

			public float MinWidth;

			public fiExpandMode ExpandMode;

			public fiLayout Rule;
		}

		private List<SectionItem> _items = new List<SectionItem>();

		private fiLayout _defaultRule = new fiVerticalLayout();

		private int ExpandCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < _items.Count; i++)
				{
					if (_items[i].ExpandMode == fiExpandMode.Expand)
					{
						num++;
					}
				}
				if (num == 0)
				{
					num = 1;
				}
				return num;
			}
		}

		private float MinimumWidth
		{
			get
			{
				float num = 0f;
				for (int i = 0; i < _items.Count; i++)
				{
					num += _items[i].MinWidth;
				}
				return num;
			}
		}

		public override float Height
		{
			get
			{
				float num = 0f;
				for (int i = 0; i < _items.Count; i++)
				{
					num = Math.Max(num, _items[i].Rule.Height);
				}
				return num;
			}
		}

		public fiHorizontalLayout()
		{
		}

		public fiHorizontalLayout(fiLayout defaultRule)
		{
			_defaultRule = defaultRule;
		}

		public void Add(fiLayout rule)
		{
			ActualAdd(string.Empty, 0f, fiExpandMode.Expand, rule);
		}

		public void Add(float width)
		{
			ActualAdd(string.Empty, width, fiExpandMode.Fixed, _defaultRule);
		}

		public void Add(string id)
		{
			ActualAdd(id, 0f, fiExpandMode.Expand, _defaultRule);
		}

		public void Add(string id, float width)
		{
			ActualAdd(id, width, fiExpandMode.Fixed, _defaultRule);
		}

		public void Add(string id, fiLayout rule)
		{
			ActualAdd(id, 0f, fiExpandMode.Expand, rule);
		}

		public void Add(float width, fiLayout rule)
		{
			ActualAdd(string.Empty, width, fiExpandMode.Fixed, rule);
		}

		public void Add(string id, float width, fiLayout rule)
		{
			ActualAdd(id, width, fiExpandMode.Fixed, rule);
		}

		private void ActualAdd(string id, float width, fiExpandMode expandMode, fiLayout rule)
		{
			_items.Add(new SectionItem
			{
				Id = id,
				MinWidth = width,
				ExpandMode = expandMode,
				Rule = rule
			});
		}

		public override Rect GetSectionRect(string sectionId, Rect initial)
		{
			float num = initial.width - MinimumWidth;
			if (num < 0f)
			{
				num = 0f;
			}
			float num2 = 1f / (float)ExpandCount;
			for (int i = 0; i < _items.Count; i++)
			{
				SectionItem sectionItem = _items[i];
				float num3 = sectionItem.MinWidth;
				if (sectionItem.ExpandMode == fiExpandMode.Expand)
				{
					num3 += num * num2;
				}
				if (sectionItem.Id == sectionId || sectionItem.Rule.RespondsTo(sectionId))
				{
					initial.width = num3;
					initial = sectionItem.Rule.GetSectionRect(sectionId, initial);
					break;
				}
				initial.x += num3;
			}
			return initial;
		}

		public override bool RespondsTo(string sectionId)
		{
			for (int i = 0; i < _items.Count; i++)
			{
				if (_items[i].Id == sectionId || _items[i].Rule.RespondsTo(sectionId))
				{
					return true;
				}
			}
			return false;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotSupportedException();
		}
	}
}
