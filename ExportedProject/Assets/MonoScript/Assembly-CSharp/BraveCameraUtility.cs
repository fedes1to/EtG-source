using UnityEngine;

public static class BraveCameraUtility
{
	public static float? OverrideAspect;

	private static int m_cachedMultiple = -1;

	public static float ASPECT
	{
		get
		{
			if (OverrideAspect.HasValue)
			{
				return OverrideAspect.Value;
			}
			if (GameManager.Options.CurrentPreferredScalingMode == GameOptions.PreferredScalingMode.FORCE_PIXEL_PERFECT || GameManager.Options.CurrentPreferredScalingMode == GameOptions.PreferredScalingMode.PIXEL_PERFECT)
			{
				return 1.77777779f;
			}
			return Mathf.Max(1.77777779f, (float)Screen.width / (float)Screen.height);
		}
		set
		{
		}
	}

	public static int H_PIXELS
	{
		get
		{
			return Mathf.RoundToInt(480f * (ASPECT / 1.77777779f));
		}
	}

	public static int V_PIXELS
	{
		get
		{
			return Mathf.RoundToInt((float)H_PIXELS / ASPECT);
		}
	}

	public static Rect GetRect()
	{
		if (GameManager.Options.CurrentPreferredScalingMode != GameOptions.PreferredScalingMode.UNIFORM_SCALING && GameManager.Options.CurrentPreferredScalingMode != GameOptions.PreferredScalingMode.UNIFORM_SCALING_FAST)
		{
			int a = Mathf.FloorToInt((float)Screen.width / (float)H_PIXELS);
			int b = Mathf.FloorToInt((float)Screen.height / (float)V_PIXELS);
			int num = Mathf.Min(a, b);
			int num2 = H_PIXELS * num;
			int num3 = V_PIXELS * num;
			float num4 = 1f - (float)num2 / (float)Screen.width;
			float num5 = 1f - (float)num3 / (float)Screen.height;
			return new Rect(num4 / 2f, num5 / 2f, 1f - num4, 1f - num5);
		}
		float num6 = (float)Screen.width / (float)Screen.height;
		float num7 = 0f;
		float num8 = 0f;
		if (Screen.width % 16 == 0 && Screen.height % 9 == 0 && Screen.width / 16 == Screen.height / 9)
		{
			return new Rect(0f, 0f, 1f, 1f);
		}
		if (num6 > ASPECT)
		{
			num7 = 1f - ASPECT / num6;
		}
		else if (num6 < ASPECT)
		{
			num8 = 1f - num6 / ASPECT;
		}
		return new Rect(num7 / 2f, num8 / 2f, 1f - num7, 1f - num8);
	}

	public static Vector2 GetRectSize()
	{
		if (GameManager.Options.CurrentPreferredScalingMode != GameOptions.PreferredScalingMode.UNIFORM_SCALING && GameManager.Options.CurrentPreferredScalingMode != GameOptions.PreferredScalingMode.UNIFORM_SCALING_FAST)
		{
			int a = Mathf.FloorToInt((float)Screen.width / (float)H_PIXELS);
			int b = Mathf.FloorToInt((float)Screen.height / (float)V_PIXELS);
			int num = Mathf.Min(a, b);
			int num2 = H_PIXELS * num;
			int num3 = V_PIXELS * num;
			float num4 = 1f - (float)num2 / (float)Screen.width;
			float num5 = 1f - (float)num3 / (float)Screen.height;
			return new Vector2(1f - num4, 1f - num5);
		}
		float num6 = (float)Screen.width / (float)Screen.height;
		float num7 = 0f;
		float num8 = 0f;
		if (Screen.width % 16 == 0 && Screen.height % 9 == 0 && Screen.width / 16 == Screen.height / 9)
		{
			return Vector2.one;
		}
		if (num6 > ASPECT)
		{
			num7 = 1f - ASPECT / num6;
		}
		else if (num6 < ASPECT)
		{
			num8 = 1f - num6 / ASPECT;
		}
		return new Vector2(1f - num7, 1f - num8);
	}

	public static Vector2 ConvertGameViewportToScreenViewport(Vector2 pos)
	{
		Rect rect = GetRect();
		return new Vector2(pos.x * rect.width + rect.x, pos.y * rect.height + rect.y);
	}

	public static IntVector2 GetTargetScreenResolution(IntVector2 inResolution)
	{
		float num = (float)inResolution.x / (float)inResolution.y;
		if (inResolution.x % 16 == 0 && inResolution.y % 9 == 0 && inResolution.x / 16 == inResolution.y / 9)
		{
			return inResolution;
		}
		if (num > ASPECT)
		{
			float num2 = num / ASPECT;
			float f = (float)inResolution.y * num2;
			return new IntVector2(inResolution.x, Mathf.RoundToInt(f));
		}
		if (num < ASPECT)
		{
			float num3 = num / ASPECT;
			float f2 = (float)inResolution.x / num3;
			return new IntVector2(Mathf.RoundToInt(f2), inResolution.y);
		}
		return inResolution;
	}

	public static void MaintainCameraAspect(Camera c)
	{
		c.transparencySortMode = TransparencySortMode.Orthographic;
		if (GameManager.Options == null || (GameManager.Options.CurrentPreferredScalingMode != GameOptions.PreferredScalingMode.UNIFORM_SCALING && GameManager.Options.CurrentPreferredScalingMode != GameOptions.PreferredScalingMode.UNIFORM_SCALING_FAST))
		{
			int a = Mathf.FloorToInt((float)Screen.width / (float)H_PIXELS);
			int b = Mathf.FloorToInt((float)Screen.height / (float)V_PIXELS);
			int num = Mathf.Max(1, Mathf.Min(a, b));
			int num2 = H_PIXELS * num;
			int num3 = V_PIXELS * num;
			float num4 = 1f - (float)num2 / (float)Screen.width;
			float num5 = 1f - (float)num3 / (float)Screen.height;
			c.rect = new Rect(num4 / 2f, num5 / 2f, 1f - num4, 1f - num5);
			if (m_cachedMultiple != num)
			{
				dfGUIManager.ForceResolutionUpdates();
			}
			m_cachedMultiple = num;
		}
		else
		{
			float num6 = (float)Screen.width / (float)Screen.height;
			float aSPECT = ASPECT;
			float num7 = 0f;
			float num8 = 0f;
			bool flag = false;
			if (Screen.width % 16 == 0 && Screen.height % 9 == 0 && Screen.width / 16 == Screen.height / 9)
			{
				c.rect = new Rect(0f, 0f, 1f, 1f);
				flag = true;
			}
			else if (num6 > aSPECT)
			{
				num7 = 1f - aSPECT / num6;
			}
			else if (num6 < aSPECT)
			{
				num8 = 1f - num6 / aSPECT;
			}
			if (!flag)
			{
				c.rect = new Rect(num7 / 2f, num8 / 2f, 1f - num7, 1f - num8);
			}
		}
		float displaySafeArea = GameManager.Options.DisplaySafeArea;
		displaySafeArea = Mathf.Clamp01(displaySafeArea);
		float width = c.rect.width;
		float height = c.rect.height;
		Rect rect2 = (c.rect = new Rect(c.rect.xMin + width * (0.5f * (1f - displaySafeArea)), c.rect.yMin + height * (0.5f * (1f - displaySafeArea)), width * displaySafeArea, height * displaySafeArea));
	}

	public static void MaintainCameraAspectForceAspect(Camera c, float forcedAspect)
	{
		c.transparencySortMode = TransparencySortMode.Orthographic;
		if (GameManager.Options == null || (GameManager.Options.CurrentPreferredScalingMode != GameOptions.PreferredScalingMode.UNIFORM_SCALING && GameManager.Options.CurrentPreferredScalingMode != GameOptions.PreferredScalingMode.UNIFORM_SCALING_FAST))
		{
			int a = Mathf.FloorToInt((float)Screen.width / 480f);
			int b = Mathf.FloorToInt((float)Screen.height / 270f);
			int num = Mathf.Max(1, Mathf.Min(a, b));
			int num2 = 480 * num;
			int num3 = 270 * num;
			float num4 = 1f - (float)num2 / (float)Screen.width;
			float num5 = 1f - (float)num3 / (float)Screen.height;
			c.rect = new Rect(num4 / 2f, num5 / 2f, 1f - num4, 1f - num5);
			if (m_cachedMultiple != num)
			{
				dfGUIManager.ForceResolutionUpdates();
			}
			m_cachedMultiple = num;
		}
		else
		{
			float num6 = (float)Screen.width / (float)Screen.height;
			float num7 = 0f;
			float num8 = 0f;
			bool flag = false;
			if (Screen.width % 16 == 0 && Screen.height % 9 == 0 && Screen.width / 16 == Screen.height / 9)
			{
				c.rect = new Rect(0f, 0f, 1f, 1f);
				flag = true;
			}
			else if (num6 > forcedAspect)
			{
				num7 = 1f - forcedAspect / num6;
			}
			else if (num6 < forcedAspect)
			{
				num8 = 1f - num6 / forcedAspect;
			}
			if (!flag)
			{
				c.rect = new Rect(num7 / 2f, num8 / 2f, 1f - num7, 1f - num8);
			}
		}
		float displaySafeArea = GameManager.Options.DisplaySafeArea;
		displaySafeArea = Mathf.Clamp01(displaySafeArea);
		float width = c.rect.width;
		float height = c.rect.height;
		Rect rect2 = (c.rect = new Rect(c.rect.xMin + width * (0.5f * (1f - displaySafeArea)), c.rect.yMin + height * (0.5f * (1f - displaySafeArea)), width * displaySafeArea, height * displaySafeArea));
	}

	public static Camera GenerateBackgroundCamera(Camera c)
	{
		Camera component = new GameObject("BackgroundCamera", typeof(Camera)).GetComponent<Camera>();
		component.transform.position = new Vector3(-1000f, -1000f, 0f);
		component.orthographic = true;
		component.orthographicSize = 1f;
		component.depth = -5f;
		component.clearFlags = CameraClearFlags.Color;
		component.backgroundColor = Color.black;
		component.cullingMask = -1;
		component.rect = new Rect(0f, 0f, 1f, 1f);
		return component;
	}
}
