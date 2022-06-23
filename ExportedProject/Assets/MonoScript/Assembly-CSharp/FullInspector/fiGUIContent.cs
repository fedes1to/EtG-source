using UnityEngine;

namespace FullInspector
{
	public class fiGUIContent
	{
		public static fiGUIContent Empty = new fiGUIContent();

		private string _text;

		private string _tooltip;

		private Texture _image;

		public GUIContent AsGUIContent
		{
			get
			{
				return new GUIContent(_text, _image, _tooltip);
			}
		}

		public bool IsEmpty
		{
			get
			{
				if (!string.IsNullOrEmpty(_text))
				{
					return false;
				}
				if (!string.IsNullOrEmpty(_tooltip))
				{
					return false;
				}
				if (_image != null)
				{
					return false;
				}
				return true;
			}
		}

		public fiGUIContent()
			: this(string.Empty, string.Empty, null)
		{
		}

		public fiGUIContent(string text)
			: this(text, string.Empty, null)
		{
		}

		public fiGUIContent(string text, string tooltip)
			: this(text, tooltip, null)
		{
		}

		public fiGUIContent(string text, string tooltip, Texture image)
		{
			_text = text;
			_tooltip = tooltip;
			_image = image;
		}

		public fiGUIContent(Texture image)
			: this(string.Empty, string.Empty, image)
		{
		}

		public fiGUIContent(Texture image, string tooltip)
			: this(string.Empty, tooltip, image)
		{
		}

		public static implicit operator GUIContent(fiGUIContent label)
		{
			if (label == null)
			{
				return GUIContent.none;
			}
			return label.AsGUIContent;
		}

		public static implicit operator fiGUIContent(string text)
		{
			fiGUIContent fiGUIContent2 = new fiGUIContent();
			fiGUIContent2._text = text;
			return fiGUIContent2;
		}

		public static implicit operator fiGUIContent(GUIContent label)
		{
			fiGUIContent fiGUIContent2 = new fiGUIContent();
			fiGUIContent2._text = label.text;
			fiGUIContent2._tooltip = label.tooltip;
			fiGUIContent2._image = label.image;
			return fiGUIContent2;
		}
	}
}
