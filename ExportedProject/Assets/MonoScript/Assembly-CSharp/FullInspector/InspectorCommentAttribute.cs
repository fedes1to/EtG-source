using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
	public class InspectorCommentAttribute : Attribute, IInspectorAttributeOrder
	{
		public string Comment;

		public CommentType Type;

		public double Order = 100.0;

		double IInspectorAttributeOrder.Order
		{
			get
			{
				return Order;
			}
		}

		public InspectorCommentAttribute(string comment)
			: this(fiSettings.DefaultCommentType, comment)
		{
		}

		public InspectorCommentAttribute(CommentType type, string comment)
		{
			Type = type;
			Comment = comment;
		}
	}
}
