using System.Runtime.InteropServices;
using UnityEngine;

public class GameCursorController : MonoBehaviour
{
	public struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	public static OverridableBool CursorOverride = new OverridableBool(false);

	public Texture2D normalCursor;

	public Texture2D[] cursors;

	public float sizeMultiplier = 4f;

	private Vector3 lastPosition;

	private RECT m_originalClippingRect;

	private static bool showMouseCursor
	{
		get
		{
			if (CursorOverride.Value)
			{
				return false;
			}
			if (GameManager.Instance.IsLoadingLevel)
			{
				return false;
			}
			if (GameManager.IsReturningToBreach)
			{
				return false;
			}
			if (GameManager.Instance.IsSelectingCharacter && BraveInput.PlayerlessInstance.IsKeyboardAndMouse(true))
			{
				return true;
			}
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				if (!BraveInput.GetInstanceForPlayer(0).HasMouse() && !BraveInput.GetInstanceForPlayer(1).HasMouse())
				{
					return false;
				}
			}
			else if (!BraveInput.GetInstanceForPlayer(0).HasMouse())
			{
				return false;
			}
			return true;
		}
	}

	private static bool showPlayerOneControllerCursor
	{
		get
		{
			if (CursorOverride.Value)
			{
				return false;
			}
			if (GameManager.Instance.IsLoadingLevel)
			{
				return false;
			}
			if (GameManager.IsReturningToBreach)
			{
				return false;
			}
			if (BraveInput.GetInstanceForPlayer(0).IsKeyboardAndMouse())
			{
				return false;
			}
			if (!GameManager.Options.PlayerOneControllerCursor)
			{
				return false;
			}
			return true;
		}
	}

	private static bool showPlayerTwoControllerCursor
	{
		get
		{
			if (GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER)
			{
				return false;
			}
			if (CursorOverride.Value)
			{
				return false;
			}
			if (GameManager.Instance.IsLoadingLevel)
			{
				return false;
			}
			if (GameManager.IsReturningToBreach)
			{
				return false;
			}
			if (BraveInput.GetInstanceForPlayer(1).IsKeyboardAndMouse())
			{
				return false;
			}
			if (!GameManager.Options.PlayerTwoControllerCursor)
			{
				return false;
			}
			return true;
		}
	}

	private void Start()
	{
		GetClipCursor(out m_originalClippingRect);
		Cursor.visible = false;
		Cursor.lockState = ((GameManager.Options.CurrentPreferredFullscreenMode == GameOptions.PreferredFullscreenMode.FULLSCREEN) ? CursorLockMode.Confined : CursorLockMode.None);
	}

	[DllImport("user32.dll")]
	private static extern bool ClipCursor(ref RECT lpRect);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetClipCursor(out RECT rcClip);

	public void ToggleClip(bool clipToWindow)
	{
		if (clipToWindow)
		{
			RECT lpRect = default(RECT);
			lpRect.Left = 0;
			lpRect.Top = 0;
			lpRect.Right = Screen.width - 1;
			lpRect.Bottom = Screen.height - 1;
			ClipCursor(ref lpRect);
		}
		else
		{
			ClipCursor(ref m_originalClippingRect);
		}
	}

	private void OnDestroy()
	{
		ClipCursor(ref m_originalClippingRect);
	}

	public void DrawCursor()
	{
		if (!BraveUtility.isLoadingLevel && GameManager.HasInstance && GameManager.Instance.Dungeon != null)
		{
			Cursor.visible = false;
		}
		if (!GameManager.HasInstance)
		{
			return;
		}
		Texture2D texture2D = normalCursor;
		int currentCursorIndex = GameManager.Options.CurrentCursorIndex;
		if (currentCursorIndex >= 0 && currentCursorIndex < cursors.Length)
		{
			texture2D = cursors[currentCursorIndex];
		}
		if (showMouseCursor)
		{
			Vector2 mousePosition = BraveInput.GetInstanceForPlayer(0).MousePosition;
			mousePosition.y = (float)Screen.height - mousePosition.y;
			Vector2 vector = new Vector2(texture2D.width, texture2D.height) * ((!(Pixelator.Instance != null)) ? 3 : ((int)Pixelator.Instance.ScaleTileScale));
			Rect screenRect = new Rect(mousePosition.x + 0.5f - vector.x / 2f, mousePosition.y + 0.5f - vector.y / 2f, vector.x, vector.y);
			bool flag = false;
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && BraveInput.GetInstanceForPlayer(1).IsKeyboardAndMouse())
			{
				flag = true;
				Graphics.DrawTexture(screenRect, texture2D, new Rect(0f, 0f, 1f, 1f), 0, 0, 0, 0, new Color(0.402f, 0.111f, 0.32f));
			}
			if (!flag)
			{
				Graphics.DrawTexture(screenRect, texture2D);
			}
		}
		if (showPlayerOneControllerCursor && !GameManager.Instance.IsPaused && !GameManager.IsBossIntro)
		{
			PlayerController primaryPlayer = GameManager.Instance.PrimaryPlayer;
			BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer(0);
			if ((bool)primaryPlayer && instanceForPlayer.ActiveActions.Aim.Vector != Vector2.zero && (primaryPlayer.CurrentInputState == PlayerInputState.AllInput || primaryPlayer.IsInMinecart))
			{
				Vector2 pos = Camera.main.WorldToViewportPoint(primaryPlayer.CenterPosition + instanceForPlayer.ActiveActions.Aim.Vector.normalized * 5f);
				Vector2 vector2 = BraveCameraUtility.ConvertGameViewportToScreenViewport(pos);
				Vector2 vector3 = new Vector2(vector2.x * (float)Screen.width, (1f - vector2.y) * (float)Screen.height);
				Vector2 vector4 = new Vector2(texture2D.width, texture2D.height) * ((!(Pixelator.Instance != null)) ? 3 : ((int)Pixelator.Instance.ScaleTileScale));
				Rect screenRect2 = new Rect(vector3.x + 0.5f - vector4.x / 2f, vector3.y + 0.5f - vector4.y / 2f, vector4.x, vector4.y);
				Graphics.DrawTexture(screenRect2, texture2D);
			}
		}
		if (showPlayerTwoControllerCursor && !GameManager.Instance.IsPaused && !GameManager.IsBossIntro)
		{
			PlayerController secondaryPlayer = GameManager.Instance.SecondaryPlayer;
			BraveInput instanceForPlayer2 = BraveInput.GetInstanceForPlayer(1);
			if ((bool)secondaryPlayer && instanceForPlayer2.ActiveActions.Aim.Vector != Vector2.zero && (secondaryPlayer.CurrentInputState == PlayerInputState.AllInput || secondaryPlayer.IsInMinecart))
			{
				Vector2 pos2 = Camera.main.WorldToViewportPoint(secondaryPlayer.CenterPosition + instanceForPlayer2.ActiveActions.Aim.Vector.normalized * 5f);
				Vector2 vector5 = BraveCameraUtility.ConvertGameViewportToScreenViewport(pos2);
				Vector2 vector6 = new Vector2(vector5.x * (float)Screen.width, (1f - vector5.y) * (float)Screen.height);
				Vector2 vector7 = new Vector2(texture2D.width, texture2D.height) * ((!(Pixelator.Instance != null)) ? 3 : ((int)Pixelator.Instance.ScaleTileScale));
				Rect screenRect3 = new Rect(vector6.x + 0.5f - vector7.x / 2f, vector6.y + 0.5f - vector7.y / 2f, vector7.x, vector7.y);
				Graphics.DrawTexture(screenRect3, texture2D, new Rect(0f, 0f, 1f, 1f), 0, 0, 0, 0, new Color(0.402f, 0.111f, 0.32f));
			}
		}
		if (GameManager.Options.CurrentPreferredFullscreenMode == GameOptions.PreferredFullscreenMode.FULLSCREEN)
		{
			Cursor.lockState = ((!GameManager.Instance.IsPaused || (!GameUIRoot.Instance.PauseMenuPanel.IsVisible && !GameUIRoot.Instance.HasOpenPauseSubmenu())) ? CursorLockMode.Confined : CursorLockMode.None);
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
		}
	}

	private void OnGUI()
	{
		DrawCursor();
	}
}
