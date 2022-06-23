using UnityEngine;

namespace FullInspector.Internal
{
	public static class fiRectUtility
	{
		public const float IndentHorizontal = 15f;

		public const float IndentVertical = 2f;

		public static Rect IndentedRect(Rect source)
		{
			return new Rect(source.x + 15f, source.y + 2f, source.width - 15f, source.height - 2f);
		}

		public static Rect MoveDown(Rect rect, float amount)
		{
			rect.y += amount;
			rect.height -= amount;
			return rect;
		}

		public static void SplitLeftHorizontalExact(Rect rect, float leftWidth, float margin, out Rect left, out Rect right)
		{
			left = rect;
			right = rect;
			left.width = leftWidth;
			right.x += leftWidth + margin;
			right.width -= leftWidth + margin;
		}

		public static void SplitRightHorizontalExact(Rect rect, float rightWidth, float margin, out Rect left, out Rect right)
		{
			left = new Rect(rect);
			left.width -= rightWidth + margin;
			right = new Rect(rect);
			right.x += left.width + margin;
			right.width = rightWidth;
		}

		public static void SplitHorizontalPercentage(Rect rect, float percentage, float margin, out Rect left, out Rect right)
		{
			left = new Rect(rect);
			left.width *= percentage;
			right = new Rect(rect);
			right.x += left.width + margin;
			right.width -= left.width + margin;
		}

		public static void SplitHorizontalMiddleExact(Rect rect, float middleWidth, float margin, out Rect left, out Rect middle, out Rect right)
		{
			left = new Rect(rect);
			left.width = (rect.width - 2f * margin - middleWidth) / 2f;
			middle = new Rect(rect);
			middle.x += left.width + margin;
			middle.width = middleWidth;
			right = new Rect(rect);
			right.x += left.width + margin + middleWidth + margin;
			right.width = (rect.width - 2f * margin - middleWidth) / 2f;
		}

		public static void SplitHorizontalFlexibleMiddle(Rect rect, float leftWidth, float rightWidth, out Rect left, out Rect middle, out Rect right)
		{
			left = new Rect(rect);
			left.width = leftWidth;
			middle = new Rect(rect);
			middle.x += left.width;
			middle.width = rect.width - leftWidth - rightWidth;
			right = new Rect(rect);
			right.x += left.width + middle.width;
			right.width = rightWidth;
		}

		public static void CenterRect(Rect toCenter, float height, out Rect centered)
		{
			float num = toCenter.height - height;
			centered = new Rect(toCenter);
			centered.y += num / 2f;
			centered.height = height;
		}

		public static void Margin(Rect container, float horizontalMargin, float verticalMargin, out Rect smaller)
		{
			smaller = container;
			smaller.x += horizontalMargin;
			smaller.width -= horizontalMargin * 2f;
			smaller.y += verticalMargin;
			smaller.height -= verticalMargin * 2f;
		}

		public static void SplitVerticalPercentage(Rect rect, float percentage, float margin, out Rect top, out Rect bottom)
		{
			top = new Rect(rect);
			top.height *= percentage;
			bottom = new Rect(rect);
			bottom.y += top.height + margin;
			bottom.height -= top.height + margin;
		}
	}
}
