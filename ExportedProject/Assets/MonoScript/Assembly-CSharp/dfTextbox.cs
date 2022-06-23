using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
[dfCategory("Basic Controls")]
[dfTooltip("Implements a text entry control")]
[dfHelp("http://www.daikonforge.com/docs/df-gui/classdf_textbox.html")]
[AddComponentMenu("Daikon Forge/User Interface/Textbox")]
[ExecuteInEditMode]
public class dfTextbox : dfInteractiveBase, IDFMultiRender, IRendersText
{
	[SerializeField]
	protected dfFontBase font;

	[SerializeField]
	protected bool acceptsTab;

	[SerializeField]
	protected bool displayAsPassword;

	[SerializeField]
	protected string passwordChar = "*";

	[SerializeField]
	protected bool readOnly;

	[SerializeField]
	protected string text = string.Empty;

	[SerializeField]
	protected Color32 textColor = UnityEngine.Color.white;

	[SerializeField]
	protected Color32 selectionBackground = new Color32(0, 105, 210, byte.MaxValue);

	[SerializeField]
	protected Color32 cursorColor = UnityEngine.Color.white;

	[SerializeField]
	protected string selectionSprite = string.Empty;

	[SerializeField]
	protected float textScale = 1f;

	[SerializeField]
	protected dfTextScaleMode textScaleMode;

	[SerializeField]
	protected RectOffset padding = new RectOffset();

	[SerializeField]
	protected float cursorBlinkTime = 0.45f;

	[SerializeField]
	protected int cursorWidth = 1;

	[SerializeField]
	protected int maxLength = 1024;

	[SerializeField]
	protected bool selectOnFocus;

	[SerializeField]
	protected bool shadow;

	[SerializeField]
	protected Color32 shadowColor = UnityEngine.Color.black;

	[SerializeField]
	protected Vector2 shadowOffset = new Vector2(1f, -1f);

	[SerializeField]
	protected bool useMobileKeyboard;

	[SerializeField]
	protected int mobileKeyboardType;

	[SerializeField]
	protected bool mobileAutoCorrect;

	[SerializeField]
	protected bool mobileHideInputField;

	[SerializeField]
	protected dfMobileKeyboardTrigger mobileKeyboardTrigger;

	[SerializeField]
	protected TextAlignment textAlign;

	private Vector2 startSize = Vector2.zero;

	private int selectionStart;

	private int selectionEnd;

	private int mouseSelectionAnchor;

	private int scrollIndex;

	private int cursorIndex;

	private float leftOffset;

	private bool cursorShown;

	private float[] charWidths;

	private float whenGotFocus;

	private string undoText = string.Empty;

	private float tripleClickTimer;

	private bool isFontCallbackAssigned;

	private dfRenderData textRenderData;

	private dfList<dfRenderData> buffers = dfList<dfRenderData>.Obtain();

	public dfFontBase Font
	{
		get
		{
			if (font == null)
			{
				dfGUIManager manager = GetManager();
				if (manager != null)
				{
					font = manager.DefaultFont;
				}
			}
			return font;
		}
		set
		{
			if (value != font)
			{
				unbindTextureRebuildCallback();
				font = value;
				bindTextureRebuildCallback();
				Invalidate();
			}
		}
	}

	public int SelectionStart
	{
		get
		{
			return selectionStart;
		}
		set
		{
			if (value != selectionStart)
			{
				selectionStart = Mathf.Max(0, Mathf.Min(value, text.Length));
				selectionEnd = Mathf.Max(selectionEnd, selectionStart);
				Invalidate();
			}
		}
	}

	public int SelectionEnd
	{
		get
		{
			return selectionEnd;
		}
		set
		{
			if (value != selectionEnd)
			{
				selectionEnd = Mathf.Max(0, Mathf.Min(value, text.Length));
				selectionStart = Mathf.Max(selectionStart, selectionEnd);
				Invalidate();
			}
		}
	}

	public int SelectionLength
	{
		get
		{
			return selectionEnd - selectionStart;
		}
	}

	public string SelectedText
	{
		get
		{
			if (selectionEnd == selectionStart)
			{
				return string.Empty;
			}
			return text.Substring(selectionStart, selectionEnd - selectionStart);
		}
	}

	public bool SelectOnFocus
	{
		get
		{
			return selectOnFocus;
		}
		set
		{
			selectOnFocus = value;
		}
	}

	public RectOffset Padding
	{
		get
		{
			if (padding == null)
			{
				padding = new RectOffset();
			}
			return padding;
		}
		set
		{
			value = value.ConstrainPadding();
			if (!object.Equals(value, padding))
			{
				padding = value;
				Invalidate();
			}
		}
	}

	public bool IsPasswordField
	{
		get
		{
			return displayAsPassword;
		}
		set
		{
			if (value != displayAsPassword)
			{
				displayAsPassword = value;
				Invalidate();
			}
		}
	}

	public string PasswordCharacter
	{
		get
		{
			return passwordChar;
		}
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				passwordChar = value[0].ToString();
			}
			else
			{
				passwordChar = value;
			}
			OnPasswordCharacterChanged();
			Invalidate();
		}
	}

	public float CursorBlinkTime
	{
		get
		{
			return cursorBlinkTime;
		}
		set
		{
			cursorBlinkTime = value;
		}
	}

	public int CursorWidth
	{
		get
		{
			return cursorWidth;
		}
		set
		{
			cursorWidth = value;
		}
	}

	public int CursorIndex
	{
		get
		{
			return cursorIndex;
		}
		set
		{
			setCursorPos(value);
		}
	}

	public bool ReadOnly
	{
		get
		{
			return readOnly;
		}
		set
		{
			if (value != readOnly)
			{
				readOnly = value;
				OnReadOnlyChanged();
				Invalidate();
			}
		}
	}

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			value = value ?? string.Empty;
			if (value.Length > MaxLength)
			{
				value = value.Substring(0, MaxLength);
			}
			value = value.Replace("\t", " ");
			if (value != text)
			{
				text = value;
				scrollIndex = (cursorIndex = 0);
				OnTextChanged();
				Invalidate();
			}
		}
	}

	public Color32 TextColor
	{
		get
		{
			return textColor;
		}
		set
		{
			textColor = value;
			Invalidate();
		}
	}

	public string SelectionSprite
	{
		get
		{
			return selectionSprite;
		}
		set
		{
			if (value != selectionSprite)
			{
				selectionSprite = value;
				Invalidate();
			}
		}
	}

	public Color32 SelectionBackgroundColor
	{
		get
		{
			return selectionBackground;
		}
		set
		{
			selectionBackground = value;
			Invalidate();
		}
	}

	public Color32 CursorColor
	{
		get
		{
			return cursorColor;
		}
		set
		{
			cursorColor = value;
			Invalidate();
		}
	}

	public float TextScale
	{
		get
		{
			return textScale;
		}
		set
		{
			value = Mathf.Max(0.1f, value);
			if (!Mathf.Approximately(textScale, value))
			{
				dfFontManager.Invalidate(Font);
				textScale = value;
				Invalidate();
			}
		}
	}

	public dfTextScaleMode TextScaleMode
	{
		get
		{
			return textScaleMode;
		}
		set
		{
			textScaleMode = value;
			Invalidate();
		}
	}

	public int MaxLength
	{
		get
		{
			return maxLength;
		}
		set
		{
			if (value != maxLength)
			{
				maxLength = Mathf.Max(0, value);
				if (maxLength < text.Length)
				{
					Text = text.Substring(0, maxLength);
				}
				Invalidate();
			}
		}
	}

	public TextAlignment TextAlignment
	{
		get
		{
			return textAlign;
		}
		set
		{
			if (value != textAlign)
			{
				textAlign = value;
				Invalidate();
			}
		}
	}

	public bool Shadow
	{
		get
		{
			return shadow;
		}
		set
		{
			if (value != shadow)
			{
				shadow = value;
				Invalidate();
			}
		}
	}

	public Color32 ShadowColor
	{
		get
		{
			return shadowColor;
		}
		set
		{
			if (!value.Equals(shadowColor))
			{
				shadowColor = value;
				Invalidate();
			}
		}
	}

	public Vector2 ShadowOffset
	{
		get
		{
			return shadowOffset;
		}
		set
		{
			if (value != shadowOffset)
			{
				shadowOffset = value;
				Invalidate();
			}
		}
	}

	public bool UseMobileKeyboard
	{
		get
		{
			return useMobileKeyboard;
		}
		set
		{
			useMobileKeyboard = value;
		}
	}

	public bool MobileAutoCorrect
	{
		get
		{
			return mobileAutoCorrect;
		}
		set
		{
			mobileAutoCorrect = value;
		}
	}

	public bool HideMobileInputField
	{
		get
		{
			return mobileHideInputField;
		}
		set
		{
			mobileHideInputField = value;
		}
	}

	public dfMobileKeyboardTrigger MobileKeyboardTrigger
	{
		get
		{
			return mobileKeyboardTrigger;
		}
		set
		{
			mobileKeyboardTrigger = value;
		}
	}

	public event PropertyChangedEventHandler<bool> ReadOnlyChanged;

	public event PropertyChangedEventHandler<string> PasswordCharacterChanged;

	public event PropertyChangedEventHandler<string> TextChanged;

	public event PropertyChangedEventHandler<string> TextSubmitted;

	public event PropertyChangedEventHandler<string> TextCancelled;

	protected override void OnTabKeyPressed(dfKeyEventArgs args)
	{
		if (acceptsTab)
		{
			base.OnKeyPress(args);
			if (!args.Used)
			{
				args.Character = '\t';
				processKeyPress(args);
			}
		}
		else
		{
			base.OnTabKeyPressed(args);
		}
	}

	protected internal override void OnKeyPress(dfKeyEventArgs args)
	{
		if (ReadOnly || char.IsControl(args.Character))
		{
			base.OnKeyPress(args);
			return;
		}
		base.OnKeyPress(args);
		if (!args.Used)
		{
			processKeyPress(args);
		}
	}

	private void processKeyPress(dfKeyEventArgs args)
	{
		DeleteSelection();
		if (text.Length < MaxLength)
		{
			if (cursorIndex == text.Length)
			{
				text += args.Character;
			}
			else
			{
				text = text.Insert(cursorIndex, args.Character.ToString());
			}
			cursorIndex++;
			OnTextChanged();
			Invalidate();
		}
		args.Use();
	}

	protected internal override void OnKeyDown(dfKeyEventArgs args)
	{
		if (ReadOnly)
		{
			return;
		}
		base.OnKeyDown(args);
		if (args.Used)
		{
			return;
		}
		switch (args.KeyCode)
		{
		case KeyCode.A:
			if (args.Control)
			{
				SelectAll();
			}
			break;
		case KeyCode.Insert:
			if (args.Shift)
			{
				string clipBoard = dfClipboardHelper.clipBoard;
				if (!string.IsNullOrEmpty(clipBoard))
				{
					PasteAtCursor(clipBoard);
				}
			}
			break;
		case KeyCode.V:
			if (args.Control)
			{
				string clipBoard2 = dfClipboardHelper.clipBoard;
				if (!string.IsNullOrEmpty(clipBoard2))
				{
					PasteAtCursor(clipBoard2);
				}
			}
			break;
		case KeyCode.C:
			if (args.Control)
			{
				CopySelectionToClipboard();
			}
			break;
		case KeyCode.X:
			if (args.Control)
			{
				CutSelectionToClipboard();
			}
			break;
		case KeyCode.LeftArrow:
			if (args.Control)
			{
				if (args.Shift)
				{
					moveSelectionPointLeftWord();
				}
				else
				{
					MoveCursorToPreviousWord();
				}
			}
			else if (args.Shift)
			{
				moveSelectionPointLeft();
			}
			else
			{
				MoveCursorToPreviousChar();
			}
			break;
		case KeyCode.RightArrow:
			if (args.Control)
			{
				if (args.Shift)
				{
					moveSelectionPointRightWord();
				}
				else
				{
					MoveCursorToNextWord();
				}
			}
			else if (args.Shift)
			{
				moveSelectionPointRight();
			}
			else
			{
				MoveCursorToNextChar();
			}
			break;
		case KeyCode.Home:
			if (args.Shift)
			{
				SelectToStart();
			}
			else
			{
				MoveCursorToStart();
			}
			break;
		case KeyCode.End:
			if (args.Shift)
			{
				SelectToEnd();
			}
			else
			{
				MoveCursorToEnd();
			}
			break;
		case KeyCode.Delete:
			if (selectionStart != selectionEnd)
			{
				DeleteSelection();
			}
			else if (args.Control)
			{
				DeleteNextWord();
			}
			else
			{
				DeleteNextChar();
			}
			break;
		case KeyCode.Backspace:
			if (args.Control)
			{
				DeletePreviousWord();
			}
			else
			{
				DeletePreviousChar();
			}
			break;
		case KeyCode.Escape:
			ClearSelection();
			cursorIndex = (scrollIndex = 0);
			Invalidate();
			OnCancel();
			break;
		case KeyCode.Return:
			OnSubmit();
			break;
		default:
			base.OnKeyDown(args);
			return;
		}
		args.Use();
	}

	public override void OnEnable()
	{
		if (padding == null)
		{
			padding = new RectOffset();
		}
		base.OnEnable();
		if (size.magnitude == 0f)
		{
			base.Size = new Vector2(100f, 20f);
		}
		cursorShown = false;
		cursorIndex = (scrollIndex = 0);
		bool flag = Font != null && Font.IsValid;
		if (Application.isPlaying && !flag)
		{
			Font = GetManager().DefaultFont;
		}
		bindTextureRebuildCallback();
	}

	public override void OnDisable()
	{
		base.OnDisable();
		unbindTextureRebuildCallback();
	}

	public override void Awake()
	{
		base.Awake();
		startSize = base.Size;
	}

	protected internal override void OnEnterFocus(dfFocusEventArgs args)
	{
		base.OnEnterFocus(args);
		undoText = Text;
		if (!ReadOnly)
		{
			whenGotFocus = Time.realtimeSinceStartup;
			StopAllCoroutines();
			StartCoroutine(doCursorBlink());
			if (selectOnFocus)
			{
				selectionStart = 0;
				selectionEnd = text.Length;
			}
			else
			{
				selectionStart = (selectionEnd = 0);
			}
		}
		Invalidate();
	}

	protected internal override void OnLeaveFocus(dfFocusEventArgs args)
	{
		base.OnLeaveFocus(args);
		StopAllCoroutines();
		cursorShown = false;
		ClearSelection();
		Invalidate();
		whenGotFocus = 0f;
	}

	protected internal override void OnDoubleClick(dfMouseEventArgs args)
	{
		tripleClickTimer = Time.realtimeSinceStartup;
		if (args.Source != this)
		{
			base.OnDoubleClick(args);
			return;
		}
		if (!ReadOnly && HasFocus && args.Buttons.IsSet(dfMouseButtons.Left) && Time.realtimeSinceStartup - whenGotFocus > 0.5f)
		{
			int charIndexOfMouse = getCharIndexOfMouse(args);
			SelectWordAtIndex(charIndexOfMouse);
		}
		base.OnDoubleClick(args);
	}

	protected internal override void OnMouseDown(dfMouseEventArgs args)
	{
		if (args.Source != this)
		{
			base.OnMouseDown(args);
			return;
		}
		if (!ReadOnly && args.Buttons.IsSet(dfMouseButtons.Left) && ((!HasFocus && !SelectOnFocus) || Time.realtimeSinceStartup - whenGotFocus > 0.25f))
		{
			int charIndexOfMouse = getCharIndexOfMouse(args);
			if (charIndexOfMouse != cursorIndex)
			{
				cursorIndex = charIndexOfMouse;
				cursorShown = true;
				Invalidate();
				args.Use();
			}
			mouseSelectionAnchor = cursorIndex;
			selectionStart = (selectionEnd = cursorIndex);
			if (Time.realtimeSinceStartup - tripleClickTimer < 0.25f)
			{
				SelectAll();
				tripleClickTimer = 0f;
			}
		}
		base.OnMouseDown(args);
	}

	protected internal override void OnMouseMove(dfMouseEventArgs args)
	{
		if (args.Source != this)
		{
			base.OnMouseMove(args);
			return;
		}
		if (!ReadOnly && HasFocus && args.Buttons.IsSet(dfMouseButtons.Left))
		{
			int charIndexOfMouse = getCharIndexOfMouse(args);
			if (charIndexOfMouse != cursorIndex)
			{
				cursorIndex = charIndexOfMouse;
				cursorShown = true;
				Invalidate();
				args.Use();
				selectionStart = Mathf.Min(mouseSelectionAnchor, charIndexOfMouse);
				selectionEnd = Mathf.Max(mouseSelectionAnchor, charIndexOfMouse);
				return;
			}
		}
		base.OnMouseMove(args);
	}

	protected internal virtual void OnTextChanged()
	{
		SignalHierarchy("OnTextChanged", this, text);
		if (this.TextChanged != null)
		{
			this.TextChanged(this, text);
		}
	}

	protected internal virtual void OnReadOnlyChanged()
	{
		if (this.ReadOnlyChanged != null)
		{
			this.ReadOnlyChanged(this, readOnly);
		}
	}

	protected internal virtual void OnPasswordCharacterChanged()
	{
		if (this.PasswordCharacterChanged != null)
		{
			this.PasswordCharacterChanged(this, passwordChar);
		}
	}

	protected internal virtual void OnSubmit()
	{
		SignalHierarchy("OnTextSubmitted", this, text);
		if (this.TextSubmitted != null)
		{
			this.TextSubmitted(this, text);
		}
	}

	protected internal virtual void OnCancel()
	{
		text = undoText;
		SignalHierarchy("OnTextCancelled", this, text);
		if (this.TextCancelled != null)
		{
			this.TextCancelled(this, text);
		}
	}

	public void ClearSelection()
	{
		selectionStart = 0;
		selectionEnd = 0;
		mouseSelectionAnchor = 0;
	}

	public void SelectAll()
	{
		selectionStart = 0;
		selectionEnd = text.Length;
		scrollIndex = 0;
		setCursorPos(0);
	}

	private void CutSelectionToClipboard()
	{
		CopySelectionToClipboard();
		DeleteSelection();
	}

	private void CopySelectionToClipboard()
	{
		if (selectionStart != selectionEnd)
		{
			dfClipboardHelper.clipBoard = text.Substring(selectionStart, selectionEnd - selectionStart);
		}
	}

	public void PasteAtCursor(string clipData)
	{
		DeleteSelection();
		StringBuilder stringBuilder = new StringBuilder(text.Length + clipData.Length);
		stringBuilder.Append(text);
		foreach (char c in clipData)
		{
			if (c >= ' ')
			{
				stringBuilder.Insert(cursorIndex++, c);
			}
		}
		stringBuilder.Length = Mathf.Min(stringBuilder.Length, maxLength);
		text = stringBuilder.ToString();
		setCursorPos(cursorIndex);
		OnTextChanged();
		Invalidate();
	}

	public void SelectWordAtIndex(int index)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		index = Mathf.Max(Mathf.Min(text.Length - 1, index), 0);
		char c = text[index];
		if (!char.IsLetterOrDigit(c))
		{
			selectionStart = index;
			selectionEnd = index + 1;
			mouseSelectionAnchor = 0;
		}
		else
		{
			selectionStart = index;
			int num = index;
			while (num > 0 && char.IsLetterOrDigit(text[num - 1]))
			{
				selectionStart--;
				num--;
			}
			selectionEnd = index;
			for (int i = index; i < text.Length && char.IsLetterOrDigit(text[i]); i++)
			{
				selectionEnd = i + 1;
			}
		}
		cursorIndex = selectionStart;
		Invalidate();
	}

	public void DeletePreviousChar()
	{
		if (selectionStart != selectionEnd)
		{
			int cursorPos = selectionStart;
			DeleteSelection();
			setCursorPos(cursorPos);
			return;
		}
		ClearSelection();
		if (cursorIndex != 0)
		{
			text = text.Remove(cursorIndex - 1, 1);
			cursorIndex--;
			cursorShown = true;
			OnTextChanged();
			Invalidate();
		}
	}

	public void DeletePreviousWord()
	{
		ClearSelection();
		if (cursorIndex != 0)
		{
			int num = findPreviousWord(cursorIndex);
			if (num == cursorIndex)
			{
				num = 0;
			}
			text = text.Remove(num, cursorIndex - num);
			setCursorPos(num);
			OnTextChanged();
			Invalidate();
		}
	}

	public void DeleteSelection()
	{
		if (selectionStart != selectionEnd)
		{
			text = text.Remove(selectionStart, selectionEnd - selectionStart);
			setCursorPos(selectionStart);
			ClearSelection();
			OnTextChanged();
			Invalidate();
		}
	}

	public void DeleteNextChar()
	{
		ClearSelection();
		if (cursorIndex < text.Length)
		{
			text = text.Remove(cursorIndex, 1);
			cursorShown = true;
			OnTextChanged();
			Invalidate();
		}
	}

	public void DeleteNextWord()
	{
		ClearSelection();
		if (cursorIndex != text.Length)
		{
			int num = findNextWord(cursorIndex);
			if (num == cursorIndex)
			{
				num = text.Length;
			}
			text = text.Remove(cursorIndex, num - cursorIndex);
			OnTextChanged();
			Invalidate();
		}
	}

	public void SelectToStart()
	{
		if (cursorIndex != 0)
		{
			if (selectionEnd == selectionStart)
			{
				selectionEnd = cursorIndex;
			}
			else if (selectionEnd == cursorIndex)
			{
				selectionEnd = selectionStart;
			}
			selectionStart = 0;
			setCursorPos(0);
		}
	}

	public void SelectToEnd()
	{
		if (cursorIndex != text.Length)
		{
			if (selectionEnd == selectionStart)
			{
				selectionStart = cursorIndex;
			}
			else if (selectionStart == cursorIndex)
			{
				selectionStart = selectionEnd;
			}
			selectionEnd = text.Length;
			setCursorPos(text.Length);
		}
	}

	public void MoveCursorToNextWord()
	{
		ClearSelection();
		if (cursorIndex != text.Length)
		{
			int cursorPos = findNextWord(cursorIndex);
			setCursorPos(cursorPos);
		}
	}

	public void MoveCursorToPreviousWord()
	{
		ClearSelection();
		if (cursorIndex != 0)
		{
			int cursorPos = findPreviousWord(cursorIndex);
			setCursorPos(cursorPos);
		}
	}

	public void MoveCursorToEnd()
	{
		ClearSelection();
		setCursorPos(text.Length);
	}

	public void MoveCursorToStart()
	{
		ClearSelection();
		setCursorPos(0);
	}

	public void MoveCursorToNextChar()
	{
		ClearSelection();
		setCursorPos(cursorIndex + 1);
	}

	public void MoveCursorToPreviousChar()
	{
		ClearSelection();
		setCursorPos(cursorIndex - 1);
	}

	private void moveSelectionPointRightWord()
	{
		if (cursorIndex != text.Length)
		{
			int cursorPos = findNextWord(cursorIndex);
			if (selectionEnd == selectionStart)
			{
				selectionStart = cursorIndex;
				selectionEnd = cursorPos;
			}
			else if (selectionEnd == cursorIndex)
			{
				selectionEnd = cursorPos;
			}
			else if (selectionStart == cursorIndex)
			{
				selectionStart = cursorPos;
			}
			setCursorPos(cursorPos);
		}
	}

	private void moveSelectionPointLeftWord()
	{
		if (cursorIndex != 0)
		{
			int cursorPos = findPreviousWord(cursorIndex);
			if (selectionEnd == selectionStart)
			{
				selectionEnd = cursorIndex;
				selectionStart = cursorPos;
			}
			else if (selectionEnd == cursorIndex)
			{
				selectionEnd = cursorPos;
			}
			else if (selectionStart == cursorIndex)
			{
				selectionStart = cursorPos;
			}
			setCursorPos(cursorPos);
		}
	}

	private void moveSelectionPointRight()
	{
		if (cursorIndex != text.Length)
		{
			if (selectionEnd == selectionStart)
			{
				selectionEnd = cursorIndex + 1;
				selectionStart = cursorIndex;
			}
			else if (selectionEnd == cursorIndex)
			{
				selectionEnd++;
			}
			else if (selectionStart == cursorIndex)
			{
				selectionStart++;
			}
			setCursorPos(cursorIndex + 1);
		}
	}

	private void moveSelectionPointLeft()
	{
		if (cursorIndex != 0)
		{
			if (selectionEnd == selectionStart)
			{
				selectionEnd = cursorIndex;
				selectionStart = cursorIndex - 1;
			}
			else if (selectionEnd == cursorIndex)
			{
				selectionEnd--;
			}
			else if (selectionStart == cursorIndex)
			{
				selectionStart--;
			}
			setCursorPos(cursorIndex - 1);
		}
	}

	private void setCursorPos(int index)
	{
		index = Mathf.Max(0, Mathf.Min(text.Length, index));
		if (index != cursorIndex)
		{
			cursorIndex = index;
			cursorShown = HasFocus;
			scrollIndex = Mathf.Min(scrollIndex, cursorIndex);
			Invalidate();
		}
	}

	private int findPreviousWord(int startIndex)
	{
		int num = startIndex;
		while (num > 0)
		{
			char c = text[num - 1];
			if (char.IsWhiteSpace(c) || char.IsSeparator(c) || char.IsPunctuation(c))
			{
				num--;
				continue;
			}
			break;
		}
		for (int num2 = num; num2 >= 0; num2--)
		{
			if (num2 == 0)
			{
				num = 0;
				break;
			}
			char c2 = text[num2 - 1];
			if (char.IsWhiteSpace(c2) || char.IsSeparator(c2) || char.IsPunctuation(c2))
			{
				num = num2;
				break;
			}
		}
		return num;
	}

	private int findNextWord(int startIndex)
	{
		int length = text.Length;
		int i = startIndex;
		for (int j = i; j < length; j++)
		{
			char c = text[j];
			if (char.IsWhiteSpace(c) || char.IsSeparator(c) || char.IsPunctuation(c))
			{
				i = j;
				break;
			}
		}
		for (; i < length; i++)
		{
			char c2 = text[i];
			if (!char.IsWhiteSpace(c2) && !char.IsSeparator(c2) && !char.IsPunctuation(c2))
			{
				break;
			}
		}
		return i;
	}

	private IEnumerator doCursorBlink()
	{
		if (Application.isPlaying)
		{
			cursorShown = true;
			while (ContainsFocus)
			{
				yield return new WaitForSeconds(cursorBlinkTime);
				cursorShown = !cursorShown;
				Invalidate();
			}
			cursorShown = false;
		}
	}

	private void renderText(dfRenderData textBuffer)
	{
		float num = PixelsToUnits();
		Vector2 vector = new Vector2(size.x - (float)padding.horizontal, size.y - (float)padding.vertical);
		Vector3 vector2 = pivot.TransformToUpperLeft(base.Size);
		Vector3 vectorOffset = new Vector3(vector2.x + (float)padding.left, vector2.y - (float)padding.top, 0f) * num;
		string text = ((!IsPasswordField || string.IsNullOrEmpty(passwordChar)) ? this.text : passwordDisplayText());
		Color32 color = ((!base.IsEnabled) ? base.DisabledColor : TextColor);
		float textScaleMultiplier = getTextScaleMultiplier();
		using (dfFontRendererBase dfFontRendererBase2 = font.ObtainRenderer())
		{
			dfFontRendererBase2.WordWrap = false;
			dfFontRendererBase2.MaxSize = vector;
			dfFontRendererBase2.PixelRatio = num;
			dfFontRendererBase2.TextScale = TextScale * textScaleMultiplier;
			dfFontRendererBase2.VectorOffset = vectorOffset;
			dfFontRendererBase2.MultiLine = false;
			dfFontRendererBase2.TextAlign = TextAlignment.Left;
			dfFontRendererBase2.ProcessMarkup = false;
			dfFontRendererBase2.DefaultColor = color;
			dfFontRendererBase2.BottomColor = color;
			dfFontRendererBase2.OverrideMarkupColors = false;
			dfFontRendererBase2.Opacity = CalculateOpacity();
			dfFontRendererBase2.Shadow = Shadow;
			dfFontRendererBase2.ShadowColor = ShadowColor;
			dfFontRendererBase2.ShadowOffset = ShadowOffset;
			cursorIndex = Mathf.Min(cursorIndex, text.Length);
			scrollIndex = Mathf.Min(Mathf.Min(scrollIndex, cursorIndex), text.Length);
			charWidths = dfFontRendererBase2.GetCharacterWidths(text);
			Vector2 vector3 = vector * num;
			leftOffset = 0f;
			if (textAlign == TextAlignment.Left)
			{
				float num2 = 0f;
				for (int i = scrollIndex; i < cursorIndex; i++)
				{
					num2 += charWidths[i];
				}
				while (num2 >= vector3.x && scrollIndex < cursorIndex)
				{
					num2 -= charWidths[scrollIndex++];
				}
			}
			else
			{
				scrollIndex = Mathf.Max(0, Mathf.Min(cursorIndex, text.Length - 1));
				float num3 = 0f;
				float num4 = (float)font.FontSize * 1.25f * num;
				while (scrollIndex > 0 && num3 < vector3.x - num4)
				{
					num3 += charWidths[scrollIndex--];
				}
				float num5 = ((text.Length <= 0) ? 0f : dfFontRendererBase2.GetCharacterWidths(text.Substring(scrollIndex)).Sum());
				switch (textAlign)
				{
				case TextAlignment.Center:
					leftOffset = Mathf.Max(0f, (vector3.x - num5) * 0.5f);
					break;
				case TextAlignment.Right:
					leftOffset = Mathf.Max(0f, vector3.x - num5);
					break;
				}
				vectorOffset.x += leftOffset;
				dfFontRendererBase2.VectorOffset = vectorOffset;
			}
			if (selectionEnd != selectionStart)
			{
				renderSelection(scrollIndex, charWidths, leftOffset);
			}
			else if (cursorShown)
			{
				renderCursor(scrollIndex, cursorIndex, charWidths, leftOffset);
			}
			dfFontRendererBase2.Render(text.Substring(scrollIndex), textBuffer);
		}
	}

	private float getTextScaleMultiplier()
	{
		if (textScaleMode == dfTextScaleMode.None || !Application.isPlaying)
		{
			return 1f;
		}
		if (textScaleMode == dfTextScaleMode.ScreenResolution)
		{
			return (float)Screen.height / (float)cachedManager.FixedHeight;
		}
		return base.Size.y / startSize.y;
	}

	private string passwordDisplayText()
	{
		return new string(passwordChar[0], text.Length);
	}

	private void renderSelection(int scrollIndex, float[] charWidths, float leftOffset)
	{
		if (string.IsNullOrEmpty(SelectionSprite) || base.Atlas == null)
		{
			return;
		}
		float num = PixelsToUnits();
		float num2 = (size.x - (float)padding.horizontal) * num;
		int num3 = scrollIndex;
		float num4 = 0f;
		for (int i = scrollIndex; i < text.Length; i++)
		{
			num3++;
			num4 += charWidths[i];
			if (num4 > num2)
			{
				break;
			}
		}
		if (selectionStart > num3 || selectionEnd < scrollIndex)
		{
			return;
		}
		int num5 = Mathf.Max(scrollIndex, selectionStart);
		if (num5 > num3)
		{
			return;
		}
		int num6 = Mathf.Min(selectionEnd, num3);
		if (num6 <= scrollIndex)
		{
			return;
		}
		float num7 = 0f;
		float num8 = 0f;
		num4 = 0f;
		for (int j = scrollIndex; j <= num3; j++)
		{
			if (j == num5)
			{
				num7 = num4;
			}
			if (j == num6)
			{
				num8 = num4;
				break;
			}
			num4 += charWidths[j];
		}
		float num9 = base.Size.y * num;
		addQuadIndices(renderData.Vertices, renderData.Triangles);
		RectOffset selectionPadding = getSelectionPadding();
		float num10 = num7 + leftOffset + (float)padding.left * num;
		float x = num10 + Mathf.Min(num8 - num7, num2);
		float num11 = (float)(-(selectionPadding.top + 1)) * num;
		float y = num11 - num9 + (float)(selectionPadding.vertical + 2) * num;
		Vector3 vector = pivot.TransformToUpperLeft(base.Size) * num;
		Vector3 item = new Vector3(num10, num11) + vector;
		Vector3 item2 = new Vector3(x, num11) + vector;
		Vector3 item3 = new Vector3(num10, y) + vector;
		Vector3 item4 = new Vector3(x, y) + vector;
		renderData.Vertices.Add(item);
		renderData.Vertices.Add(item2);
		renderData.Vertices.Add(item4);
		renderData.Vertices.Add(item3);
		Color32 item5 = ApplyOpacity(SelectionBackgroundColor);
		renderData.Colors.Add(item5);
		renderData.Colors.Add(item5);
		renderData.Colors.Add(item5);
		renderData.Colors.Add(item5);
		dfAtlas.ItemInfo itemInfo = base.Atlas[SelectionSprite];
		Rect region = itemInfo.region;
		float num12 = region.width / itemInfo.sizeInPixels.x;
		float num13 = region.height / itemInfo.sizeInPixels.y;
		renderData.UV.Add(new Vector2(region.x + num12, region.yMax - num13));
		renderData.UV.Add(new Vector2(region.xMax - num12, region.yMax - num13));
		renderData.UV.Add(new Vector2(region.xMax - num12, region.y + num13));
		renderData.UV.Add(new Vector2(region.x + num12, region.y + num13));
	}

	private RectOffset getSelectionPadding()
	{
		if (base.Atlas == null)
		{
			return padding;
		}
		dfAtlas.ItemInfo itemInfo = getBackgroundSprite();
		if (itemInfo == null)
		{
			return padding;
		}
		return itemInfo.border;
	}

	private void renderCursor(int startIndex, int cursorIndex, float[] charWidths, float leftOffset)
	{
		if (!string.IsNullOrEmpty(SelectionSprite) && !(base.Atlas == null))
		{
			float num = 0f;
			for (int i = startIndex; i < cursorIndex; i++)
			{
				num += charWidths[i];
			}
			float num2 = PixelsToUnits();
			float num3 = (num + leftOffset + (float)padding.left * num2).Quantize(num2);
			float num4 = (float)(-padding.top) * num2;
			float num5 = num2 * (float)cursorWidth;
			float num6 = (size.y - (float)padding.vertical) * num2;
			Vector3 vector = new Vector3(num3, num4);
			Vector3 vector2 = new Vector3(num3 + num5, num4);
			Vector3 vector3 = new Vector3(num3 + num5, num4 - num6);
			Vector3 vector4 = new Vector3(num3, num4 - num6);
			dfList<Vector3> vertices = renderData.Vertices;
			dfList<int> triangles = renderData.Triangles;
			dfList<Vector2> uV = renderData.UV;
			dfList<Color32> colors = renderData.Colors;
			Vector3 vector5 = pivot.TransformToUpperLeft(size) * num2;
			addQuadIndices(vertices, triangles);
			vertices.Add(vector + vector5);
			vertices.Add(vector2 + vector5);
			vertices.Add(vector3 + vector5);
			vertices.Add(vector4 + vector5);
			Color32 item = ApplyOpacity(CursorColor);
			colors.Add(item);
			colors.Add(item);
			colors.Add(item);
			colors.Add(item);
			dfAtlas.ItemInfo itemInfo = base.Atlas[SelectionSprite];
			Rect region = itemInfo.region;
			uV.Add(new Vector2(region.x, region.yMax));
			uV.Add(new Vector2(region.xMax, region.yMax));
			uV.Add(new Vector2(region.xMax, region.y));
			uV.Add(new Vector2(region.x, region.y));
		}
	}

	private void addQuadIndices(dfList<Vector3> verts, dfList<int> triangles)
	{
		int count = verts.Count;
		int[] array = new int[6] { 0, 1, 3, 3, 1, 2 };
		for (int i = 0; i < array.Length; i++)
		{
			triangles.Add(count + array[i]);
		}
	}

	private int getCharIndexOfMouse(dfMouseEventArgs args)
	{
		Vector2 hitPosition = GetHitPosition(args);
		float num = PixelsToUnits();
		int num2 = scrollIndex;
		float num3 = leftOffset / num;
		for (int i = scrollIndex; i < charWidths.Length; i++)
		{
			num3 += charWidths[i] / num;
			if (num3 < hitPosition.x)
			{
				num2++;
			}
		}
		return num2;
	}

	public dfList<dfRenderData> RenderMultiple()
	{
		if (base.Atlas == null || Font == null)
		{
			return null;
		}
		if (!isVisible)
		{
			return null;
		}
		if (renderData == null)
		{
			renderData = dfRenderData.Obtain();
			textRenderData = dfRenderData.Obtain();
			isControlInvalidated = true;
		}
		Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
		if (!isControlInvalidated)
		{
			for (int i = 0; i < buffers.Count; i++)
			{
				buffers[i].Transform = localToWorldMatrix;
			}
			return buffers;
		}
		buffers.Clear();
		renderData.Clear();
		renderData.Material = base.Atlas.Material;
		renderData.Transform = localToWorldMatrix;
		buffers.Add(renderData);
		textRenderData.Clear();
		textRenderData.Material = base.Atlas.Material;
		textRenderData.Transform = localToWorldMatrix;
		buffers.Add(textRenderData);
		renderBackground();
		renderText(textRenderData);
		isControlInvalidated = false;
		updateCollider();
		return buffers;
	}

	private void bindTextureRebuildCallback()
	{
		if (!isFontCallbackAssigned && !(Font == null) && Font is dfDynamicFont)
		{
			UnityEngine.Font.textureRebuilt += onFontTextureRebuilt;
			isFontCallbackAssigned = true;
		}
	}

	private void unbindTextureRebuildCallback()
	{
		if (isFontCallbackAssigned && !(Font == null))
		{
			if (Font is dfDynamicFont)
			{
				UnityEngine.Font.textureRebuilt -= onFontTextureRebuilt;
			}
			isFontCallbackAssigned = false;
		}
	}

	private void requestCharacterInfo()
	{
		dfDynamicFont dfDynamicFont2 = Font as dfDynamicFont;
		if (!(dfDynamicFont2 == null) && dfFontManager.IsDirty(Font) && !string.IsNullOrEmpty(text))
		{
			float num = TextScale * getTextScaleMultiplier();
			int fontSize = Mathf.CeilToInt((float)font.FontSize * num);
			dfDynamicFont2.AddCharacterRequest(text, fontSize, FontStyle.Normal);
		}
	}

	private void onFontTextureRebuilt(Font font)
	{
		if (Font is dfDynamicFont && font == (Font as dfDynamicFont).BaseFont)
		{
			requestCharacterInfo();
			Invalidate();
		}
	}

	public void UpdateFontInfo()
	{
		requestCharacterInfo();
	}
}
