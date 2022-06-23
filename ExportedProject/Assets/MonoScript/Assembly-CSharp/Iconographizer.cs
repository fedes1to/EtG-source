using System.Collections;
using System.Collections.ObjectModel;
using InControl;
using UnityEngine;

public class Iconographizer : MonoBehaviour
{
	public string SonySpriteName;

	public string XboxSpriteName;

	public string SwitchSpriteName;

	public string SwitchSingleJoyConSpriteName;

	public bool InteractOverride;

	public bool DoResize;

	[ShowInInspectorIf("DoResize", true)]
	public float ResizeScale = 3f;

	private dfSprite m_control;

	private tk2dBaseSprite m_sprite;

	private IEnumerator Start()
	{
		yield return null;
		m_sprite = GetComponent<tk2dBaseSprite>();
		m_control = GetComponent<dfSprite>();
		if ((bool)m_control)
		{
			m_control.IsVisibleChanged += HandleVisibilityChanged;
			HandleVisibilityChanged(m_control, false);
		}
		else if ((bool)m_sprite)
		{
			string sprite;
			switch (BraveInput.PlayerOneCurrentSymbology)
			{
			case GameOptions.ControllerSymbology.PS4:
				sprite = SonySpriteName;
				break;
			case GameOptions.ControllerSymbology.Switch:
				sprite = SwitchSpriteName;
				break;
			default:
				sprite = XboxSpriteName;
				break;
			}
			m_sprite.SetSprite(sprite);
		}
	}

	private void HandleVisibilityChanged(dfControl control, bool value)
	{
		string text = null;
		Vector2 vector = (Vector2)m_control.Position + new Vector2(m_control.Size.x / 2f, (0f - m_control.Size.y) / 2f);
		text = ((BraveInput.PlayerOneCurrentSymbology == GameOptions.ControllerSymbology.PS4) ? SonySpriteName : ((BraveInput.PlayerOneCurrentSymbology != GameOptions.ControllerSymbology.Switch) ? XboxSpriteName : SwitchSpriteName));
		if (InteractOverride)
		{
			string controllerInteractKey = GetControllerInteractKey();
			if (controllerInteractKey != null)
			{
				text = controllerInteractKey;
			}
		}
		if (text != null)
		{
			m_control.SpriteName = text;
			if (DoResize)
			{
				m_control.Size = m_control.SpriteInfo.sizeInPixels * ResizeScale;
				m_control.Position = vector + new Vector2((0f - m_control.Size.x) / 2f, m_control.Size.y / 2f);
			}
		}
	}

	private string GetControllerInteractKey()
	{
		if (!Minimap.HasInstance)
		{
			return null;
		}
		BraveInput primaryPlayerInstance = BraveInput.PrimaryPlayerInstance;
		if (primaryPlayerInstance == null || primaryPlayerInstance.IsKeyboardAndMouse())
		{
			return null;
		}
		GungeonActions activeActions = primaryPlayerInstance.ActiveActions;
		if (activeActions != null && activeActions.InteractAction.Bindings.Count > 0)
		{
			ReadOnlyCollection<BindingSource> bindings = activeActions.InteractAction.Bindings;
			for (int i = 0; i < bindings.Count; i++)
			{
				DeviceBindingSource deviceBindingSource = bindings[i] as DeviceBindingSource;
				if (deviceBindingSource != null && deviceBindingSource.Control != 0)
				{
					return UIControllerButtonHelper.GetControllerButtonSpriteName(deviceBindingSource.Control, BraveInput.PlayerOneCurrentSymbology);
				}
			}
		}
		return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.Action1, BraveInput.PlayerOneCurrentSymbology);
	}
}
