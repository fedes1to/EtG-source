using System;
using System.Collections.Generic;
using System.Text;
using tk2dRuntime;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[AddComponentMenu("2D Toolkit/Text/tk2dTextMesh")]
[ExecuteInEditMode]
public class tk2dTextMesh : MonoBehaviour, ISpriteCollectionForceBuild
{
	public enum WoobleStyle
	{
		SEQUENTIAL,
		SIMULTANEOUS,
		RANDOM_JITTER,
		RANDOM_SEQUENTIAL,
		SEQUENTIAL_RAINBOW
	}

	internal class WoobleDefinition
	{
		public int startIndex = -1;

		public int endIndex = -1;

		public WoobleStyle style;
	}

	[Flags]
	private enum UpdateFlags
	{
		UpdateNone = 0,
		UpdateText = 1,
		UpdateColors = 2,
		UpdateBuffers = 4
	}

	private tk2dFontData _fontInst;

	private string _formattedText = string.Empty;

	[SerializeField]
	private tk2dFontData _font;

	[SerializeField]
	private string _text = string.Empty;

	[SerializeField]
	private Color _color = Color.white;

	[SerializeField]
	private Color _color2 = Color.white;

	[SerializeField]
	private bool _useGradient;

	[SerializeField]
	private int _textureGradient;

	[SerializeField]
	private TextAnchor _anchor = TextAnchor.LowerLeft;

	[SerializeField]
	private Vector3 _scale = new Vector3(1f, 1f, 1f);

	[SerializeField]
	private bool _kerning;

	[SerializeField]
	private int _maxChars = 16;

	[SerializeField]
	private bool _inlineStyling;

	[SerializeField]
	public bool supportsWooblyText;

	[SerializeField]
	public int[] woobleStartIndices;

	[SerializeField]
	public int[] woobleEndIndices;

	[SerializeField]
	public WoobleStyle[] woobleStyles;

	public int visibleCharacters = int.MaxValue;

	[SerializeField]
	private bool _formatting;

	[SerializeField]
	private int _wordWrapWidth;

	[SerializeField]
	private float spacing;

	[SerializeField]
	private float lineSpacing;

	[SerializeField]
	private tk2dTextMeshData data = new tk2dTextMeshData();

	private Vector3[] vertices;

	private Vector2[] uvs;

	private Vector2[] uv2;

	private Color32[] colors;

	private Color32[] untintedColors;

	private UpdateFlags updateFlags = UpdateFlags.UpdateBuffers;

	private Mesh mesh;

	private MeshFilter meshFilter;

	private Renderer _cachedRenderer;

	protected Vector2[] woobleBuffer;

	protected bool[] woobleRainbowBuffer;

	protected float[] woobleTimes;

	protected List<int> indices;

	private List<GameObject> m_inlineSprites = new List<GameObject>();

	private List<int> m_inlineSpriteIndices = new List<int>();

	private tk2dFontData m_defaultAssignedFont;

	public string FormattedText
	{
		get
		{
			return _formattedText;
		}
	}

	public tk2dFontData font
	{
		get
		{
			UpgradeData();
			return data.font;
		}
		set
		{
			UpgradeData();
			data.font = value;
			_fontInst = data.font.inst;
			SetNeedUpdate(UpdateFlags.UpdateText);
			UpdateMaterial();
		}
	}

	public bool formatting
	{
		get
		{
			UpgradeData();
			return data.formatting;
		}
		set
		{
			UpgradeData();
			if (data.formatting != value)
			{
				data.formatting = value;
				SetNeedUpdate(UpdateFlags.UpdateText);
			}
		}
	}

	public int wordWrapWidth
	{
		get
		{
			UpgradeData();
			return data.wordWrapWidth;
		}
		set
		{
			UpgradeData();
			if (data.wordWrapWidth != value)
			{
				data.wordWrapWidth = value;
				SetNeedUpdate(UpdateFlags.UpdateText);
			}
		}
	}

	public string text
	{
		get
		{
			UpgradeData();
			return data.text;
		}
		set
		{
			UpgradeData();
			data.text = value;
			SetNeedUpdate(UpdateFlags.UpdateText);
		}
	}

	public Color color
	{
		get
		{
			UpgradeData();
			return data.color;
		}
		set
		{
			UpgradeData();
			data.color = value;
			SetNeedUpdate(UpdateFlags.UpdateColors);
		}
	}

	public Color color2
	{
		get
		{
			UpgradeData();
			return data.color2;
		}
		set
		{
			UpgradeData();
			data.color2 = value;
			SetNeedUpdate(UpdateFlags.UpdateColors);
		}
	}

	public bool useGradient
	{
		get
		{
			UpgradeData();
			return data.useGradient;
		}
		set
		{
			UpgradeData();
			data.useGradient = value;
			SetNeedUpdate(UpdateFlags.UpdateColors);
		}
	}

	public TextAnchor anchor
	{
		get
		{
			UpgradeData();
			return data.anchor;
		}
		set
		{
			UpgradeData();
			data.anchor = value;
			SetNeedUpdate(UpdateFlags.UpdateText);
		}
	}

	public Vector3 scale
	{
		get
		{
			UpgradeData();
			return data.scale;
		}
		set
		{
			UpgradeData();
			data.scale = value;
			SetNeedUpdate(UpdateFlags.UpdateText);
		}
	}

	public bool kerning
	{
		get
		{
			UpgradeData();
			return data.kerning;
		}
		set
		{
			UpgradeData();
			data.kerning = value;
			SetNeedUpdate(UpdateFlags.UpdateText);
		}
	}

	public int maxChars
	{
		get
		{
			UpgradeData();
			return data.maxChars;
		}
		set
		{
			UpgradeData();
			data.maxChars = value;
			SetNeedUpdate(UpdateFlags.UpdateBuffers);
		}
	}

	public int textureGradient
	{
		get
		{
			UpgradeData();
			return data.textureGradient;
		}
		set
		{
			UpgradeData();
			data.textureGradient = value % font.gradientCount;
			SetNeedUpdate(UpdateFlags.UpdateText);
		}
	}

	public bool inlineStyling
	{
		get
		{
			UpgradeData();
			return data.inlineStyling;
		}
		set
		{
			UpgradeData();
			data.inlineStyling = value;
			SetNeedUpdate(UpdateFlags.UpdateText);
		}
	}

	public float Spacing
	{
		get
		{
			UpgradeData();
			return data.spacing;
		}
		set
		{
			UpgradeData();
			if (data.spacing != value)
			{
				data.spacing = value;
				SetNeedUpdate(UpdateFlags.UpdateText);
			}
		}
	}

	public float LineSpacing
	{
		get
		{
			UpgradeData();
			return data.lineSpacing;
		}
		set
		{
			UpgradeData();
			if (data.lineSpacing != value)
			{
				data.lineSpacing = value;
				SetNeedUpdate(UpdateFlags.UpdateText);
			}
		}
	}

	public int SortingOrder
	{
		get
		{
			return CachedRenderer.sortingOrder;
		}
		set
		{
			if (CachedRenderer.sortingOrder != value)
			{
				data.renderLayer = value;
				CachedRenderer.sortingOrder = value;
			}
		}
	}

	private Renderer CachedRenderer
	{
		get
		{
			if (_cachedRenderer == null)
			{
				_cachedRenderer = GetComponent<Renderer>();
			}
			return _cachedRenderer;
		}
	}

	private bool useInlineStyling
	{
		get
		{
			return inlineStyling && _fontInst.textureGradients;
		}
	}

	private void UpgradeData()
	{
		if (data.version != 1)
		{
			data.font = _font;
			data.text = _text;
			data.color = _color;
			data.color2 = _color2;
			data.useGradient = _useGradient;
			data.textureGradient = _textureGradient;
			data.anchor = _anchor;
			data.scale = _scale;
			data.kerning = _kerning;
			data.maxChars = _maxChars;
			data.inlineStyling = _inlineStyling;
			data.formatting = _formatting;
			data.wordWrapWidth = _wordWrapWidth;
			data.spacing = spacing;
			data.lineSpacing = lineSpacing;
		}
		data.version = 1;
	}

	private static int GetInlineStyleCommandLength(int cmdSymbol)
	{
		int result = 0;
		switch (cmdSymbol)
		{
		case 99:
			result = 5;
			break;
		case 67:
			result = 9;
			break;
		case 103:
			result = 9;
			break;
		case 71:
			result = 17;
			break;
		}
		return result;
	}

	public string FormatText(string unformattedString)
	{
		string _targetString = string.Empty;
		FormatText(ref _targetString, unformattedString);
		return _targetString;
	}

	private void FormatText()
	{
		FormatText(ref _formattedText, data.text);
	}

	public string GetStrippedWoobleString(string _source)
	{
		for (int i = 0; i < _source.Length; i++)
		{
			if (_source[i] != '{' || _source[i + 1] != 'w' || _source[i + 3] != '}')
			{
				continue;
			}
			int num = -1;
			for (int j = i + 3; j < _source.Length; j++)
			{
				if (_source[j] == '{' && _source[j + 1] == 'w')
				{
					num = j - 5;
					_source = _source.Remove(j, 3);
					break;
				}
			}
			if (num != -1)
			{
				_source = _source.Remove(i, 4);
			}
		}
		FormatText(ref _formattedText, _source, true);
		return _source;
	}

	public string PreprocessWoobleSignifiers(string _source)
	{
		List<WoobleDefinition> list = new List<WoobleDefinition>();
		for (int i = 0; i < _source.Length; i++)
		{
			if (_source[i] != '{' || _source[i + 1] != 'w' || _source[i + 3] != '}')
			{
				continue;
			}
			int num = -1;
			for (int j = i + 3; j < _source.Length; j++)
			{
				if (_source[j] == '{' && _source[j + 1] == 'w')
				{
					num = j - 5;
					_source = _source.Remove(j, 3);
					break;
				}
			}
			if (num != -1)
			{
				string text = _source.Substring(i, 4);
				_source = _source.Remove(i, 4);
				char c = text[2];
				WoobleDefinition woobleDefinition = new WoobleDefinition();
				switch (c)
				{
				case 's':
					woobleDefinition.style = WoobleStyle.SIMULTANEOUS;
					break;
				case 'q':
					woobleDefinition.style = WoobleStyle.SEQUENTIAL;
					break;
				case 'r':
					woobleDefinition.style = WoobleStyle.RANDOM_SEQUENTIAL;
					break;
				case 'j':
					woobleDefinition.style = WoobleStyle.RANDOM_JITTER;
					break;
				case 'b':
					woobleDefinition.style = WoobleStyle.SEQUENTIAL_RAINBOW;
					break;
				}
				woobleDefinition.startIndex = i;
				woobleDefinition.endIndex = num;
				list.Add(woobleDefinition);
			}
		}
		woobleStartIndices = new int[list.Count];
		woobleEndIndices = new int[list.Count];
		woobleStyles = new WoobleStyle[list.Count];
		for (int k = 0; k < list.Count; k++)
		{
			woobleStartIndices[k] = list[k].startIndex;
			woobleEndIndices[k] = list[k].endIndex;
			woobleStyles[k] = list[k].style;
		}
		FormatText(ref _formattedText, _source, true);
		return _source;
	}

	private void PushbackWooblesByAmount(int newCharIndex, int amt, int max)
	{
		for (int i = 0; i < woobleStyles.Length; i++)
		{
			if (woobleStartIndices[i] >= newCharIndex)
			{
				woobleStartIndices[i] = Mathf.Min(woobleStartIndices[i] + amt, max);
			}
			if (woobleEndIndices[i] >= newCharIndex)
			{
				woobleEndIndices[i] = Mathf.Min(woobleEndIndices[i] + amt, max);
			}
		}
	}

	private void FormatText(ref string _targetString, string _source, bool doPushback = false)
	{
		InitInstance();
		if (!formatting || wordWrapWidth == 0 || _fontInst.texelSize == Vector2.zero)
		{
			_targetString = _source;
			return;
		}
		float num = _fontInst.texelSize.x * (float)wordWrapWidth;
		StringBuilder stringBuilder = new StringBuilder(_source.Length);
		float num2 = 0f;
		float num3 = 0f;
		int num4 = -1;
		int num5 = -1;
		bool flag = false;
		for (int i = 0; i < _source.Length; i++)
		{
			char c = _source[i];
			tk2dFontChar tk2dFontChar2 = null;
			bool flag2 = c == '^';
			bool flag3 = false;
			if (c == '[' && i < _source.Length - 1 && _source[i + 1] != ']')
			{
				for (int j = i; j < _source.Length; j++)
				{
					char c2 = _source[j];
					if (c2 != ']')
					{
						continue;
					}
					flag3 = true;
					int num6 = j - i + 1;
					string substringInsideBrackets = _source.Substring(i + 9, num6 - 10);
					tk2dFontChar2 = tk2dTextGeomGen.GetSpecificSpriteCharDef(substringInsideBrackets);
					for (int k = 0; k < num6; k++)
					{
						if (i + k < _source.Length)
						{
							stringBuilder.Append(_source[i + k]);
						}
					}
					i += num6 - 1;
					break;
				}
			}
			if (!flag3)
			{
				if (_fontInst.useDictionary)
				{
					if (!_fontInst.charDict.ContainsKey(c))
					{
						c = '\0';
					}
					tk2dFontChar2 = _fontInst.charDict[c];
				}
				else
				{
					if (c >= _fontInst.chars.Length)
					{
						c = '\0';
					}
					tk2dFontChar2 = _fontInst.chars[(uint)c];
				}
			}
			if (flag2)
			{
				c = '^';
			}
			if (flag)
			{
				flag = false;
				continue;
			}
			if (data.inlineStyling && c == '^' && i + 1 < _source.Length)
			{
				if (_source[i + 1] != '^')
				{
					int inlineStyleCommandLength = GetInlineStyleCommandLength(_source[i + 1]);
					int num7 = 1 + inlineStyleCommandLength;
					for (int l = 0; l < num7; l++)
					{
						if (i + l < _source.Length)
						{
							stringBuilder.Append(_source[i + l]);
						}
					}
					i += num7 - 1;
					continue;
				}
				flag = true;
				stringBuilder.Append('^');
			}
			switch (c)
			{
			case '\n':
				num2 = 0f;
				num3 = 0f;
				num4 = stringBuilder.Length;
				num5 = i;
				break;
			case ' ':
				num2 += (tk2dFontChar2.advance + data.spacing) * data.scale.x;
				num3 = num2;
				num4 = stringBuilder.Length;
				num5 = i;
				break;
			default:
				if (num2 + tk2dFontChar2.p1.x * data.scale.x > num)
				{
					if (num3 > 0f)
					{
						num3 = 0f;
						num2 = 0f;
						stringBuilder.Remove(num4 + 1, stringBuilder.Length - num4 - 1);
						stringBuilder.Append('\n');
						i = num5;
						if (doPushback)
						{
							PushbackWooblesByAmount(i, 1, _source.Length);
						}
						continue;
					}
					stringBuilder.Append('\n');
					if (doPushback)
					{
						PushbackWooblesByAmount(i, 1, _source.Length);
					}
					num2 = (tk2dFontChar2.advance + data.spacing) * data.scale.x;
				}
				else
				{
					num2 += (tk2dFontChar2.advance + data.spacing) * data.scale.x;
				}
				break;
			}
			if (!flag3)
			{
				stringBuilder.Append(c);
			}
		}
		_targetString = stringBuilder.ToString();
	}

	private void SetNeedUpdate(UpdateFlags uf)
	{
		if (updateFlags == UpdateFlags.UpdateNone)
		{
			updateFlags |= uf;
			tk2dUpdateManager.QueueCommit(this);
		}
		else
		{
			updateFlags |= uf;
		}
	}

	private void InitInstance()
	{
		if (_fontInst == null && data.font != null)
		{
			_fontInst = data.font.inst;
		}
	}

	private void Awake()
	{
		UpgradeData();
		if (data.font != null)
		{
			_fontInst = data.font.inst;
		}
		updateFlags = UpdateFlags.UpdateBuffers;
		if (data.font != null)
		{
			Init();
			UpdateMaterial();
		}
		updateFlags = UpdateFlags.UpdateNone;
	}

	private void Update()
	{
		if (supportsWooblyText && Application.isPlaying)
		{
			UpdateWooblyTextBuffers();
		}
	}

	protected void InitWooblyTextBuffers()
	{
		if (indices == null)
		{
			indices = new List<int>();
		}
		indices.Clear();
		int num = 0;
		if (woobleBuffer != null && woobleBuffer.Length == FormattedText.Length)
		{
			return;
		}
		if (woobleBuffer == null)
		{
			woobleBuffer = new Vector2[FormattedText.Length];
			woobleTimes = new float[FormattedText.Length];
			woobleRainbowBuffer = new bool[FormattedText.Length];
		}
		else
		{
			Array.Resize(ref woobleTimes, FormattedText.Length);
			Array.Resize(ref woobleBuffer, FormattedText.Length);
			Array.Resize(ref woobleRainbowBuffer, FormattedText.Length);
		}
		for (int i = 0; i < woobleStartIndices.Length; i++)
		{
			int num2 = woobleStartIndices[i];
			int num3 = woobleEndIndices[i];
			switch (woobleStyles[i])
			{
			case WoobleStyle.SEQUENTIAL:
			{
				for (int n = num2; n <= num3; n++)
				{
					if (n >= 0 && n < woobleTimes.Length && num3 + 1 - num2 > 0)
					{
						float num12 = ((float)n - (float)num2 * 1f) / (float)(num3 + 1 - num2);
						woobleTimes[n] = -1f * num12;
						woobleRainbowBuffer[n] = false;
					}
				}
				break;
			}
			case WoobleStyle.SEQUENTIAL_RAINBOW:
			{
				for (int num13 = num2; num13 <= num3; num13++)
				{
					if (num13 >= 0 && num13 < woobleTimes.Length && num3 + 1 - num2 > 0)
					{
						float num14 = ((float)num13 - (float)num2 * 1f) / (float)(num3 + 1 - num2);
						woobleTimes[num13] = -1f * num14;
						woobleRainbowBuffer[num13] = true;
					}
				}
				break;
			}
			case WoobleStyle.RANDOM_JITTER:
			{
				for (int l = num2; l <= num3; l++)
				{
					int num8 = num3 - num2;
					int num9 = Mathf.FloorToInt((float)num8 / 2f + 1f);
					indices.Add(num2 + num);
					num = (num + num9) % Mathf.Max(1, num8);
				}
				for (int m = num2; m <= num3; m++)
				{
					if (m >= 0 && m < woobleTimes.Length && num3 + 1 - num2 > 0)
					{
						int num10 = indices[m - num2];
						float num11 = ((float)num10 - (float)num2 * 1f) / (float)(num3 + 1 - num2);
						woobleTimes[m] = -1f * num11;
						woobleRainbowBuffer[m] = false;
					}
				}
				break;
			}
			case WoobleStyle.RANDOM_SEQUENTIAL:
			{
				for (int j = num2; j <= num3; j++)
				{
					int num4 = num3 - num2;
					int num5 = Mathf.FloorToInt((float)num4 / 2f + 1f);
					indices.Add(num2 + num);
					num = (num + num5) % Mathf.Max(1, num4);
				}
				for (int k = num2; k <= num3; k++)
				{
					if (k >= 0 && k < woobleTimes.Length && num3 + 1 - num2 > 0)
					{
						int num6 = indices[k - num2];
						float num7 = ((float)num6 - (float)num2 * 1f) / (float)(num3 + 1 - num2);
						woobleTimes[k] = -1f * num7;
						woobleRainbowBuffer[k] = false;
					}
				}
				break;
			}
			}
		}
	}

	protected void UpdateWooblyTextBuffers()
	{
		if (woobleBuffer == null || woobleBuffer.Length != FormattedText.Length)
		{
			InitWooblyTextBuffers();
		}
		float num = 3f;
		for (int i = 0; i < woobleStartIndices.Length; i++)
		{
			int num2 = woobleStartIndices[i];
			int num3 = woobleEndIndices[i];
			switch (woobleStyles[i])
			{
			case WoobleStyle.SEQUENTIAL:
			case WoobleStyle.RANDOM_SEQUENTIAL:
			{
				for (int m = num2; m <= num3; m++)
				{
					if (m >= 0 && m < woobleTimes.Length)
					{
						woobleTimes[m] += BraveTime.DeltaTime * num;
						float y3 = ((!(woobleTimes[m] < 0f)) ? (BraveMathCollege.HermiteInterpolation(Mathf.PingPong(woobleTimes[m], 1f)) * 0.25f - 0.0625f) : 0f);
						woobleBuffer[m] = new Vector2(0f, y3);
					}
				}
				break;
			}
			case WoobleStyle.SEQUENTIAL_RAINBOW:
			{
				for (int k = num2; k <= num3; k++)
				{
					if (k >= 0 && k < woobleTimes.Length)
					{
						woobleTimes[k] += BraveTime.DeltaTime * num;
						float y = ((!(woobleTimes[k] < 0f)) ? (BraveMathCollege.HermiteInterpolation(Mathf.PingPong(woobleTimes[k], 1f)) * 0.25f - 0.0625f) : 0f);
						woobleBuffer[k] = new Vector2(0f, y);
					}
				}
				break;
			}
			case WoobleStyle.SIMULTANEOUS:
			{
				for (int l = num2; l <= num3; l++)
				{
					if (l >= 0 && l < woobleTimes.Length)
					{
						woobleTimes[l] += BraveTime.DeltaTime * num;
						float y2 = ((!(woobleTimes[l] < 0f)) ? (BraveMathCollege.HermiteInterpolation(Mathf.PingPong(woobleTimes[l], 1f)) * 0.25f - 0.0625f) : 0f);
						woobleBuffer[l] = new Vector2(0f, y2);
					}
				}
				break;
			}
			case WoobleStyle.RANDOM_JITTER:
			{
				for (int j = num2; j <= num3; j++)
				{
					if (j >= 0 && j < woobleTimes.Length)
					{
						woobleTimes[j] += BraveTime.DeltaTime * num;
						if (woobleTimes[j] > 1f)
						{
							woobleTimes[j] -= 1f;
							woobleBuffer[j] = Vector2.Scale(new Vector2(1f / 32f, 1f / 32f), BraveUtility.GetMajorAxis(UnityEngine.Random.insideUnitCircle.normalized));
							woobleBuffer[j].x = Mathf.Abs(woobleBuffer[j].x);
							woobleBuffer[(j != num2) ? (j - 1) : num3] = Vector2.zero;
						}
					}
				}
				break;
			}
			}
		}
		SetNeedUpdate(UpdateFlags.UpdateText);
	}

	protected void OnDestroy()
	{
		if (meshFilter == null)
		{
			meshFilter = GetComponent<MeshFilter>();
		}
		if (meshFilter != null)
		{
			mesh = meshFilter.sharedMesh;
		}
		if ((bool)mesh)
		{
			UnityEngine.Object.DestroyImmediate(mesh, true);
			meshFilter.mesh = null;
		}
	}

	public int NumDrawnCharacters()
	{
		int num = NumTotalCharacters();
		if (num > data.maxChars)
		{
			num = data.maxChars;
		}
		return num;
	}

	public int NumTotalCharacters()
	{
		InitInstance();
		if ((updateFlags & (UpdateFlags.UpdateText | UpdateFlags.UpdateBuffers)) != 0)
		{
			FormatText();
		}
		int num = 0;
		for (int i = 0; i < _formattedText.Length; i++)
		{
			int num2 = _formattedText[i];
			bool flag = num2 == 94;
			if (_fontInst.useDictionary)
			{
				if (!_fontInst.charDict.ContainsKey(num2))
				{
					num2 = 0;
				}
			}
			else if (num2 >= _fontInst.chars.Length)
			{
				num2 = 0;
			}
			if (flag)
			{
				num2 = 94;
			}
			if (num2 == 10)
			{
				continue;
			}
			if (data.inlineStyling && num2 == 94 && i + 1 < _formattedText.Length)
			{
				if (_formattedText[i + 1] != '^')
				{
					i += GetInlineStyleCommandLength(_formattedText[i + 1]);
					continue;
				}
				i++;
			}
			num++;
		}
		return num;
	}

	[Obsolete("Use GetEstimatedMeshBoundsForString().size instead")]
	public Vector2 GetMeshDimensionsForString(string str)
	{
		return tk2dTextGeomGen.GetMeshDimensionsForString(str, tk2dTextGeomGen.Data(data, _fontInst, _formattedText));
	}

	public Bounds GetEstimatedMeshBoundsForString(string str)
	{
		InitInstance();
		tk2dTextGeomGen.GeomData geomData = tk2dTextGeomGen.Data(data, _fontInst, _formattedText);
		Vector2 meshDimensionsForString = tk2dTextGeomGen.GetMeshDimensionsForString(FormatText(str), geomData);
		float yAnchorForHeight = tk2dTextGeomGen.GetYAnchorForHeight(meshDimensionsForString.y, geomData);
		float xAnchorForWidth = tk2dTextGeomGen.GetXAnchorForWidth(meshDimensionsForString.x, geomData);
		float num = (_fontInst.lineHeight + data.lineSpacing) * data.scale.y;
		return new Bounds(new Vector3(xAnchorForWidth + meshDimensionsForString.x * 0.5f, yAnchorForHeight + meshDimensionsForString.y * 0.5f + num, 0f), Vector3.Scale(meshDimensionsForString, new Vector3(1f, -1f, 1f)));
	}

	public Bounds GetTrueBounds()
	{
		return mesh.bounds;
	}

	public void Init(bool force)
	{
		if (force)
		{
			SetNeedUpdate(UpdateFlags.UpdateBuffers);
		}
		Init();
	}

	private void UpdateRainbowStatus(bool[] rainbowValues)
	{
		if (!Foyer.DoIntroSequence)
		{
			bool flag = false;
			for (int i = 0; i < rainbowValues.Length; i++)
			{
				flag = flag || rainbowValues[i];
			}
			if (flag)
			{
				color = Color.white;
			}
			else
			{
				color = Color.black;
			}
		}
	}

	public void Init()
	{
		if (!_fontInst || ((updateFlags & UpdateFlags.UpdateBuffers) == 0 && !(mesh == null)))
		{
			return;
		}
		_fontInst.InitDictionary();
		FormatText();
		tk2dTextGeomGen.GeomData geomData = tk2dTextGeomGen.Data(data, _fontInst, _formattedText);
		int numVertices;
		int numIndices;
		tk2dTextGeomGen.GetTextMeshGeomDesc(out numVertices, out numIndices, geomData);
		vertices = new Vector3[numVertices];
		uvs = new Vector2[numVertices];
		colors = new Color32[numVertices];
		untintedColors = new Color32[numVertices];
		if (_fontInst.textureGradients)
		{
			uv2 = new Vector2[numVertices];
		}
		int[] triangles = new int[numIndices];
		if (supportsWooblyText)
		{
			InitWooblyTextBuffers();
		}
		if (supportsWooblyText)
		{
			UpdateRainbowStatus(woobleRainbowBuffer);
		}
		int target = ((!supportsWooblyText) ? tk2dTextGeomGen.SetTextMeshGeom(vertices, uvs, uv2, untintedColors, 0, geomData, visibleCharacters) : tk2dTextGeomGen.SetTextMeshGeom(vertices, uvs, uv2, untintedColors, 0, geomData, visibleCharacters, woobleBuffer, woobleRainbowBuffer));
		if (!_fontInst.isPacked)
		{
			Color32 color = data.color;
			Color32 color2 = ((!data.useGradient) ? data.color : data.color2);
			for (int i = 0; i < numVertices; i++)
			{
				Color32 color3 = ((i % 4 >= 2) ? color2 : color);
				byte b = (byte)(untintedColors[i].r * color3.r / 255);
				byte b2 = (byte)(untintedColors[i].g * color3.g / 255);
				byte b3 = (byte)(untintedColors[i].b * color3.b / 255);
				byte b4 = (byte)(untintedColors[i].a * color3.a / 255);
				if (_fontInst.premultipliedAlpha)
				{
					b = (byte)(b * b4 / 255);
					b2 = (byte)(b2 * b4 / 255);
					b3 = (byte)(b3 * b4 / 255);
				}
				colors[i] = new Color32(b, b2, b3, b4);
			}
		}
		else
		{
			colors = untintedColors;
		}
		tk2dTextGeomGen.SetTextMeshIndices(triangles, 0, 0, geomData, target);
		if (mesh == null)
		{
			if (meshFilter == null)
			{
				meshFilter = GetComponent<MeshFilter>();
			}
			mesh = new Mesh();
			mesh.hideFlags = HideFlags.DontSave;
			meshFilter.mesh = mesh;
		}
		else
		{
			mesh.Clear();
		}
		mesh.vertices = vertices;
		mesh.uv = uvs;
		if (font.textureGradients)
		{
			mesh.uv2 = uv2;
		}
		mesh.triangles = triangles;
		mesh.colors32 = colors;
		mesh.RecalculateBounds();
		mesh.bounds = tk2dBaseSprite.AdjustedMeshBounds(mesh.bounds, data.renderLayer);
		updateFlags = UpdateFlags.UpdateNone;
	}

	public void Commit()
	{
		tk2dUpdateManager.FlushQueues();
	}

	public void CheckFontsForLanguage()
	{
		InitInstance();
		if (m_defaultAssignedFont == null)
		{
			m_defaultAssignedFont = font;
		}
		tk2dFontData tk2dFontData2 = null;
		tk2dFontData2 = ((GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.JAPANESE) ? (ResourceCache.Acquire("Alternate Fonts/JackeyFont_TK2D") as GameObject).GetComponent<tk2dFont>().data : ((GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.RUSSIAN) ? (ResourceCache.Acquire("Alternate Fonts/PixelaCYR_15_TK2D") as GameObject).GetComponent<tk2dFont>().data : ((GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE) ? (ResourceCache.Acquire("Alternate Fonts/SimSun12_TK2D") as GameObject).GetComponent<tk2dFont>().data : ((GameManager.Options.CurrentLanguage != StringTableManager.GungeonSupportedLanguages.KOREAN) ? m_defaultAssignedFont : (ResourceCache.Acquire("Alternate Fonts/NanumGothic16TK2D") as GameObject).GetComponent<tk2dFont>().data))));
		if (tk2dFontData2 != null && font != tk2dFontData2)
		{
			font = tk2dFontData2;
			Init(true);
		}
	}

	public void DoNotUse__CommitInternal()
	{
		InitInstance();
		CheckFontsForLanguage();
		if (_fontInst == null)
		{
			return;
		}
		_fontInst.InitDictionary();
		if ((updateFlags & UpdateFlags.UpdateBuffers) != 0 || mesh == null)
		{
			Init();
		}
		else
		{
			if ((updateFlags & UpdateFlags.UpdateText) != 0)
			{
				FormatText();
				tk2dTextGeomGen.GeomData geomData = tk2dTextGeomGen.Data(data, _fontInst, _formattedText);
				if (supportsWooblyText && woobleBuffer.Length != FormattedText.Length)
				{
					InitWooblyTextBuffers();
				}
				if (supportsWooblyText)
				{
					UpdateRainbowStatus(woobleRainbowBuffer);
				}
				int num = ((!supportsWooblyText || !Application.isPlaying) ? tk2dTextGeomGen.SetTextMeshGeom(vertices, uvs, uv2, untintedColors, 0, geomData, visibleCharacters) : tk2dTextGeomGen.SetTextMeshGeom(vertices, uvs, uv2, untintedColors, 0, geomData, visibleCharacters, woobleBuffer, woobleRainbowBuffer));
				float a = float.MaxValue;
				float a2 = float.MinValue;
				for (int i = 0; i < vertices.Length; i++)
				{
					a = Mathf.Min(a, vertices[i].x);
					a2 = Mathf.Max(a2, vertices[i].x);
				}
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				for (int j = 0; j < geomData.formattedText.Length; j++)
				{
					string formattedText = geomData.formattedText;
					int num5 = formattedText[j];
					if (num5 != 91 || j >= formattedText.Length - 1 || formattedText[j + 1] == ']')
					{
						continue;
					}
					for (int k = j; k < formattedText.Length; k++)
					{
						char c = formattedText[k];
						if (c == ']')
						{
							int num6 = k - j;
							string text = formattedText.Substring(j + 9, num6 - 10);
							GameObject gameObject = null;
							gameObject = ((m_inlineSprites.Count <= num2) ? ((GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("ControllerButtonSprite"))) : m_inlineSprites[num2]);
							tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
							component.HeightOffGround = 3f;
							DepthLookupManager.AssignRendererToSortingLayer(component.renderer, DepthLookupManager.GungeonSortingLayer.FOREGROUND);
							gameObject.SetLayerRecursively(base.gameObject.layer);
							component.spriteId = component.GetSpriteIdByName(text);
							component.transform.parent = base.transform;
							component.transform.localPosition = tk2dTextGeomGen.inlineSpriteOffsetsForLastString[num4];
							if (!m_inlineSprites.Contains(gameObject))
							{
								m_inlineSprites.Add(gameObject);
								m_inlineSpriteIndices.Add(j - num3);
							}
							else
							{
								m_inlineSpriteIndices[m_inlineSprites.IndexOf(gameObject)] = j - num3;
							}
							j += num6;
							num3 += num6;
							num4++;
							num2++;
							break;
						}
					}
				}
				int num7;
				for (num7 = num2; num7 < m_inlineSprites.Count; num7++)
				{
					if (Application.isPlaying)
					{
						UnityEngine.Object.Destroy(m_inlineSprites[num7]);
					}
					else
					{
						UnityEngine.Object.DestroyImmediate(m_inlineSprites[num7]);
					}
					m_inlineSprites.RemoveAt(num7);
					m_inlineSpriteIndices.RemoveAt(num7);
					num7--;
				}
				for (int l = 0; l < m_inlineSprites.Count; l++)
				{
					if (m_inlineSpriteIndices[l] > visibleCharacters)
					{
						m_inlineSprites[l].GetComponent<Renderer>().enabled = false;
					}
					else
					{
						m_inlineSprites[l].GetComponent<Renderer>().enabled = true;
					}
				}
				for (int m = num; m < data.maxChars; m++)
				{
					vertices[m * 4] = (vertices[m * 4 + 1] = (vertices[m * 4 + 2] = (vertices[m * 4 + 3] = Vector3.zero)));
				}
				mesh.vertices = vertices;
				mesh.uv = uvs;
				if (_fontInst.textureGradients)
				{
					mesh.uv2 = uv2;
				}
				if (_fontInst.isPacked)
				{
					colors = untintedColors;
					mesh.colors32 = colors;
				}
				if (data.inlineStyling)
				{
					SetNeedUpdate(UpdateFlags.UpdateColors);
				}
				mesh.RecalculateBounds();
				mesh.bounds = tk2dBaseSprite.AdjustedMeshBounds(mesh.bounds, data.renderLayer);
			}
			if (!font.isPacked && (updateFlags & UpdateFlags.UpdateColors) != 0)
			{
				Color32 color = data.color;
				Color32 color2 = ((!data.useGradient) ? data.color : data.color2);
				for (int n = 0; n < colors.Length; n++)
				{
					Color32 color3 = ((n % 4 >= 2) ? color2 : color);
					byte b = (byte)(untintedColors[n].r * color3.r / 255);
					byte b2 = (byte)(untintedColors[n].g * color3.g / 255);
					byte b3 = (byte)(untintedColors[n].b * color3.b / 255);
					byte b4 = (byte)(untintedColors[n].a * color3.a / 255);
					if (_fontInst.premultipliedAlpha)
					{
						b = (byte)(b * b4 / 255);
						b2 = (byte)(b2 * b4 / 255);
						b3 = (byte)(b3 * b4 / 255);
					}
					colors[n] = new Color32(b, b2, b3, b4);
				}
				mesh.colors32 = colors;
			}
		}
		updateFlags = UpdateFlags.UpdateNone;
	}

	public void MakePixelPerfect()
	{
		float num = 1f;
		tk2dCamera tk2dCamera2 = tk2dCamera.CameraForLayer(base.gameObject.layer);
		if (tk2dCamera2 != null)
		{
			if (_fontInst.version < 1)
			{
				Debug.LogError("Need to rebuild font.");
			}
			float distance = base.transform.position.z - tk2dCamera2.transform.position.z;
			float num2 = _fontInst.invOrthoSize * _fontInst.halfTargetHeight;
			num = tk2dCamera2.GetSizeAtDistance(distance) * num2;
		}
		else if ((bool)Camera.main)
		{
			if (Camera.main.orthographic)
			{
				num = Camera.main.orthographicSize;
			}
			else
			{
				float zdist = base.transform.position.z - Camera.main.transform.position.z;
				num = tk2dPixelPerfectHelper.CalculateScaleForPerspectiveCamera(Camera.main.fieldOfView, zdist);
			}
			num *= _fontInst.invOrthoSize;
		}
		scale = new Vector3(Mathf.Sign(scale.x) * num, Mathf.Sign(scale.y) * num, Mathf.Sign(scale.z) * num);
	}

	public bool UsesSpriteCollection(tk2dSpriteCollectionData spriteCollection)
	{
		if (data.font != null && data.font.spriteCollection != null)
		{
			return data.font.spriteCollection == spriteCollection;
		}
		return true;
	}

	private void UpdateMaterial()
	{
		if (GetComponent<Renderer>().sharedMaterial != _fontInst.materialInst)
		{
			GetComponent<Renderer>().material = _fontInst.materialInst;
		}
	}

	public void ForceBuild()
	{
		if (data.font != null)
		{
			_fontInst = data.font.inst;
			UpdateMaterial();
		}
		Init(true);
	}
}
