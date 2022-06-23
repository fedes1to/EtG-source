using System;

namespace DaikonForge.Editor
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class InspectorAttribute : Attribute, IComparable<InspectorAttribute>
	{
		public string Group { get; set; }

		public int Order { get; set; }

		public string Label { get; set; }

		public string BackingField { get; set; }

		public string Tooltip { get; set; }

		public InspectorAttribute(string group)
		{
			Group = group;
			Order = int.MaxValue;
		}

		public InspectorAttribute(string category, int order)
		{
			Group = category;
			Order = order;
		}

		public override string ToString()
		{
			return string.Format("{0} {1} - {2}", Group, Order, Label ?? BackingField ?? "(Unknown)");
		}

		public int CompareTo(InspectorAttribute other)
		{
			if (!string.Equals(Group, other.Group))
			{
				return Group.CompareTo(other.Group);
			}
			if (Order != other.Order)
			{
				return Order.CompareTo(other.Order);
			}
			string text = Label ?? BackingField;
			string text2 = other.Label ?? other.BackingField;
			if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2))
			{
				return text.CompareTo(text2);
			}
			return 0;
		}
	}
}
