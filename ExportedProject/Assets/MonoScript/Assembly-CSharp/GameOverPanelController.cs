using UnityEngine;

public class GameOverPanelController : TimeInvariantMonoBehaviour
{
	public dfButton QuickRestartButton;

	public dfButton MainMenuButton;

	public tk2dSprite deathGuyLeft;

	public tk2dSprite deathGuyRight;

	private dfPanel m_panel;

	private void Start()
	{
		m_panel = GetComponent<dfPanel>();
		QuickRestartButton.Click += DoQuickRestart;
		MainMenuButton.Click += DoMainMenu;
	}

	private void DoMainMenu(dfControl control, dfMouseEventArgs mouseEvent)
	{
		if (m_panel.IsVisible)
		{
			m_panel.IsVisible = false;
			dfGUIManager.PopModal();
			Pixelator.Instance.DoFinalNonFadedLayer = false;
			GameUIRoot.Instance.ToggleUICamera(false);
			Pixelator.Instance.FadeToBlack(0.15f);
			GameManager.Instance.DelayedLoadMainMenu(0.15f);
			AkSoundEngine.PostEvent("Play_UI_menu_cancel_01", base.gameObject);
		}
	}

	private void DoQuickRestart(dfControl control, dfMouseEventArgs mouseEvent)
	{
		if (m_panel.IsVisible)
		{
			m_panel.IsVisible = false;
			dfGUIManager.PopModal();
			Pixelator.Instance.DoFinalNonFadedLayer = false;
			GameUIRoot.Instance.ToggleUICamera(false);
			Pixelator.Instance.FadeToBlack(0.15f);
			GameManager.Instance.DelayedQuickRestart(0.15f);
			AkSoundEngine.PostEvent("Play_UI_menu_characterselect_01", base.gameObject);
		}
	}

	public void Activate()
	{
		QuickRestartButton.Focus();
		deathGuyLeft.ignoresTiltworldDepth = true;
		deathGuyRight.ignoresTiltworldDepth = true;
		UpdateDeathGuys();
	}

	protected void UpdateDeathGuys()
	{
		deathGuyLeft.scale = GameUIUtility.GetCurrentTK2D_DFScale(m_panel.GetManager()) * Vector3.one;
		deathGuyRight.scale = deathGuyLeft.scale.WithX(deathGuyLeft.scale.x * -1f);
		Vector3 vector = (m_panel.Size.ToVector3ZUp() + new Vector3(36f, 0f, 0f)) * m_panel.PixelsToUnits() * 0.5f;
		deathGuyLeft.transform.position = m_panel.transform.position - vector + new Vector3(0f - deathGuyLeft.GetBounds().size.x, 0f, 0f);
		deathGuyRight.transform.position = m_panel.transform.position + new Vector3(vector.x, 0f - vector.y, vector.z) + new Vector3(deathGuyRight.GetBounds().size.x, 0f, 0f);
		deathGuyLeft.renderer.enabled = true;
		deathGuyRight.renderer.enabled = true;
	}

	protected override void InvariantUpdate(float realDeltaTime)
	{
		if (m_panel.IsVisible)
		{
			UpdateDeathGuys();
			if (deathGuyLeft.renderer.enabled)
			{
				deathGuyLeft.spriteAnimator.UpdateAnimation(realDeltaTime);
			}
			if (deathGuyRight.renderer.enabled)
			{
				deathGuyRight.spriteAnimator.UpdateAnimation(realDeltaTime);
			}
			GameUIRoot.Instance.ForceClearReload();
		}
	}

	public void Deactivate()
	{
		deathGuyRight.renderer.enabled = false;
		deathGuyLeft.renderer.enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
