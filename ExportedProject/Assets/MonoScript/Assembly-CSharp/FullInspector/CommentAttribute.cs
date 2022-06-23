using System;

namespace FullInspector
{
	[Obsolete("Use [InspectorComment] instead of [Comment]")]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
	public class CommentAttribute : Attribute, IInspectorAttributeOrder
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

		public CommentAttribute(string comment)
			: this(CommentType.Info, comment)
		{
		}

		public CommentAttribute(CommentType type, string comment)
		{
			Type = type;
			Comment = comment;
		}
	}
}
