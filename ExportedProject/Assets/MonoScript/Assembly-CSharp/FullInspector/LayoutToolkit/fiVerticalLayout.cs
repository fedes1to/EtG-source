using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.LayoutToolkit
{
	public class fiVerticalLayout : fiLayout, IEnumerable
	{
		private struct SectionItem
		{
			public string Id;

			public fiLayout Rule;
		}

		private List<SectionItem> _items = new List<SectionItem>();

		public override float Height
		{
			get
			{
				float num = 0f;
				for (int i = 0; i < _items.Count; i++)
				{
					num += _items[i].Rule.Height;
				}
				return num;
			}
		}

		public void Add(fiLayout rule)
		{
			Add(string.Empty, rule);
		}

		public void Add(string sectionId, fiLayout rule)
		{
			_items.Add(new SectionItem
			{
				Id = sectionId,
				Rule = rule
			});
		}

		public void Add(string sectionId, float height)
		{
			Add(sectionId, new fiLayoutHeight(sectionId, height));
		}

		public void Add(float height)
		{
			Add(string.Empty, height);
		}

		public override Rect GetSectionRect(string sectionId, Rect initial)
		{
			for (int i = 0; i < _items.Count; i++)
			{
				SectionItem sectionItem = _items[i];
				if (sectionItem.Id == sectionId || sectionItem.Rule.RespondsTo(sectionId))
				{
					if (sectionItem.Rule.RespondsTo(sectionId))
					{
						initial = sectionItem.Rule.GetSectionRect(sectionId, initial);
					}
					else
					{
						initial.height = sectionItem.Rule.Height;
					}
					break;
				}
				initial.y += sectionItem.Rule.Height;
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
