using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using FullInspector.Internal;
using FullSerializer;
using FullSerializer.Internal;
using UnityEngine;

namespace FullInspector
{
	public class tk<T, TContext>
	{
		public class Box : tkControl<T, TContext>
		{
			[ShowInInspector]
			private readonly tkControl<T, TContext> _control;

			public Box(tkControl<T, TContext> control)
			{
				_control = control;
			}

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				GUI.Box(rect, string.Empty);
				return _control.Edit(rect, obj, context, metadata);
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				return _control.GetHeight(obj, context, metadata);
			}
		}

		public class Button : tkControl<T, TContext>
		{
			[ShowInInspector]
			private readonly Value<fiGUIContent> _label;

			private readonly bool _enabled;

			private readonly Action<T, TContext> _onClick;

			public Button(string methodName)
			{
				InspectedMethod foundMethod = null;
				foreach (InspectedMethod method in InspectedType.Get(typeof(T)).GetMethods(InspectedMemberFilters.All))
				{
					if (method.Method.Name == methodName)
					{
						foundMethod = method;
					}
				}
				if (foundMethod != null)
				{
					_label = (fiGUIContent)foundMethod.DisplayLabel;
					_enabled = true;
					_onClick = delegate(T o, TContext c)
					{
						foundMethod.Invoke(o);
					};
				}
				else
				{
					Debug.LogError("Unable to find method " + methodName + " on " + typeof(T).CSharpName());
					_label = new fiGUIContent(methodName + " (unable to find on " + typeof(T).CSharpName() + ")");
					_enabled = false;
					_onClick = delegate
					{
					};
				}
			}

			public Button(Value<fiGUIContent> label, Action<T, TContext> onClick)
			{
				_enabled = true;
				_label = label;
				_onClick = onClick;
			}

			public Button(fiGUIContent label, Action<T, TContext> onClick)
				: this(tk<T, TContext>.Val(label), onClick)
			{
			}

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				fiLateBindings.EditorGUI.BeginDisabledGroup(!_enabled);
				if (GUI.Button(rect, _label.GetCurrentValue(obj, context)))
				{
					_onClick(obj, context);
				}
				fiLateBindings.EditorGUI.EndDisabledGroup();
				return obj;
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				return 18f;
			}
		}

		public class CenterVertical : tkControl<T, TContext>
		{
			[ShowInInspector]
			private readonly tkControl<T, TContext> _centered;

			public CenterVertical(tkControl<T, TContext> centered)
			{
				_centered = centered;
			}

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				float num = rect.height - _centered.GetHeight(obj, context, metadata);
				rect.y += num / 2f;
				rect.height -= num;
				return _centered.Edit(rect, obj, context, metadata);
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				return _centered.GetHeight(obj, context, metadata);
			}
		}

		public class Color : ColorIf
		{
			public Color(Value<UnityEngine.Color> color)
				: base(tk<T, TContext>.Val((Value<bool>.GeneratorNoContext)((T o) => true)), color)
			{
			}
		}

		public class ColorIf : ConditionalStyle
		{
			public ColorIf(Value<bool> shouldActivate, Value<UnityEngine.Color> color)
				: base((Func<T, TContext, bool>)shouldActivate.GetCurrentValue, (Func<T, TContext, object>)delegate(T obj, TContext context)
				{
					UnityEngine.Color color2 = GUI.color;
					GUI.color = color.GetCurrentValue(obj, context);
					return color2;
				}, (Action<T, TContext, object>)delegate(T obj, TContext context, object state)
				{
					GUI.color = (UnityEngine.Color)state;
				})
			{
			}

			public ColorIf(Value<bool>.Generator shouldActivate, Value<UnityEngine.Color> color)
				: this(tk<T, TContext>.Val(shouldActivate), color)
			{
			}

			public ColorIf(Value<bool>.GeneratorNoContext shouldActivate, Value<UnityEngine.Color> color)
				: this(tk<T, TContext>.Val(shouldActivate), color)
			{
			}
		}

		public class Comment : tkControl<T, TContext>
		{
			private readonly Value<string> _comment;

			private readonly CommentType _commentType;

			public Comment(Value<string> comment, CommentType commentType)
			{
				_comment = comment;
				_commentType = commentType;
			}

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				string currentValue = _comment.GetCurrentValue(obj, context);
				fiLateBindings.EditorGUI.HelpBox(rect, currentValue, _commentType);
				return obj;
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				string currentValue = _comment.GetCurrentValue(obj, context);
				return fiCommentUtility.GetCommentHeight(currentValue, _commentType);
			}
		}

		public class ConditionalStyle : tkStyle<T, TContext>
		{
			private readonly Func<T, TContext, bool> _shouldActivate;

			private readonly Func<T, TContext, object> _activate;

			private readonly Action<T, TContext, object> _deactivate;

			private readonly fiStackValue<bool> _activatedStack = new fiStackValue<bool>();

			private readonly fiStackValue<object> _activationStateStack = new fiStackValue<object>();

			public ConditionalStyle(Func<T, TContext, bool> shouldActivate, Func<T, TContext, object> activate, Action<T, TContext, object> deactivate)
			{
				_shouldActivate = shouldActivate;
				_activate = activate;
				_deactivate = deactivate;
			}

			public override void Activate(T obj, TContext context)
			{
				bool flag = _shouldActivate(obj, context);
				_activatedStack.Push(flag);
				if (flag)
				{
					_activationStateStack.Push(_activate(obj, context));
				}
			}

			public override void Deactivate(T obj, TContext context)
			{
				if (_activatedStack.Pop())
				{
					_deactivate(obj, context, _activationStateStack.Pop());
				}
			}
		}

		public class Context : tkControl<T, TContext>
		{
			[ShowInInspector]
			private tkControl<T, TContext> _control;

			[ShowInInspector]
			private readonly fiStackValue<T> _data = new fiStackValue<T>();

			public T Data
			{
				get
				{
					return _data.Value;
				}
			}

			public tkControl<T, TContext> With(tkControl<T, TContext> control)
			{
				_control = control;
				return this;
			}

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				_data.Push(obj);
				obj = _control.Edit(rect, obj, context, metadata);
				_data.Pop();
				return obj;
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				_data.Push(obj);
				float height = _control.GetHeight(obj, context, metadata);
				_data.Pop();
				return height;
			}
		}

		public class DefaultInspector : tkControl<T, TContext>
		{
			private readonly Type type_fitkControlPropertyEditor = TypeCache.FindType("FullInspector.Internal.tkControlPropertyEditor");

			private readonly Type type_IObjectPropertyEditor = TypeCache.FindType("FullInspector.Modules.Common.IObjectPropertyEditor");

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				return (T)fiLateBindings.PropertyEditor.EditSkipUntilNot(new Type[2] { type_fitkControlPropertyEditor, type_IObjectPropertyEditor }, typeof(T), typeof(T).Resolve(), rect, GUIContent.none, obj, new fiGraphMetadataChild
				{
					Metadata = metadata
				});
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				return fiLateBindings.PropertyEditor.GetElementHeightSkipUntilNot(new Type[2] { type_fitkControlPropertyEditor, type_IObjectPropertyEditor }, typeof(T), typeof(T).Resolve(), GUIContent.none, obj, new fiGraphMetadataChild
				{
					Metadata = metadata
				});
			}
		}

		public class DisableHierarchyMode : tkControl<T, TContext>
		{
			private tkControl<T, TContext> _childControl;

			public DisableHierarchyMode(tkControl<T, TContext> childControl)
			{
				_childControl = childControl;
			}

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				fiLateBindings.fiEditorGUI.PushHierarchyMode(false);
				T result = _childControl.Edit(rect, obj, context, metadata);
				fiLateBindings.fiEditorGUI.PopHierarchyMode();
				return result;
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				return _childControl.GetHeight(obj, context, metadata);
			}
		}

		public class Empty : tkControl<T, TContext>
		{
			[ShowInInspector]
			private readonly Value<float> _height;

			public Empty()
				: this((Value<float>)0f)
			{
			}

			public Empty(Value<float> height)
			{
				_height = height;
			}

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				return obj;
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				return _height.GetCurrentValue(obj, context);
			}
		}

		public class EnabledIf : ConditionalStyle
		{
			public EnabledIf(Value<bool> isEnabled)
				: base((Func<T, TContext, bool>)((T o, TContext c) => !isEnabled.GetCurrentValue(o, c)), (Func<T, TContext, object>)delegate
				{
					fiLateBindings.EditorGUI.BeginDisabledGroup(true);
					return null;
				}, (Action<T, TContext, object>)delegate
				{
					fiLateBindings.EditorGUI.EndDisabledGroup();
				})
			{
			}

			public EnabledIf(Value<bool>.Generator isEnabled)
				: this(tk<T, TContext>.Val(isEnabled))
			{
			}

			public EnabledIf(Value<bool>.GeneratorNoContext isEnabled)
				: this(tk<T, TContext>.Val(isEnabled))
			{
			}
		}

		public class Foldout : tkControl<T, TContext>
		{
			private readonly GUIStyle _foldoutStyle;

			[ShowInInspector]
			private readonly fiGUIContent _label;

			[ShowInInspector]
			private readonly tkControl<T, TContext> _control;

			[ShowInInspector]
			private readonly bool _defaultToExpanded;

			private bool _doNotIndentChildControl;

			public bool? HierarchyMode;

			[ShowInInspector]
			public bool IndentChildControl
			{
				get
				{
					return !_doNotIndentChildControl;
				}
				set
				{
					_doNotIndentChildControl = !value;
				}
			}

			public Foldout(fiGUIContent label, tkControl<T, TContext> control)
				: this(label, FontStyle.Normal, control)
			{
			}

			public Foldout(fiGUIContent label, FontStyle fontStyle, tkControl<T, TContext> control)
				: this(label, fontStyle, true, control)
			{
			}

			public Foldout(fiGUIContent label, FontStyle fontStyle, bool defaultToExpanded, tkControl<T, TContext> control)
			{
				_label = label;
				_foldoutStyle = new GUIStyle(fiLateBindings.EditorStyles.foldout)
				{
					fontStyle = fontStyle
				};
				_defaultToExpanded = defaultToExpanded;
				_control = control;
			}

			private tkFoldoutMetadata GetMetadata(fiGraphMetadata metadata)
			{
				bool wasCreated;
				tkFoldoutMetadata persistentMetadata = GetInstanceMetadata(metadata).GetPersistentMetadata<tkFoldoutMetadata>(out wasCreated);
				if (wasCreated)
				{
					persistentMetadata.IsExpanded = _defaultToExpanded;
				}
				return persistentMetadata;
			}

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				tkFoldoutMetadata metadata2 = GetMetadata(metadata);
				if (HierarchyMode.HasValue)
				{
					fiLateBindings.fiEditorGUI.PushHierarchyMode(HierarchyMode.Value);
				}
				Rect rect2 = rect;
				rect2.height = fiLateBindings.EditorGUIUtility.singleLineHeight;
				metadata2.IsExpanded = fiLateBindings.EditorGUI.Foldout(rect2, metadata2.IsExpanded, _label, true, _foldoutStyle);
				if (metadata2.IsExpanded)
				{
					float num = fiLateBindings.EditorGUIUtility.singleLineHeight + fiLateBindings.EditorGUIUtility.standardVerticalSpacing;
					Rect rect3 = rect;
					if (IndentChildControl)
					{
						rect3.x += 15f;
						rect3.width -= 15f;
					}
					rect3.y += num;
					rect3.height -= num;
					obj = _control.Edit(rect3, obj, context, metadata);
				}
				if (HierarchyMode.HasValue)
				{
					fiLateBindings.fiEditorGUI.PopHierarchyMode();
				}
				return obj;
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				tkFoldoutMetadata metadata2 = GetMetadata(metadata);
				float num = fiLateBindings.EditorGUIUtility.singleLineHeight;
				if (metadata2.IsExpanded)
				{
					num += fiLateBindings.EditorGUIUtility.standardVerticalSpacing;
					num += _control.GetHeight(obj, context, metadata);
				}
				return num;
			}
		}

		public class HorizontalGroup : tkControl<T, TContext>, IEnumerable
		{
			private struct SectionItem
			{
				private float _minWidth;

				private float _fillStrength;

				public bool MatchParentHeight;

				public tkControl<T, TContext> Rule;

				public bool Layout_IsFlexible;

				public float Layout_FlexibleWidth;

				[ShowInInspector]
				public float MinWidth
				{
					get
					{
						return _minWidth;
					}
					set
					{
						_minWidth = Math.Max(value, 0f);
					}
				}

				[ShowInInspector]
				public float FillStrength
				{
					get
					{
						return _fillStrength;
					}
					set
					{
						_fillStrength = Math.Max(value, 0f);
					}
				}

				public float Layout_Width
				{
					get
					{
						if (Layout_IsFlexible)
						{
							return Layout_FlexibleWidth;
						}
						return MinWidth;
					}
				}
			}

			[ShowInInspector]
			private readonly List<SectionItem> _items = new List<SectionItem>();

			private static readonly tkControl<T, TContext> DefaultRule = new VerticalGroup();

			protected override IEnumerable<tkIControl> NonMemberChildControls
			{
				get
				{
					foreach (SectionItem item in _items)
					{
						yield return item.Rule;
					}
				}
			}

			public void Add(tkControl<T, TContext> rule)
			{
				InternalAdd(false, 0f, 1f, rule);
			}

			public void Add(bool matchParentHeight, tkControl<T, TContext> rule)
			{
				InternalAdd(matchParentHeight, 0f, 1f, rule);
			}

			public void Add(float width)
			{
				InternalAdd(false, width, 0f, DefaultRule);
			}

			public void Add(float width, tkControl<T, TContext> rule)
			{
				InternalAdd(false, width, 0f, rule);
			}

			private void InternalAdd(bool matchParentHeight, float width, float fillStrength, tkControl<T, TContext> rule)
			{
				if (width < 0f)
				{
					throw new ArgumentException("width must be >= 0");
				}
				if (fillStrength < 0f)
				{
					throw new ArgumentException("fillStrength must be >= 0");
				}
				_items.Add(new SectionItem
				{
					MatchParentHeight = matchParentHeight,
					MinWidth = width,
					FillStrength = fillStrength,
					Rule = rule
				});
			}

			private void DoLayout(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				//Discarded unreachable code: IL_017b
				for (int i = 0; i < _items.Count; i++)
				{
					SectionItem value = _items[i];
					value.Layout_IsFlexible = value.FillStrength > 0f;
					_items[i] = value;
				}
				while (true)
				{
					float num = 0f;
					float num2 = 0f;
					for (int j = 0; j < _items.Count; j++)
					{
						SectionItem sectionItem = _items[j];
						if (sectionItem.Rule.ShouldShow(obj, context, metadata))
						{
							if (sectionItem.Layout_IsFlexible)
							{
								num2 += sectionItem.FillStrength;
							}
							else
							{
								num += sectionItem.MinWidth;
							}
						}
					}
					float num3 = rect.width - num;
					int num4 = 0;
					SectionItem value2;
					while (true)
					{
						if (num4 >= _items.Count)
						{
							return;
						}
						value2 = _items[num4];
						if (value2.Rule.ShouldShow(obj, context, metadata) && value2.Layout_IsFlexible)
						{
							value2.Layout_FlexibleWidth = num3 * value2.FillStrength / num2;
							_items[num4] = value2;
							if (value2.Layout_FlexibleWidth < value2.MinWidth)
							{
								break;
							}
						}
						num4++;
					}
					value2.Layout_IsFlexible = false;
					_items[num4] = value2;
				}
			}

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				DoLayout(rect, obj, context, metadata);
				for (int i = 0; i < _items.Count; i++)
				{
					SectionItem sectionItem = _items[i];
					if (sectionItem.Rule.ShouldShow(obj, context, metadata))
					{
						float layout_Width = sectionItem.Layout_Width;
						Rect rect2 = rect;
						rect2.width = layout_Width;
						if (!sectionItem.MatchParentHeight)
						{
							rect2.height = sectionItem.Rule.GetHeight(obj, context, metadata);
						}
						obj = sectionItem.Rule.Edit(rect2, obj, context, metadata);
						rect.x += layout_Width;
					}
				}
				return obj;
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				float num = 0f;
				for (int i = 0; i < _items.Count; i++)
				{
					SectionItem sectionItem = _items[i];
					if (sectionItem.Rule.ShouldShow(obj, context, metadata))
					{
						num = Math.Max(num, sectionItem.Rule.GetHeight(obj, context, metadata));
					}
				}
				return num;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				throw new NotSupportedException();
			}
		}

		public class Indent : tkControl<T, TContext>
		{
			[ShowInInspector]
			private readonly Value<float> _indent;

			[ShowInInspector]
			private readonly tkControl<T, TContext> _control;

			public Indent(tkControl<T, TContext> control)
				: this((Value<float>)15f, control)
			{
			}

			public Indent(Value<float> indent, tkControl<T, TContext> control)
			{
				_indent = indent;
				_control = control;
			}

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				float currentValue = _indent.GetCurrentValue(obj, context);
				rect.x += currentValue;
				rect.width -= currentValue;
				return _control.Edit(rect, obj, context, metadata);
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				return _control.GetHeight(obj, context, metadata);
			}
		}

		public class IntSlider : tkControl<T, TContext>
		{
			private readonly Value<int> _min;

			private readonly Value<int> _max;

			private readonly Func<T, TContext, int> _getValue;

			private readonly Action<T, TContext, int> _setValue;

			private readonly Value<fiGUIContent> _label;

			public IntSlider(Value<int> min, Value<int> max, Func<T, TContext, int> getValue, Action<T, TContext, int> setValue)
				: this((Value<fiGUIContent>)fiGUIContent.Empty, min, max, getValue, setValue)
			{
			}

			public IntSlider(Value<fiGUIContent> label, Value<int> min, Value<int> max, Func<T, TContext, int> getValue, Action<T, TContext, int> setValue)
			{
				_label = label;
				_min = min;
				_max = max;
				_getValue = getValue;
				_setValue = setValue;
			}

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				int value = _getValue(obj, context);
				int currentValue = _min.GetCurrentValue(obj, context);
				int currentValue2 = _max.GetCurrentValue(obj, context);
				fiLateBindings.EditorGUI.BeginChangeCheck();
				value = fiLateBindings.EditorGUI.IntSlider(rect, _label.GetCurrentValue(obj, context), value, currentValue, currentValue2);
				if (fiLateBindings.EditorGUI.EndChangeCheck())
				{
					_setValue(obj, context, value);
				}
				return obj;
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				return fiLateBindings.EditorGUIUtility.singleLineHeight;
			}
		}

		public class Label : tkControl<T, TContext>
		{
			public Value<fiGUIContent> GUIContent;

			[ShowInInspector]
			private readonly FontStyle _fontStyle;

			[ShowInInspector]
			private readonly tkControl<T, TContext> _control;

			public bool InlineControl;

			public Label(fiGUIContent label)
				: this(label, FontStyle.Normal, (tkControl<T, TContext>)null)
			{
			}

			public Label(Value<fiGUIContent> label)
				: this(label, FontStyle.Normal, (tkControl<T, TContext>)null)
			{
			}

			public Label(Value<fiGUIContent>.Generator label)
				: this(label, FontStyle.Normal, (tkControl<T, TContext>)null)
			{
			}

			public Label(fiGUIContent label, FontStyle fontStyle)
				: this(label, fontStyle, (tkControl<T, TContext>)null)
			{
			}

			public Label(Value<fiGUIContent> label, FontStyle fontStyle)
				: this(label, fontStyle, (tkControl<T, TContext>)null)
			{
			}

			public Label(Value<fiGUIContent>.Generator label, FontStyle fontStyle)
				: this(label, fontStyle, (tkControl<T, TContext>)null)
			{
			}

			public Label(fiGUIContent label, tkControl<T, TContext> control)
				: this(label, FontStyle.Normal, control)
			{
			}

			public Label(Value<fiGUIContent> label, tkControl<T, TContext> control)
				: this(label, FontStyle.Normal, control)
			{
			}

			public Label(Value<fiGUIContent>.Generator label, tkControl<T, TContext> control)
				: this(label, FontStyle.Normal, control)
			{
			}

			public Label(fiGUIContent label, FontStyle fontStyle, tkControl<T, TContext> control)
				: this(tk<T, TContext>.Val(label), fontStyle, control)
			{
			}

			public Label(Value<fiGUIContent> label, FontStyle fontStyle, tkControl<T, TContext> control)
			{
				GUIContent = label;
				_fontStyle = fontStyle;
				_control = control;
			}

			public Label(Value<fiGUIContent>.Generator label, FontStyle fontStyle, tkControl<T, TContext> control)
				: this(tk<T, TContext>.Val(label), fontStyle, control)
			{
			}

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				fiGUIContent currentValue = GUIContent.GetCurrentValue(obj, context);
				Rect position = rect;
				Rect rect2 = rect;
				bool flag = false;
				if (_control != null && !currentValue.IsEmpty)
				{
					position.height = fiLateBindings.EditorGUIUtility.singleLineHeight;
					if (InlineControl)
					{
						position.width = fiGUI.PushLabelWidth(currentValue, position.width);
						flag = true;
						rect2.x += position.width;
						rect2.width -= position.width;
					}
					else
					{
						float num = position.height + fiLateBindings.EditorGUIUtility.standardVerticalSpacing;
						rect2.x += 15f;
						rect2.width -= 15f;
						rect2.y += num;
						rect2.height -= num;
					}
				}
				if (!currentValue.IsEmpty)
				{
					GUIStyle label = fiLateBindings.EditorStyles.label;
					FontStyle fontStyle = label.fontStyle;
					label.fontStyle = _fontStyle;
					GUI.Label(position, currentValue, label);
					label.fontStyle = fontStyle;
				}
				if (_control != null)
				{
					_control.Edit(rect2, obj, context, metadata);
				}
				if (flag)
				{
					fiGUI.PopLabelWidth();
				}
				return obj;
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				float num = 0f;
				if (!GUIContent.GetCurrentValue(obj, context).IsEmpty)
				{
					num += fiLateBindings.EditorGUIUtility.singleLineHeight;
				}
				if (_control != null)
				{
					float height = _control.GetHeight(obj, context, metadata);
					if (!InlineControl)
					{
						num += fiLateBindings.EditorGUIUtility.standardVerticalSpacing + height;
					}
				}
				return num;
			}
		}

		public class Margin : tkControl<T, TContext>
		{
			[ShowInInspector]
			private readonly Value<float> _left;

			[ShowInInspector]
			private readonly Value<float> _top;

			[ShowInInspector]
			private readonly Value<float> _right;

			[ShowInInspector]
			private readonly Value<float> _bottom;

			[ShowInInspector]
			private readonly tkControl<T, TContext> _control;

			public Margin(Value<float> margin, tkControl<T, TContext> control)
				: this(margin, margin, margin, margin, control)
			{
			}

			public Margin(Value<float> left, Value<float> top, tkControl<T, TContext> control)
				: this(left, top, left, top, control)
			{
			}

			public Margin(Value<float> left, Value<float> top, Value<float> right, Value<float> bottom, tkControl<T, TContext> control)
			{
				_left = left;
				_top = top;
				_right = right;
				_bottom = bottom;
				_control = control;
			}

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				float currentValue = _left.GetCurrentValue(obj, context);
				float currentValue2 = _right.GetCurrentValue(obj, context);
				float currentValue3 = _top.GetCurrentValue(obj, context);
				float currentValue4 = _bottom.GetCurrentValue(obj, context);
				rect.x += currentValue;
				rect.width -= currentValue + currentValue2;
				rect.y += currentValue3;
				rect.height -= currentValue3 + currentValue4;
				return _control.Edit(rect, obj, context, metadata);
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				float currentValue = _top.GetCurrentValue(obj, context);
				float currentValue2 = _bottom.GetCurrentValue(obj, context);
				return _control.GetHeight(obj, context, metadata) + currentValue + currentValue2;
			}
		}

		public class Popup : tkControl<T, TContext>
		{
			public delegate T OnSelectionChanged(T obj, TContext context, int selected);

			private readonly Value<fiGUIContent> _label;

			private readonly Value<GUIContent[]> _options;

			private readonly Value<int> _currentSelection;

			private readonly OnSelectionChanged _onSelectionChanged;

			public Popup(Value<fiGUIContent> label, Value<GUIContent[]> options, Value<int> currentSelection, OnSelectionChanged onSelectionChanged)
			{
				_label = label;
				_options = options;
				_currentSelection = currentSelection;
				_onSelectionChanged = onSelectionChanged;
			}

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				fiGUIContent currentValue = _label.GetCurrentValue(obj, context);
				int currentValue2 = _currentSelection.GetCurrentValue(obj, context);
				GUIContent[] currentValue3 = _options.GetCurrentValue(obj, context);
				int num = fiLateBindings.EditorGUI.Popup(rect, currentValue.AsGUIContent, currentValue2, currentValue3);
				if (currentValue2 != num)
				{
					obj = _onSelectionChanged(obj, context, num);
				}
				return obj;
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				return fiLateBindings.EditorGUIUtility.singleLineHeight;
			}
		}

		public class PropertyEditor : tkControl<T, TContext>
		{
			private MemberInfo _attributes;

			private Func<T, TContext, object> _getValue;

			private Action<T, TContext, object> _setValue;

			private Value<fiGUIContent> _label;

			private Type _fieldType;

			private string _errorMessage;

			public PropertyEditor(string memberName)
			{
				InitializeFromMemberName(memberName);
			}

			public PropertyEditor(fiGUIContent label, string memberName)
				: this(memberName)
			{
				_label = label;
			}

			public PropertyEditor(Value<fiGUIContent> label, string memberName)
				: this(memberName)
			{
				_label = label;
			}

			public PropertyEditor(fiGUIContent label, Type fieldType, MemberInfo attributes, Func<T, TContext, object> getValue, Action<T, TContext, object> setValue)
			{
				_label = label;
				_fieldType = fieldType;
				_attributes = attributes;
				_getValue = getValue;
				_setValue = setValue;
			}

			private void InitializeFromMemberName(string memberName)
			{
				InspectedProperty property = InspectedType.Get(typeof(T)).GetPropertyByName(memberName);
				if (property == null)
				{
					_errorMessage = "Unable to locate member `" + memberName + "` on type `" + typeof(T).CSharpName() + "`";
					_fieldType = typeof(T);
					_attributes = null;
					_getValue = (T o, TContext c) => default(T);
					_setValue = delegate
					{
					};
					_label = new fiGUIContent(memberName + " (unable to locate)");
				}
				else
				{
					_fieldType = property.StorageType;
					_attributes = property.MemberInfo;
					_getValue = (T o, TContext c) => property.Read(o);
					_setValue = delegate(T o, TContext c, object v)
					{
						property.Write(o, v);
					};
					_label = new fiGUIContent(property.DisplayName);
				}
			}

			public static PropertyEditor Create<TEdited>(fiGUIContent label, MemberInfo attributes, Func<T, TContext, TEdited> getValue, Action<T, TContext, TEdited> setValue)
			{
				return new PropertyEditor(label, typeof(TEdited), attributes, (T o, TContext c) => getValue(o, c), delegate(T o, TContext c, object v)
				{
					setValue(o, c, (TEdited)v);
				});
			}

			public static PropertyEditor Create<TEdited>(fiGUIContent label, Func<T, TContext, TEdited> getValue)
			{
				return new PropertyEditor(label, typeof(TEdited), null, (T o, TContext c) => getValue(o, c), null);
			}

			public static PropertyEditor Create<TEdited>(fiGUIContent label, Func<T, TContext, TEdited> getValue, Action<T, TContext, TEdited> setValue)
			{
				return new PropertyEditor(label, typeof(TEdited), null, (T o, TContext c) => getValue(o, c), delegate(T o, TContext c, object v)
				{
					setValue(o, c, (TEdited)v);
				});
			}

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				if (_errorMessage != null)
				{
					fiLateBindings.EditorGUI.HelpBox(rect, _errorMessage, CommentType.Error);
					return obj;
				}
				fiLateBindings.EditorGUI.BeginChangeCheck();
				fiLateBindings.EditorGUI.BeginDisabledGroup(_setValue == null);
				object obj2 = _getValue(obj, context);
				fiGraphMetadataChild fiGraphMetadataChild2 = default(fiGraphMetadataChild);
				fiGraphMetadataChild2.Metadata = GetInstanceMetadata(metadata);
				fiGraphMetadataChild metadata2 = fiGraphMetadataChild2;
				fiGUIContent currentValue = _label.GetCurrentValue(obj, context);
				object arg = fiLateBindings.PropertyEditor.Edit(_fieldType, _attributes, rect, currentValue, obj2, metadata2);
				fiLateBindings.EditorGUI.EndDisabledGroup();
				if (fiLateBindings.EditorGUI.EndChangeCheck() && _setValue != null)
				{
					_setValue(obj, context, arg);
				}
				return obj;
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				if (_errorMessage != null)
				{
					return fiCommentUtility.GetCommentHeight(_errorMessage, CommentType.Error);
				}
				object obj2 = _getValue(obj, context);
				fiGraphMetadataChild fiGraphMetadataChild2 = default(fiGraphMetadataChild);
				fiGraphMetadataChild2.Metadata = GetInstanceMetadata(metadata);
				fiGraphMetadataChild metadata2 = fiGraphMetadataChild2;
				fiGUIContent currentValue = _label.GetCurrentValue(obj, context);
				return fiLateBindings.PropertyEditor.GetElementHeight(_fieldType, _attributes, currentValue, obj2, metadata2);
			}
		}

		public class StyleProxy : tkControl<T, TContext>
		{
			public tkControl<T, TContext> Control;

			public StyleProxy()
			{
			}

			public StyleProxy(tkControl<T, TContext> control)
			{
				Control = control;
			}

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				return Control.Edit(rect, obj, context, metadata);
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				return Control.GetHeight(obj, context, metadata);
			}
		}

		public class ReadOnly : ReadOnlyIf
		{
			public ReadOnly()
				: base(tk<T, TContext>.Val((Value<bool>.GeneratorNoContext)((T o) => true)))
			{
			}
		}

		public class ReadOnlyIf : ConditionalStyle
		{
			public ReadOnlyIf(Value<bool> isReadOnly)
				: base((Func<T, TContext, bool>)isReadOnly.GetCurrentValue, (Func<T, TContext, object>)delegate
				{
					fiLateBindings.EditorGUI.BeginDisabledGroup(true);
					return null;
				}, (Action<T, TContext, object>)delegate
				{
					fiLateBindings.EditorGUI.EndDisabledGroup();
				})
			{
			}

			public ReadOnlyIf(Value<bool>.Generator isReadOnly)
				: this(tk<T, TContext>.Val(isReadOnly))
			{
			}

			public ReadOnlyIf(Value<bool>.GeneratorNoContext isReadOnly)
				: this(tk<T, TContext>.Val(isReadOnly))
			{
			}
		}

		public class ShowIf : tkControl<T, TContext>
		{
			private readonly Value<bool> _shouldDisplay;

			[ShowInInspector]
			private readonly tkControl<T, TContext> _control;

			public ShowIf(Value<bool> shouldDisplay, tkControl<T, TContext> control)
			{
				_shouldDisplay = shouldDisplay;
				_control = control;
			}

			public ShowIf(Value<bool>.Generator shouldDisplay, tkControl<T, TContext> control)
				: this(new Value<bool>(shouldDisplay), control)
			{
			}

			public ShowIf(Value<bool>.GeneratorNoContext shouldDisplay, tkControl<T, TContext> control)
				: this(new Value<bool>(shouldDisplay), control)
			{
			}

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				return _control.Edit(rect, obj, context, metadata);
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				return _control.GetHeight(obj, context, metadata);
			}

			public override bool ShouldShow(T obj, TContext context, fiGraphMetadata metadata)
			{
				return _shouldDisplay.GetCurrentValue(obj, context);
			}
		}

		public class Slider : tkControl<T, TContext>
		{
			private readonly Value<float> _min;

			private readonly Value<float> _max;

			private readonly Func<T, TContext, float> _getValue;

			private readonly Action<T, TContext, float> _setValue;

			private readonly Value<fiGUIContent> _label;

			public Slider(Value<float> min, Value<float> max, Func<T, TContext, float> getValue, Action<T, TContext, float> setValue)
				: this((Value<fiGUIContent>)fiGUIContent.Empty, min, max, getValue, setValue)
			{
			}

			public Slider(Value<fiGUIContent> label, Value<float> min, Value<float> max, Func<T, TContext, float> getValue, Action<T, TContext, float> setValue)
			{
				_label = label;
				_min = min;
				_max = max;
				_getValue = getValue;
				_setValue = setValue;
			}

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				float value = _getValue(obj, context);
				float currentValue = _min.GetCurrentValue(obj, context);
				float currentValue2 = _max.GetCurrentValue(obj, context);
				fiLateBindings.EditorGUI.BeginChangeCheck();
				value = fiLateBindings.EditorGUI.Slider(rect, _label.GetCurrentValue(obj, context), value, currentValue, currentValue2);
				if (fiLateBindings.EditorGUI.EndChangeCheck())
				{
					_setValue(obj, context, value);
				}
				return obj;
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				return fiLateBindings.EditorGUIUtility.singleLineHeight;
			}
		}

		public class VerticalGroup : tkControl<T, TContext>, IEnumerable
		{
			private struct SectionItem
			{
				public tkControl<T, TContext> Rule;
			}

			[ShowInInspector]
			private readonly List<SectionItem> _items = new List<SectionItem>();

			private readonly float _marginBetweenItems;

			protected override IEnumerable<tkIControl> NonMemberChildControls
			{
				get
				{
					foreach (SectionItem item in _items)
					{
						yield return item.Rule;
					}
				}
			}

			public VerticalGroup()
				: this(fiLateBindings.EditorGUIUtility.standardVerticalSpacing)
			{
			}

			public VerticalGroup(float marginBetweenItems)
			{
				_marginBetweenItems = marginBetweenItems;
			}

			public void Add(tkControl<T, TContext> rule)
			{
				InternalAdd(rule);
			}

			private void InternalAdd(tkControl<T, TContext> rule)
			{
				_items.Add(new SectionItem
				{
					Rule = rule
				});
			}

			protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
			{
				for (int i = 0; i < _items.Count; i++)
				{
					SectionItem sectionItem = _items[i];
					if (sectionItem.Rule.ShouldShow(obj, context, metadata))
					{
						float height = sectionItem.Rule.GetHeight(obj, context, metadata);
						Rect rect2 = rect;
						rect2.height = height;
						obj = sectionItem.Rule.Edit(rect2, obj, context, metadata);
						rect.y += height;
						rect.y += _marginBetweenItems;
					}
				}
				return obj;
			}

			protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
			{
				float num = 0f;
				for (int i = 0; i < _items.Count; i++)
				{
					SectionItem sectionItem = _items[i];
					if (sectionItem.Rule.ShouldShow(obj, context, metadata))
					{
						num += sectionItem.Rule.GetHeight(obj, context, metadata);
						if (i != _items.Count - 1)
						{
							num += _marginBetweenItems;
						}
					}
				}
				return num;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				throw new NotSupportedException();
			}
		}

		public struct Value<TValue>
		{
			public delegate TValue Generator(T input, TContext context);

			public delegate TValue GeneratorNoContext(T input);

			private Generator _generator;

			private TValue _direct;

			public Value(Generator generator)
			{
				_generator = generator;
				_direct = default(TValue);
			}

			public Value(GeneratorNoContext generator)
			{
				_generator = (T o, TContext context) => generator(o);
				_direct = default(TValue);
			}

			public TValue GetCurrentValue(T instance, TContext context)
			{
				if (_generator == null)
				{
					return _direct;
				}
				return _generator(instance, context);
			}

			public static implicit operator Value<TValue>(TValue direct)
			{
				Value<TValue> result = default(Value<TValue>);
				result._generator = null;
				result._direct = direct;
				return result;
			}

			public static implicit operator Value<TValue>(Generator generator)
			{
				Value<TValue> result = default(Value<TValue>);
				result._generator = generator;
				result._direct = default(TValue);
				return result;
			}

			public static implicit operator Value<TValue>(GeneratorNoContext generator)
			{
				Value<TValue> result = default(Value<TValue>);
				result._generator = (T obj, TContext context) => generator(obj);
				result._direct = default(TValue);
				return result;
			}

			public static implicit operator Value<TValue>(Func<T, int, TValue> generator)
			{
				return default(Value<TValue>);
			}

			public static implicit operator Value<TValue>(Func<T, TValue> generator)
			{
				Value<TValue> result = default(Value<TValue>);
				result._generator = (T obj, TContext context) => generator(obj);
				result._direct = default(TValue);
				return result;
			}
		}

		public static Value<TValue> Val<TValue>(Value<TValue>.GeneratorNoContext generator)
		{
			return generator;
		}

		public static Value<TValue> Val<TValue>(Value<TValue>.Generator generator)
		{
			return generator;
		}

		public static Value<TValue> Val<TValue>(TValue value)
		{
			return value;
		}
	}
	public class tk<T> : tk<T, tkDefaultContext>
	{
	}
}
