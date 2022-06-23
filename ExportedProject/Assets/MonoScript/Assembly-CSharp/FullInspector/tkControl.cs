using System;
using System.Collections.Generic;
using System.Reflection;
using FullSerializer.Internal;
using UnityEngine;

namespace FullInspector
{
	public abstract class tkControl<T, TContext> : tkIControl
	{
		private int _uniqueId;

		private List<tkStyle<T, TContext>> _styles;

		public Type ContextType
		{
			get
			{
				return typeof(TContext);
			}
		}

		public tkStyle<T, TContext> Style
		{
			set
			{
				Styles = new List<tkStyle<T, TContext>> { value };
			}
		}

		public List<tkStyle<T, TContext>> Styles
		{
			get
			{
				if (_styles == null)
				{
					_styles = new List<tkStyle<T, TContext>>();
				}
				return _styles;
			}
			set
			{
				_styles = value;
			}
		}

		protected virtual IEnumerable<tkIControl> NonMemberChildControls
		{
			get
			{
				yield break;
			}
		}

		protected fiGraphMetadata GetInstanceMetadata(fiGraphMetadata metadata)
		{
			return metadata.Enter(_uniqueId).Metadata;
		}

		protected abstract T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata);

		protected abstract float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata);

		public virtual bool ShouldShow(T obj, TContext context, fiGraphMetadata metadata)
		{
			return true;
		}

		public T Edit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
		{
			if (Styles == null)
			{
				return DoEdit(rect, obj, context, metadata);
			}
			for (int i = 0; i < Styles.Count; i++)
			{
				Styles[i].Activate(obj, context);
			}
			T result = DoEdit(rect, obj, context, metadata);
			for (int j = 0; j < Styles.Count; j++)
			{
				Styles[j].Deactivate(obj, context);
			}
			return result;
		}

		public object Edit(Rect rect, object obj, object context, fiGraphMetadata metadata)
		{
			return Edit(rect, (T)obj, (TContext)context, metadata);
		}

		public float GetHeight(T obj, TContext context, fiGraphMetadata metadata)
		{
			if (Styles == null)
			{
				return DoGetHeight(obj, context, metadata);
			}
			for (int i = 0; i < Styles.Count; i++)
			{
				Styles[i].Activate(obj, context);
			}
			float result = DoGetHeight(obj, context, metadata);
			for (int j = 0; j < Styles.Count; j++)
			{
				Styles[j].Deactivate(obj, context);
			}
			return result;
		}

		public float GetHeight(object obj, object context, fiGraphMetadata metadata)
		{
			return GetHeight((T)obj, (TContext)context, metadata);
		}

		void tkIControl.InitializeId(ref int nextId)
		{
			_uniqueId = nextId++;
			foreach (tkIControl nonMemberChildControl in NonMemberChildControls)
			{
				nonMemberChildControl.InitializeId(ref nextId);
			}
			for (Type type = GetType(); type != null; type = type.Resolve().BaseType)
			{
				MemberInfo[] declaredMembers = type.GetDeclaredMembers();
				foreach (MemberInfo member in declaredMembers)
				{
					Type memberType;
					if (!TryGetMemberType(member, out memberType))
					{
						continue;
					}
					if (typeof(tkIControl).IsAssignableFrom(memberType))
					{
						tkIControl value;
						if (TryReadValue<tkIControl>(member, this, out value) && value != null)
						{
							value.InitializeId(ref nextId);
						}
					}
					else
					{
						IEnumerable<tkIControl> value2;
						if (!typeof(IEnumerable<tkIControl>).IsAssignableFrom(memberType) || !TryReadValue<IEnumerable<tkIControl>>(member, this, out value2) || value2 == null)
						{
							continue;
						}
						foreach (tkIControl item in value2)
						{
							item.InitializeId(ref nextId);
						}
					}
				}
			}
		}

		private static bool TryReadValue<TValue>(MemberInfo member, object context, out TValue value)
		{
			if (member is FieldInfo)
			{
				value = (TValue)((FieldInfo)member).GetValue(context);
				return true;
			}
			if (member is PropertyInfo)
			{
				value = (TValue)((PropertyInfo)member).GetValue(context, null);
				return true;
			}
			value = default(TValue);
			return false;
		}

		private static bool TryGetMemberType(MemberInfo member, out Type memberType)
		{
			if (member is FieldInfo)
			{
				memberType = ((FieldInfo)member).FieldType;
				return true;
			}
			if (member is PropertyInfo)
			{
				memberType = ((PropertyInfo)member).PropertyType;
				return true;
			}
			memberType = null;
			return false;
		}
	}
}
