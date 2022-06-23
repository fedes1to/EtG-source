using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TextBoxManager : MonoBehaviour
{
	public enum BoxSlideOrientation
	{
		NO_ADJUSTMENT,
		FORCE_RIGHT,
		FORCE_LEFT
	}

	public static bool TIME_INVARIANT = false;

	private const float TEXT_REVEAL_SPEED_INSTANT = float.MaxValue;

	private const float TEXT_REVEAL_SPEED_FAST = 100f;

	private const float TEXT_REVEAL_SPEED_SLOW = 27f;

	private const float SCALE_UP_TIME = 0.06f;

	private const float SCALE_DOWN_TIME = 0.06f;

	[SerializeField]
	private tk2dSlicedSprite boxSprite;

	[SerializeField]
	private tk2dTextMesh textMesh;

	[SerializeField]
	private tk2dTextMesh continueTextMesh;

	public float additionalPaddingLeft;

	public float additionalPaddingRight;

	public float additionalPaddingTop;

	public float additionalPaddingBottom;

	public float continuePaddingRight;

	public float continuePaddingBottom;

	public bool fitToScreen;

	public Color textColor = Color.black;

	private static float BOX_PADDING = 0.5f;

	private static float INFOBOX_PADDING = 0.25f;

	private bool m_isRevealingText;

	private bool skipTextReveal;

	private string audioTag = string.Empty;

	private float boxPadding;

	private Vector3 m_basePosition;

	private Transform boxSpriteTransform;

	private Transform textMeshTransform;

	private Transform continueTextMeshTransform;

	private static List<Transform> extantTextPointList = new List<Transform>();

	private static Dictionary<Transform, GameObject> extantTextBoxMap = new Dictionary<Transform, GameObject>();

	private static int UNPIXELATED_LAYER = -1;

	private static int PIXELATED_LAYER = -1;

	private float TEXT_REVEAL_SPEED
	{
		get
		{
			switch (GameManager.Options.TextSpeed)
			{
			case GameOptions.GenericHighMedLowOption.HIGH:
				return float.MaxValue;
			case GameOptions.GenericHighMedLowOption.MEDIUM:
				return 100f;
			case GameOptions.GenericHighMedLowOption.LOW:
				return 27f;
			default:
				return 100f;
			}
		}
	}

	public static float ZombieBoxMultiplier
	{
		get
		{
			switch (GameManager.Options.TextSpeed)
			{
			case GameOptions.GenericHighMedLowOption.HIGH:
				return 1f;
			case GameOptions.GenericHighMedLowOption.MEDIUM:
				return 1.5f;
			case GameOptions.GenericHighMedLowOption.LOW:
				return 2.5f;
			default:
				return 1f;
			}
		}
	}

	public bool IsRevealingText
	{
		get
		{
			return m_isRevealingText;
		}
	}

	public bool IsScalingUp { get; set; }

	public bool IsScalingDown { get; set; }

	public static int ExtantTextBoxCount
	{
		get
		{
			return extantTextBoxMap.Count;
		}
	}

	public static bool ExtantTextBoxVisible
	{
		get
		{
			if (extantTextBoxMap == null || extantTextBoxMap.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < extantTextPointList.Count; i++)
			{
				if (!extantTextPointList[i])
				{
					extantTextPointList.RemoveAt(i);
					i--;
				}
				else if (GameManager.Instance.MainCameraController.PointIsVisible(extantTextPointList[i].position.XY()))
				{
					return true;
				}
			}
			return false;
		}
	}

	private float ScaleFactor
	{
		get
		{
			return Mathf.Max(1, Mathf.FloorToInt(1f / GameManager.Instance.MainCameraController.CurrentZoomScale));
		}
	}

	public static void ClearPerLevelData()
	{
		extantTextBoxMap.Clear();
	}

	public static void ShowTextBox(Vector3 worldPosition, Transform parent, float duration, string text, string audioTag = "", bool instant = true, BoxSlideOrientation slideOrientation = BoxSlideOrientation.NO_ADJUSTMENT, bool showContinueText = false, bool useAlienLanguage = false)
	{
		ShowBoxInternal(worldPosition, parent, duration, text, "TextBox", BOX_PADDING, audioTag, instant, slideOrientation, showContinueText, useAlienLanguage);
	}

	public static void ShowInfoBox(Vector3 worldPosition, Transform parent, float duration, string text, bool instant = true, bool showContinueText = false)
	{
		ShowBoxInternal(worldPosition, parent, duration, text, "InfoBox", INFOBOX_PADDING, string.Empty, instant, BoxSlideOrientation.NO_ADJUSTMENT, showContinueText);
	}

	public static void ShowLetterBox(Vector3 worldPosition, Transform parent, float duration, string text, bool instant = true, bool showContinueText = false)
	{
		ShowBoxInternal(worldPosition, parent, duration, text, "LetterBox", BOX_PADDING, string.Empty, instant, BoxSlideOrientation.NO_ADJUSTMENT, showContinueText);
	}

	public static void ShowStoneTablet(Vector3 worldPosition, Transform parent, float duration, string text, bool instant = true, bool showContinueText = false)
	{
		ShowBoxInternal(worldPosition, parent, duration, text, "StoneTablet", BOX_PADDING, string.Empty, instant, BoxSlideOrientation.NO_ADJUSTMENT, showContinueText);
	}

	public static void ShowWoodPanel(Vector3 worldPosition, Transform parent, float duration, string text, bool instant = true, bool showContinueText = false)
	{
		ShowBoxInternal(worldPosition, parent, duration, text, "WoodPanel", BOX_PADDING, string.Empty, instant, BoxSlideOrientation.NO_ADJUSTMENT, showContinueText);
	}

	public static void ShowThoughtBubble(Vector3 worldPosition, Transform parent, float duration, string text, bool instant = true, bool showContinueText = false, string overrideAudioTag = "")
	{
		ShowBoxInternal(worldPosition, parent, duration, text, "ThoughtBubble", BOX_PADDING, overrideAudioTag, instant, BoxSlideOrientation.NO_ADJUSTMENT, showContinueText);
	}

	public static void ShowNote(Vector3 worldPosition, Transform parent, float duration, string text, bool instant = true, bool showContinueText = false)
	{
		ShowBoxInternal(worldPosition, parent, duration, text, "Note", BOX_PADDING, string.Empty, instant, BoxSlideOrientation.NO_ADJUSTMENT, showContinueText);
	}

	public static bool TextBoxCanBeAdvanced(Transform parent)
	{
		if (extantTextBoxMap.ContainsKey(parent))
		{
			GameObject gameObject = extantTextBoxMap[parent];
			TextBoxManager component = gameObject.GetComponent<TextBoxManager>();
			return component.IsRevealingText;
		}
		return false;
	}

	public static void AdvanceTextBox(Transform parent)
	{
		if (extantTextBoxMap.ContainsKey(parent))
		{
			GameObject gameObject = extantTextBoxMap[parent];
			TextBoxManager component = gameObject.GetComponent<TextBoxManager>();
			component.SkipTextReveal();
		}
	}

	protected static void ShowBoxInternal(Vector3 worldPosition, Transform parent, float duration, string text, string prefabName, float padding, string audioTag, bool instant, BoxSlideOrientation slideOrientation, bool showContinueText, bool UseAlienLanguage = false)
	{
		Vector2 prevBoxSize = new Vector2(-1f, -1f);
		if (parent != null && extantTextBoxMap.ContainsKey(parent))
		{
			prevBoxSize = extantTextBoxMap[parent].GetComponent<TextBoxManager>().boxSprite.dimensions;
			UnityEngine.Object.Destroy(extantTextBoxMap[parent]);
			extantTextPointList.Remove(parent);
			extantTextBoxMap.Remove(parent);
		}
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load(prefabName));
		TextBoxManager component = gameObject.GetComponent<TextBoxManager>();
		component.boxPadding = padding;
		component.IsScalingUp = true;
		component.audioTag = audioTag;
		component.SetText(text, worldPosition, instant, slideOrientation, true, UseAlienLanguage, prefabName == "ThoughtBubble");
		if (parent != null)
		{
			component.transform.parent = parent;
			extantTextPointList.Add(parent);
			extantTextBoxMap.Add(parent, gameObject);
		}
		if (duration >= 0f)
		{
			component.HandleLifespan(gameObject, parent, duration);
		}
		if (showContinueText)
		{
			component.ShowContinueText();
		}
		component.StartCoroutine(component.HandleScaleUp(prevBoxSize));
	}

	private IEnumerator HandleScaleUp(Vector2 prevBoxSize)
	{
		IsScalingUp = true;
		if (prevBoxSize.x <= 0f || prevBoxSize.y <= 0f)
		{
			Transform targetTransform = base.transform;
			float elapsed = 0f;
			float duration = 0.06f;
			while (elapsed < duration)
			{
				elapsed += GameManager.INVARIANT_DELTA_TIME;
				targetTransform.localScale = Vector3.Lerp(new Vector3(0.01f, 0.01f, 1f), Vector3.one * ScaleFactor, Mathf.SmoothStep(0f, 1f, elapsed / duration));
				yield return null;
			}
		}
		else
		{
			Vector2 targetdimensions = boxSprite.dimensions;
			float elapsed2 = 0f;
			float durationModifier = Mathf.Clamp01(((targetdimensions - prevBoxSize).magnitude - 5f) / 10f);
			float duration2 = Mathf.Lerp(0.025f, 0.06f, durationModifier);
			while (elapsed2 < duration2)
			{
				elapsed2 += GameManager.INVARIANT_DELTA_TIME;
				boxSprite.dimensions = Vector2.Lerp(prevBoxSize, targetdimensions, Mathf.SmoothStep(0f, 1f, elapsed2 / duration2));
				yield return null;
			}
		}
		IsScalingUp = false;
	}

	private IEnumerator HandleScaleDown()
	{
		IsScalingDown = true;
		Transform targetTransform = base.transform;
		float elapsed = 0f;
		float duration = 0.06f;
		Vector3 startScale = targetTransform.localScale;
		while (elapsed < duration)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			if (!targetTransform)
			{
				yield break;
			}
			targetTransform.localScale = Vector3.Lerp(startScale, new Vector3(0.01f, 0.01f, 1f), Mathf.SmoothStep(0f, 1f, elapsed / duration));
			yield return null;
		}
		if ((bool)this && (bool)base.gameObject)
		{
			IsScalingDown = false;
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public static bool HasTextBox(Transform parent)
	{
		return extantTextBoxMap.ContainsKey(parent);
	}

	public static void ClearTextBoxImmediate(Transform parent)
	{
		if (extantTextBoxMap.ContainsKey(parent))
		{
			extantTextPointList.Remove(parent);
			extantTextBoxMap.Remove(parent);
		}
		TextBoxManager componentInChildren = parent.GetComponentInChildren<TextBoxManager>();
		if ((bool)componentInChildren)
		{
			UnityEngine.Object.Destroy(componentInChildren.gameObject);
		}
	}

	public static void ClearTextBox(Transform parent)
	{
		if (extantTextBoxMap.ContainsKey(parent))
		{
			TextBoxManager component = extantTextBoxMap[parent].GetComponent<TextBoxManager>();
			component.StartCoroutine(component.HandleScaleDown());
			extantTextPointList.Remove(parent);
			extantTextBoxMap.Remove(parent);
		}
	}

	public void HandleLifespan(GameObject target, Transform parent, float lifespan)
	{
		StartCoroutine(TextBoxLifespanCR(target, parent, lifespan));
	}

	public void ShowContinueText()
	{
		if ((bool)continueTextMesh)
		{
			StartCoroutine(ShowContinueTextCR());
		}
	}

	public void SkipTextReveal()
	{
		skipTextReveal = true;
	}

	private IEnumerator TextBoxLifespanCR(GameObject target, Transform parent, float lifespan)
	{
		yield return null;
		while (m_isRevealingText)
		{
			yield return null;
		}
		yield return new WaitForSeconds(lifespan);
		if (parent != null)
		{
			ClearTextBox(parent);
		}
		else
		{
			UnityEngine.Object.Destroy(target);
		}
	}

	private IEnumerator ShowContinueTextCR()
	{
		float delay = 0.3f;
		while (true)
		{
			continueTextMesh.text = ".";
			continueTextMesh.Commit();
			yield return new WaitForSeconds(delay);
			continueTextMesh.text = "..";
			continueTextMesh.Commit();
			yield return new WaitForSeconds(delay);
			continueTextMesh.text = "...";
			continueTextMesh.Commit();
			yield return new WaitForSeconds(delay * 2f);
			continueTextMesh.text = string.Empty;
			continueTextMesh.Commit();
			yield return new WaitForSeconds(delay);
		}
	}

	private IEnumerator RevealTextCharacters(string strippedString)
	{
		m_isRevealingText = true;
		skipTextReveal = false;
		while (IsScalingUp)
		{
			yield return null;
		}
		float elapsed = 0f;
		float duration = (float)strippedString.Length / TEXT_REVEAL_SPEED;
		if (TEXT_REVEAL_SPEED > 10000f)
		{
			duration = 0f;
		}
		Renderer boxRenderer = boxSpriteTransform.GetComponent<Renderer>();
		textMesh.inlineStyling = true;
		textMesh.color = Color.black;
		textMesh.visibleCharacters = 0;
		int visibleCharacters = 0;
		if (duration > 0f)
		{
			while (elapsed < duration)
			{
				elapsed += ((!TIME_INVARIANT) ? BraveTime.DeltaTime : GameManager.INVARIANT_DELTA_TIME);
				if (skipTextReveal)
				{
					elapsed = duration;
				}
				float t = elapsed / duration;
				int numCharacters = Mathf.FloorToInt((float)strippedString.Length * t);
				if (numCharacters > 100000)
				{
					numCharacters = 0;
				}
				if (numCharacters > visibleCharacters && boxRenderer.isVisible)
				{
					visibleCharacters = numCharacters;
					if (!string.IsNullOrEmpty(audioTag))
					{
						AkSoundEngine.PostEvent("Play_CHR_" + audioTag + "_voice_01", base.gameObject);
					}
				}
				textMesh.visibleCharacters = visibleCharacters;
				textMesh.text = strippedString;
				textMesh.Commit();
				yield return null;
			}
		}
		textMesh.visibleCharacters = int.MaxValue;
		textMesh.text = strippedString;
		textMesh.Commit();
		m_isRevealingText = false;
		skipTextReveal = false;
	}

	private void LateUpdate()
	{
		UNPIXELATED_LAYER = ((!Pixelator.Instance.DoFinalNonFadedLayer) ? LayerMask.NameToLayer("Unoccluded") : LayerMask.NameToLayer("Unfaded"));
		if (PIXELATED_LAYER == -1)
		{
			PIXELATED_LAYER = LayerMask.NameToLayer("FG_Critical");
		}
		if (GameManager.Instance.IsPaused && base.gameObject.layer == UNPIXELATED_LAYER)
		{
			base.gameObject.SetLayerRecursively(PIXELATED_LAYER);
		}
		else if (!GameManager.Instance.IsPaused && base.gameObject.layer != UNPIXELATED_LAYER)
		{
			base.gameObject.SetLayerRecursively(UNPIXELATED_LAYER);
		}
		UpdateForCameraPosition();
	}

	public void UpdateForCameraPosition()
	{
		if (fitToScreen)
		{
			Vector3 vector = base.transform.position - m_basePosition;
			Vector2 vector2 = boxSprite.transform.position.XY() - vector.XY();
			Vector2 vector3 = boxSprite.transform.position.XY() + boxSprite.dimensions / 16f - vector.XY();
			Camera component = GameManager.Instance.MainCameraController.GetComponent<Camera>();
			Vector2 vector4 = component.WorldToViewportPoint(vector2.ToVector3ZUp(vector2.y));
			Vector2 vector5 = component.WorldToViewportPoint(vector3.ToVector3ZUp(vector3.y));
			float num = Mathf.Min(vector4.x, 0.1f) + Mathf.Max(vector5.x - 1f, -0.1f);
			float num2 = Mathf.Min(vector4.y, 0.1f) + Mathf.Max(vector5.y - 1f, -0.1f);
			float num3 = num * 480f / 16f;
			float num4 = num2 * 270f / 16f;
			base.transform.position = (m_basePosition + new Vector3(0f - num3, 0f - num4, 0f)).Quantize(0.0625f);
			if (!IsScalingUp && !IsScalingDown)
			{
				base.transform.localScale = Vector3.one * ScaleFactor;
			}
		}
	}

	private string ToUpperExcludeSprites(string inputString)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		for (int i = 0; i < inputString.Length; i++)
		{
			char c = inputString[i];
			if (c == '[' && !flag)
			{
				flag = true;
			}
			else if (c == ']' && flag)
			{
				flag = false;
			}
			else if (!flag)
			{
				c = char.ToUpperInvariant(c);
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}

	public void SetText(string text, Vector3 worldPosition, bool instant = true, BoxSlideOrientation slideOrientation = BoxSlideOrientation.NO_ADJUSTMENT, bool showContinueText = true, bool UseAlienLanguage = false, bool clampThoughtBubble = false)
	{
		if (boxSpriteTransform == null)
		{
			boxSpriteTransform = boxSprite.transform;
		}
		if (textMeshTransform == null)
		{
			textMeshTransform = textMesh.transform;
		}
		if (continueTextMeshTransform == null && (bool)continueTextMesh)
		{
			continueTextMeshTransform = continueTextMesh.transform;
		}
		if (text == string.Empty)
		{
			return;
		}
		text = text.Replace("\\n", Environment.NewLine);
		float x = boxSpriteTransform.localPosition.x;
		float num = (0f - x) / (boxSprite.dimensions.x / 16f);
		string text2 = textMesh.GetStrippedWoobleString(text);
		if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE)
		{
			textMesh.LineSpacing = 0.125f;
		}
		else
		{
			textMesh.LineSpacing = 0f;
		}
		if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.JAPANESE)
		{
			textMesh.wordWrapWidth = 350;
		}
		else if (text2.Length < 25)
		{
			textMesh.wordWrapWidth = 250;
		}
		else
		{
			textMesh.wordWrapWidth = 200 + (text2.Length - 25) / 4;
			if (!text2.EndsWith(" "))
			{
				text2 += " ";
			}
		}
		if (Application.isPlaying)
		{
			bool flag = false;
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				flag |= GameManager.Instance.AllPlayers[i].UnderstandsGleepGlorp;
			}
			if (UseAlienLanguage && !flag)
			{
				text2 = ToUpperExcludeSprites(text2);
				textMesh.font = GameManager.Instance.DefaultAlienConversationFont;
			}
			else
			{
				textMesh.font = GameManager.Instance.DefaultNormalConversationFont;
			}
		}
		textMesh.text = text2;
		textMesh.CheckFontsForLanguage();
		textMesh.ForceBuild();
		Bounds trueBounds = textMesh.GetTrueBounds();
		float num2 = Mathf.Ceil((trueBounds.size.x + boxPadding * 2f + additionalPaddingLeft + additionalPaddingRight) * 16f) / 16f;
		float num3 = Mathf.Ceil((trueBounds.size.y + boxPadding * 2f + additionalPaddingTop + additionalPaddingBottom) * 16f) / 16f;
		if (showContinueText && (bool)continueTextMesh)
		{
			num2 += continueTextMesh.GetEstimatedMeshBoundsForString("...").extents.x * 2f;
		}
		float num4 = num2 * 16f;
		float a = num3 * 16f;
		if (clampThoughtBubble)
		{
			float num5 = 47f + (Mathf.Max(47f, num4) - 47f).Quantize(23f, VectorConversions.Floor);
			float num6 = 57f + (Mathf.Max(57f, num4) - 57f).Quantize(23f, VectorConversions.Floor);
			if (num5 < num4)
			{
				num5 += 23f;
			}
			if (num6 < num4)
			{
				num6 += 23f;
			}
			float num7 = Mathf.Abs(num5 - num4);
			float num8 = Mathf.Abs(num6 - num4);
			num4 = ((!(num7 < num8)) ? num6 : num5);
		}
		Vector3 lhs = new Vector3(0f, 0f);
		tk2dSpriteDefinition currentSpriteDef = boxSprite.GetCurrentSpriteDef();
		Vector3 boundsDataExtents = currentSpriteDef.boundsDataExtents;
		if (currentSpriteDef.texelSize.x != 0f && currentSpriteDef.texelSize.y != 0f && boundsDataExtents.x != 0f && boundsDataExtents.y != 0f)
		{
			lhs = new Vector3(boundsDataExtents.x / currentSpriteDef.texelSize.x, boundsDataExtents.y / currentSpriteDef.texelSize.y, 1f);
		}
		lhs = Vector3.Max(lhs, Vector3.one);
		num4 = Mathf.Max(num4, (boxSprite.borderLeft + boxSprite.borderRight) * lhs.x);
		a = Mathf.Max(a, (boxSprite.borderTop + boxSprite.borderBottom) * lhs.y);
		boxSprite.dimensions = new Vector2(num4, a);
		if (boxSprite.dimensions.x < (boxSprite.borderLeft + boxSprite.borderRight) * lhs.x || boxSprite.dimensions.y < (boxSprite.borderTop + boxSprite.borderBottom) * lhs.y)
		{
			boxSprite.BorderOnly = true;
		}
		else
		{
			boxSprite.BorderOnly = false;
		}
		boxSprite.ForceBuild();
		textMesh.color = textColor;
		if (instant)
		{
			textMesh.text = textMesh.PreprocessWoobleSignifiers(text);
			if (UseAlienLanguage)
			{
				textMesh.text = ToUpperExcludeSprites(textMesh.text);
			}
			textMesh.Commit();
		}
		else
		{
			textMesh.text = string.Empty;
			textMesh.Commit();
			string text3 = textMesh.PreprocessWoobleSignifiers(text);
			if (UseAlienLanguage)
			{
				text3 = ToUpperExcludeSprites(text3);
			}
			StartCoroutine(RevealTextCharacters(text3));
		}
		float y = BraveMathCollege.QuantizeFloat(boxSprite.dimensions.y / 16f - boxPadding - additionalPaddingTop, 0.0625f);
		if (textMesh.anchor == TextAnchor.UpperLeft)
		{
			textMeshTransform.localPosition = new Vector3(boxPadding + additionalPaddingLeft, y, -0.1f);
		}
		else if (textMesh.anchor == TextAnchor.UpperCenter)
		{
			textMeshTransform.localPosition = new Vector3(num2 / 2f, y, -0.1f);
		}
		textMeshTransform.localPosition += new Vector3(3f / 128f, 3f / 128f, 0f);
		if ((bool)continueTextMesh)
		{
			if (showContinueText)
			{
				Bounds estimatedMeshBoundsForString = continueTextMesh.GetEstimatedMeshBoundsForString("...");
				continueTextMeshTransform.localPosition = new Vector3(num2 - continuePaddingRight - estimatedMeshBoundsForString.extents.x * 2f, continuePaddingBottom, -0.1f);
			}
			else
			{
				continueTextMesh.text = string.Empty;
				continueTextMesh.Commit();
			}
		}
		switch (slideOrientation)
		{
		case BoxSlideOrientation.NO_ADJUSTMENT:
			boxSpriteTransform.localPosition = boxSpriteTransform.localPosition.WithX(BraveMathCollege.QuantizeFloat(-1f * num * (boxSprite.dimensions.x / 16f), 0.0625f));
			break;
		case BoxSlideOrientation.FORCE_RIGHT:
			num = 0.1f;
			boxSpriteTransform.localPosition = boxSpriteTransform.localPosition.WithX(BraveMathCollege.QuantizeFloat(-1f * num * (boxSprite.dimensions.x / 16f), 0.0625f));
			break;
		case BoxSlideOrientation.FORCE_LEFT:
			num = 0.85f;
			boxSpriteTransform.localPosition = boxSpriteTransform.localPosition.WithX(BraveMathCollege.QuantizeFloat(-1f * num * (boxSprite.dimensions.x / 16f), 0.0625f));
			break;
		default:
			boxSpriteTransform.localPosition = boxSpriteTransform.localPosition.WithX(BraveMathCollege.QuantizeFloat(-1f * num * (boxSprite.dimensions.x / 16f), 0.0625f));
			break;
		}
		if (slideOrientation == BoxSlideOrientation.NO_ADJUSTMENT)
		{
			float num9 = ((!Application.isPlaying) ? 0f : GameManager.Instance.MainCameraController.transform.position.x);
			if (worldPosition.x > num9)
			{
				boxSpriteTransform.localPosition = boxSpriteTransform.localPosition.WithX(-1f * (1f - num) * (boxSprite.dimensions.x / 16f));
			}
		}
		base.transform.position = worldPosition;
		base.transform.localScale = Vector3.one * ScaleFactor;
		m_basePosition = worldPosition;
		UpdateForCameraPosition();
		base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Unoccluded"));
	}

	public static float GetEstimatedReadingTime(string text)
	{
		int num = 0;
		bool flag = false;
		foreach (char c in text)
		{
			switch (c)
			{
			case '[':
			case '{':
				flag = true;
				break;
			case ']':
			case '}':
				flag = false;
				break;
			}
			if (!flag && !char.IsWhiteSpace(c))
			{
				num++;
			}
		}
		int num2 = 987;
		switch (GameManager.Options.CurrentLanguage)
		{
		case StringTableManager.GungeonSupportedLanguages.CHINESE:
			num2 = 357;
			break;
		case StringTableManager.GungeonSupportedLanguages.JAPANESE:
			num2 = 357;
			break;
		case StringTableManager.GungeonSupportedLanguages.KOREAN:
			num2 = 357;
			break;
		case StringTableManager.GungeonSupportedLanguages.BRAZILIANPORTUGUESE:
			num2 = 913;
			break;
		case StringTableManager.GungeonSupportedLanguages.POLISH:
			num2 = 916;
			break;
		case StringTableManager.GungeonSupportedLanguages.GERMAN:
			num2 = 920;
			break;
		case StringTableManager.GungeonSupportedLanguages.RUSSIAN:
			num2 = 986;
			break;
		case StringTableManager.GungeonSupportedLanguages.ENGLISH:
			num2 = 987;
			break;
		case StringTableManager.GungeonSupportedLanguages.FRENCH:
			num2 = 998;
			break;
		case StringTableManager.GungeonSupportedLanguages.RUBEL_TEST:
			num2 = 1000;
			break;
		case StringTableManager.GungeonSupportedLanguages.SPANISH:
			num2 = 1025;
			break;
		}
		return (float)num / ((float)num2 / 60f);
	}
}
