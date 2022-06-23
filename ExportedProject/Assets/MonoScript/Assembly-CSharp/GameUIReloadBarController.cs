using System;
using System.Collections;
using UnityEngine;

public class GameUIReloadBarController : BraveBehaviour
{
	public dfSprite activeReloadSprite;

	public dfSlider progressSlider;

	public dfSprite celebrationSprite;

	public int startValue;

	public int endValue;

	public int lieFactor = 3;

	public dfFollowObject follower;

	public float initialDelay = 0.1f;

	public float finalDelay = 0.25f;

	[Header("Status Bars")]
	public dfProgressBar statusBarDrain;

	public dfProgressBar statusBarPoison;

	public dfProgressBar statusBarFire;

	public dfProgressBar statusBarCurse;

	private int m_activeReloadStartValue;

	private int m_activeReloadEndValue;

	private bool m_reloadIsActive;

	private Vector3 m_worldOffset;

	private Vector3 m_screenOffset;

	private PlayerController m_attachPlayer;

	private Camera worldCamera;

	private Camera uiCamera;

	private dfPanel StatusBarPanel;

	private OverridableBool m_isInvisible = new OverridableBool(false);

	private bool m_isReloading;

	public bool ReloadIsActive
	{
		get
		{
			return m_reloadIsActive;
		}
	}

	private void Awake()
	{
		if (statusBarDrain != null)
		{
			StatusBarPanel = statusBarDrain.Parent as dfPanel;
		}
	}

	private void Start()
	{
		CameraController mainCameraController = GameManager.Instance.MainCameraController;
		mainCameraController.OnFinishedFrame = (Action)Delegate.Combine(mainCameraController.OnFinishedFrame, new Action(OnMainCameraFinishedFrame));
	}

	public void SetInvisibility(bool visible, string reason)
	{
		m_isInvisible.SetOverride(reason, visible);
	}

	public void TriggerReload(PlayerController attachParent, Vector3 offset, float duration, float activeReloadStartPercent, int pixelWidth)
	{
		progressSlider.transform.localScale = Vector3.one / GameUIRoot.GameUIScalar;
		progressSlider.IsVisible = true;
		m_attachPlayer = attachParent;
		worldCamera = GameManager.Instance.MainCameraController.GetComponent<Camera>();
		uiCamera = progressSlider.GetManager().RenderCamera;
		m_worldOffset = offset;
		m_screenOffset = new Vector3((0f - progressSlider.Width) / (2f * GameUIRoot.GameUIScalar) * progressSlider.PixelsToUnits(), 0f, 0f);
		m_reloadIsActive = true;
		activeReloadSprite.enabled = true;
		progressSlider.Thumb.enabled = true;
		celebrationSprite.enabled = true;
		dfSpriteAnimation component = celebrationSprite.GetComponent<dfSpriteAnimation>();
		component.Stop();
		component.SetFrameExternal(0);
		celebrationSprite.enabled = false;
		progressSlider.Color = Color.white;
		float width = progressSlider.Width;
		float maxValue = progressSlider.MaxValue;
		float num = (float)startValue / maxValue * width;
		float num2 = (float)endValue / maxValue * width;
		float x = num + (num2 - num) * activeReloadStartPercent;
		float width2 = (float)pixelWidth * Pixelator.Instance.CurrentTileScale;
		activeReloadSprite.RelativePosition = GameUIUtility.QuantizeUIPosition(activeReloadSprite.RelativePosition.WithX(x));
		celebrationSprite.RelativePosition = activeReloadSprite.RelativePosition + new Vector3(Pixelator.Instance.CurrentTileScale * -1f, Pixelator.Instance.CurrentTileScale * -2f, 0f);
		activeReloadSprite.Width = width2;
		m_activeReloadStartValue = Mathf.RoundToInt((float)(endValue - startValue) * activeReloadStartPercent) + startValue - lieFactor / 2;
		m_activeReloadEndValue = m_activeReloadStartValue + lieFactor;
		bool flag = (bool)attachParent && (bool)attachParent.CurrentGun && attachParent.CurrentGun.LocalActiveReload;
		if (attachParent.IsPrimaryPlayer)
		{
			activeReloadSprite.IsVisible = Gun.ActiveReloadActivated || flag;
		}
		else
		{
			activeReloadSprite.IsVisible = Gun.ActiveReloadActivatedPlayerTwo || flag;
		}
		StartCoroutine(HandlePlayerReloadBar(duration));
	}

	public bool IsActiveReloadGracePeriod()
	{
		if (progressSlider.Value <= 0.3f * progressSlider.MaxValue)
		{
			return true;
		}
		return false;
	}

	public bool AttemptActiveReload()
	{
		if (!m_reloadIsActive)
		{
			return false;
		}
		if (progressSlider.Value >= (float)m_activeReloadStartValue && progressSlider.Value <= (float)m_activeReloadEndValue)
		{
			progressSlider.Color = Color.green;
			AkSoundEngine.PostEvent("Play_WPN_active_reload_01", base.gameObject);
			celebrationSprite.enabled = true;
			activeReloadSprite.enabled = false;
			progressSlider.Thumb.enabled = false;
			m_reloadIsActive = false;
			celebrationSprite.GetComponent<dfSpriteAnimation>().Play();
			return true;
		}
		progressSlider.Color = Color.red;
		return false;
	}

	public void CancelReload()
	{
		m_reloadIsActive = false;
		m_isReloading = false;
		progressSlider.IsVisible = false;
	}

	private Vector3 ConvertWorldSpaces(Vector3 inPoint, Camera inCamera, Camera outCamera)
	{
		Vector3 position = inCamera.WorldToViewportPoint(inPoint);
		return outCamera.ViewportToWorldPoint(position);
	}

	private IEnumerator HandlePlayerReloadBar(float duration)
	{
		m_isReloading = true;
		float elapsed2 = 0f;
		activeReloadSprite.RelativePosition = GameUIUtility.QuantizeUIPosition(activeReloadSprite.RelativePosition);
		while (m_reloadIsActive)
		{
			elapsed2 += BraveTime.DeltaTime;
			float modifiedElapsed = Mathf.Max(0f, elapsed2 - initialDelay);
			float completedFraction = Mathf.Clamp01(modifiedElapsed / (duration - initialDelay));
			int intValue = startValue + Mathf.RoundToInt((float)(endValue - startValue) * completedFraction);
			progressSlider.Value = intValue;
			progressSlider.IsVisible = !m_isInvisible.Value;
			if (elapsed2 > duration)
			{
				m_reloadIsActive = false;
			}
			yield return null;
		}
		elapsed2 = 0f;
		if (m_isReloading)
		{
			while (elapsed2 < finalDelay)
			{
				elapsed2 += BraveTime.DeltaTime;
				yield return null;
			}
		}
		if (!m_reloadIsActive)
		{
			progressSlider.Value = startValue;
			progressSlider.IsVisible = false;
		}
		m_isReloading = false;
	}

	public bool AnyStatusBarVisible()
	{
		return statusBarDrain.IsVisible || statusBarCurse.IsVisible || statusBarFire.IsVisible || statusBarPoison.IsVisible;
	}

	public void UpdateStatusBars(PlayerController player)
	{
		if (statusBarPoison == null || statusBarDrain == null || statusBarPoison == null)
		{
			return;
		}
		StatusBarPanel.transform.localScale = Vector3.one / GameUIRoot.GameUIScalar;
		if (!player || player.healthHaver.IsDead || GameManager.Instance.IsPaused)
		{
			statusBarPoison.IsVisible = false;
			statusBarDrain.IsVisible = false;
			statusBarFire.IsVisible = false;
			statusBarCurse.IsVisible = false;
			return;
		}
		m_attachPlayer = player;
		worldCamera = GameManager.Instance.MainCameraController.GetComponent<Camera>();
		uiCamera = progressSlider.GetManager().RenderCamera;
		m_worldOffset = new Vector3(0.1f, player.SpriteDimensions.y / 2f + 0.25f, 0f);
		m_screenOffset = new Vector3((0f - progressSlider.Width) / (2f * GameUIRoot.GameUIScalar) * progressSlider.PixelsToUnits(), 0f, 0f);
		if (player.CurrentPoisonMeterValue > 0f)
		{
			statusBarPoison.IsVisible = true;
			statusBarPoison.Value = player.CurrentPoisonMeterValue;
		}
		else
		{
			statusBarPoison.IsVisible = false;
		}
		if (player.CurrentCurseMeterValue > 0f)
		{
			statusBarCurse.IsVisible = true;
			statusBarCurse.Value = player.CurrentCurseMeterValue;
		}
		else
		{
			statusBarCurse.IsVisible = false;
		}
		if (player.IsOnFire)
		{
			statusBarFire.IsVisible = true;
			statusBarFire.Value = player.CurrentFireMeterValue;
		}
		else
		{
			statusBarFire.IsVisible = false;
		}
		if (player.CurrentDrainMeterValue > 0f)
		{
			statusBarDrain.IsVisible = true;
			statusBarDrain.Value = player.CurrentDrainMeterValue;
		}
		else
		{
			statusBarDrain.IsVisible = false;
		}
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			dfProgressBar dfProgressBar2 = null;
			switch (i)
			{
			case 0:
				dfProgressBar2 = statusBarPoison;
				break;
			case 1:
				dfProgressBar2 = statusBarDrain;
				break;
			case 2:
				dfProgressBar2 = statusBarFire;
				break;
			case 3:
				dfProgressBar2 = statusBarCurse;
				break;
			}
			if (dfProgressBar2.IsVisible)
			{
				num++;
			}
		}
		float num2 = 0f;
		int num3 = (num - 1) * 18;
		for (int j = 0; j < 4; j++)
		{
			dfProgressBar dfProgressBar3 = null;
			switch (j)
			{
			case 0:
				dfProgressBar3 = statusBarPoison;
				break;
			case 1:
				dfProgressBar3 = statusBarDrain;
				break;
			case 2:
				dfProgressBar3 = statusBarFire;
				break;
			case 3:
				dfProgressBar3 = statusBarCurse;
				break;
			}
			if (dfProgressBar3.IsVisible)
			{
				float x = num3;
				if (num3 != 0)
				{
					x = Mathf.Lerp(-num3, num3, num2 / ((float)num - 1f));
				}
				dfProgressBar3.RelativePosition = new Vector3(36f, -12f / GameUIRoot.GameUIScalar, 0f) + new Vector3(x, 0f, 0f);
				num2 += 1f;
			}
		}
	}

	private void OnMainCameraFinishedFrame()
	{
		if ((bool)m_attachPlayer && (progressSlider.IsVisible || AnyStatusBarVisible()))
		{
			Vector2 vector = m_attachPlayer.LockedApproximateSpriteCenter + m_worldOffset;
			Vector2 vector2 = ConvertWorldSpaces(vector, worldCamera, uiCamera).WithZ(0f) + m_screenOffset;
			progressSlider.transform.position = vector2;
			progressSlider.transform.position = progressSlider.transform.position.QuantizeFloor(progressSlider.PixelsToUnits() / (Pixelator.Instance.ScaleTileScale / Pixelator.Instance.CurrentTileScale));
			if (StatusBarPanel != null)
			{
				StatusBarPanel.transform.position = progressSlider.transform.position - new Vector3(0f, -48f * progressSlider.PixelsToUnits(), 0f);
			}
		}
	}

	protected override void OnDestroy()
	{
		if (GameManager.HasInstance && (bool)GameManager.Instance.MainCameraController)
		{
			CameraController mainCameraController = GameManager.Instance.MainCameraController;
			mainCameraController.OnFinishedFrame = (Action)Delegate.Remove(mainCameraController.OnFinishedFrame, new Action(OnMainCameraFinishedFrame));
		}
		base.OnDestroy();
	}
}
