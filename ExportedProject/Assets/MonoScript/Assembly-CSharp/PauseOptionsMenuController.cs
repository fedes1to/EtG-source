using UnityEngine;

public class PauseOptionsMenuController : MonoBehaviour
{
	public dfProgressBar MusicVolumeSlider;

	public dfProgressBar SoundVolumeSlider;

	public dfProgressBar UIVolumeSlider;

	public dfButton HeadphonesButton;

	public dfButton SpeakersButton;

	public dfButton AcceptButton;

	protected dfPanel m_panel;

	public bool IsVisible
	{
		get
		{
			return m_panel.IsVisible;
		}
		set
		{
			if (m_panel.IsVisible != value)
			{
				InitializeFromOptions();
				m_panel.IsVisible = value;
				if (value)
				{
					dfGUIManager.PushModal(m_panel);
				}
				else if (dfGUIManager.GetModalControl() == m_panel)
				{
					dfGUIManager.PopModal();
				}
				else
				{
					Debug.LogError("failure.");
				}
			}
		}
	}

	public void InitializeFromOptions()
	{
		Debug.Log("initializing...");
		MusicVolumeSlider.Value = GameManager.Options.MusicVolume;
		SoundVolumeSlider.Value = GameManager.Options.SoundVolume;
		if (UIVolumeSlider != null)
		{
			UIVolumeSlider.Value = GameManager.Options.UIVolume;
		}
		switch (GameManager.Options.AudioHardware)
		{
		case GameOptions.AudioHardwareMode.HEADPHONES:
			HeadphonesButton.ForceState(dfButton.ButtonState.Pressed);
			break;
		case GameOptions.AudioHardwareMode.SPEAKERS:
			SpeakersButton.ForceState(dfButton.ButtonState.Pressed);
			break;
		}
	}

	private void Start()
	{
		m_panel = GetComponent<dfPanel>();
		InitializeFromOptions();
		MusicVolumeSlider.ValueChanged += delegate(dfControl control, float value)
		{
			GameManager.Options.MusicVolume = value;
		};
		SoundVolumeSlider.ValueChanged += delegate(dfControl control, float value)
		{
			GameManager.Options.SoundVolume = value;
		};
		if (UIVolumeSlider != null)
		{
			UIVolumeSlider.ValueChanged += delegate(dfControl control, float value)
			{
				GameManager.Options.UIVolume = value;
			};
		}
		HeadphonesButton.Click += delegate
		{
			AkSoundEngine.PostEvent("Play_UI_menu_confirm_01", base.gameObject);
			HeadphonesButton.ForceState(dfButton.ButtonState.Pressed);
			SpeakersButton.ForceState(dfButton.ButtonState.Default);
			GameManager.Options.AudioHardware = GameOptions.AudioHardwareMode.HEADPHONES;
		};
		SpeakersButton.Click += delegate
		{
			AkSoundEngine.PostEvent("Play_UI_menu_confirm_01", base.gameObject);
			HeadphonesButton.ForceState(dfButton.ButtonState.Default);
			SpeakersButton.ForceState(dfButton.ButtonState.Pressed);
			GameManager.Options.AudioHardware = GameOptions.AudioHardwareMode.SPEAKERS;
		};
		AcceptButton.Click += delegate
		{
			IsVisible = false;
			GameUIRoot.Instance.ShowPauseMenu();
		};
	}
}
