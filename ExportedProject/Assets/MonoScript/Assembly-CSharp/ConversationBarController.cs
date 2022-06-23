using System.Collections;
using Dungeonator;
using UnityEngine;

public class ConversationBarController : MonoBehaviour
{
	public Color selectedColor = Color.white;

	public Color unselectedColor = Color.gray;

	public dfButton textOption1;

	public dfButton textOption2;

	public dfSprite reticleLeft1;

	public dfSprite reticleRight1;

	public dfSprite reticleLeft2;

	public dfSprite reticleRight2;

	public dfSprite portraitSprite;

	public Texture2D EeveeTex;

	private dfSprite m_conversationBarSprite;

	private bool m_isActive;

	private PlayerController m_lastAssignedPlayer;

	private bool m_temporarilyHidden;

	private dfPanel m_motionGroup;

	private bool m_portraitAdjustedForSmallUI;

	public bool IsActive
	{
		get
		{
			return m_isActive;
		}
	}

	public void HideBar()
	{
		m_isActive = false;
		if (m_conversationBarSprite == null)
		{
			m_conversationBarSprite = GetComponent<dfSprite>();
		}
		if (m_motionGroup != null)
		{
			GameUIRoot.Instance.MoveNonCoreGroupOnscreen(m_conversationBarSprite, true);
		}
		StartCoroutine(DelayedHide());
	}

	private IEnumerator DelayedHide()
	{
		yield return new WaitForSeconds(0.25f);
		if (!m_isActive)
		{
			m_conversationBarSprite.IsVisible = false;
			textOption1.IsVisible = false;
			textOption2.IsVisible = false;
		}
	}

	public void SetSelectedResponse(int selectedResponse)
	{
		switch (selectedResponse)
		{
		case 0:
			textOption1.TextColor = selectedColor;
			textOption2.TextColor = unselectedColor;
			reticleLeft1.IsVisible = false;
			reticleRight1.IsVisible = false;
			reticleLeft2.IsVisible = false;
			reticleRight2.IsVisible = false;
			break;
		case 1:
			textOption1.TextColor = unselectedColor;
			textOption2.TextColor = selectedColor;
			reticleLeft1.IsVisible = false;
			reticleRight1.IsVisible = false;
			reticleLeft2.IsVisible = false;
			reticleRight2.IsVisible = false;
			break;
		}
	}

	public void LateUpdate()
	{
		if (m_temporarilyHidden && !GameManager.Instance.IsPaused)
		{
			GameUIRoot.Instance.MoveNonCoreGroupOnscreen(m_conversationBarSprite);
			m_temporarilyHidden = false;
		}
		if (!textOption1.IsVisible)
		{
			return;
		}
		if (!m_temporarilyHidden && GameManager.Instance.IsPaused)
		{
			GameUIRoot.Instance.MoveNonCoreGroupOnscreen(m_conversationBarSprite, true);
			m_temporarilyHidden = true;
		}
		else
		{
			if (!BraveInput.GetInstanceForPlayer(m_lastAssignedPlayer.PlayerIDX).IsKeyboardAndMouse())
			{
				return;
			}
			Vector2 point = Input.mousePosition.WithY((float)Screen.height - Input.mousePosition.y).XY();
			if (textOption1.IsVisible && textOption1.GetScreenRect().Contains(point))
			{
				HandleOptionHover(textOption1);
				if (Input.GetMouseButtonDown(0))
				{
					HandleOptionClick(textOption1);
					m_lastAssignedPlayer.SuppressThisClick = true;
				}
			}
			if (textOption2.IsVisible && textOption2.GetScreenRect().Contains(point))
			{
				HandleOptionHover(textOption2);
				if (Input.GetMouseButtonDown(0))
				{
					HandleOptionClick(textOption2);
					m_lastAssignedPlayer.SuppressThisClick = true;
				}
			}
		}
	}

	private void UpdateScaleAndPosition(dfControl c, float newScalar, bool doVerticalAdjustment = true)
	{
		if (c.transform.localScale.x != newScalar)
		{
			float x = c.transform.localScale.x;
			c.transform.localScale = new Vector3(newScalar, newScalar, 1f);
			c.RelativePosition = new Vector3(c.RelativePosition.x * (newScalar / x), (!doVerticalAdjustment) ? c.RelativePosition.y : (c.RelativePosition.y + ((!(x < newScalar)) ? (0f - c.Height) : c.Height)), c.RelativePosition.z);
		}
	}

	public void ShowBar(PlayerController interactingPlayer, string[] responses)
	{
		GameUIRoot.Instance.notificationController.ForceHide();
		UpdateScaleAndPosition(reticleLeft1, 1f / GameUIRoot.GameUIScalar);
		UpdateScaleAndPosition(reticleLeft2, 1f / GameUIRoot.GameUIScalar);
		UpdateScaleAndPosition(portraitSprite, 1f / GameUIRoot.GameUIScalar);
		bool flag = false;
		if (!m_conversationBarSprite)
		{
			flag = true;
			m_conversationBarSprite = GetComponent<dfSprite>();
			m_motionGroup = GameUIRoot.Instance.AddControlToMotionGroups(m_conversationBarSprite, DungeonData.Direction.SOUTH, true);
		}
		if (m_conversationBarSprite.Parent.transform.localScale.x != 1f / GameUIRoot.GameUIScalar)
		{
			m_conversationBarSprite.Parent.transform.localScale = new Vector3(1f / GameUIRoot.GameUIScalar, 1f / GameUIRoot.GameUIScalar, 1f);
			if (flag)
			{
				m_conversationBarSprite.Parent.RelativePosition = m_conversationBarSprite.Parent.RelativePosition.WithY(m_conversationBarSprite.Parent.Height * 3f);
			}
		}
		if (interactingPlayer.characterIdentity == PlayableCharacters.Eevee)
		{
			Material material = Object.Instantiate(portraitSprite.Atlas.Material);
			material.shader = Shader.Find("Brave/Internal/GlitchEevee");
			material.SetTexture("_EeveeTex", EeveeTex);
			material.SetFloat("_WaveIntensity", 0.1f);
			material.SetFloat("_ColorIntensity", 0.015f);
			portraitSprite.OverrideMaterial = material;
		}
		else
		{
			portraitSprite.OverrideMaterial = null;
		}
		m_isActive = true;
		m_lastAssignedPlayer = interactingPlayer;
		GameUIRoot.Instance.MoveNonCoreGroupOnscreen(m_conversationBarSprite);
		m_conversationBarSprite.BringToFront();
		if (interactingPlayer.characterIdentity == PlayableCharacters.Eevee)
		{
			switch (Random.Range(0, 4))
			{
			case 0:
				portraitSprite.SpriteName = "talking_bar_character_window_rogue_003";
				break;
			case 1:
				portraitSprite.SpriteName = "talking_bar_character_window_marine_003";
				break;
			case 2:
				portraitSprite.SpriteName = "talking_bar_character_window_guide_003";
				break;
			case 3:
				portraitSprite.SpriteName = "talking_bar_character_window_convict_003";
				break;
			default:
				portraitSprite.SpriteName = "talking_bar_character_window_guide_003";
				break;
			}
		}
		else
		{
			portraitSprite.SpriteName = interactingPlayer.uiPortraitName;
		}
		if (GameManager.Options.SmallUIEnabled)
		{
			if (!m_portraitAdjustedForSmallUI)
			{
				portraitSprite.Size /= 2f;
				portraitSprite.RelativePosition -= new Vector3(0f, portraitSprite.Size.y * 2f, 0f);
				m_portraitAdjustedForSmallUI = true;
			}
		}
		else if (m_portraitAdjustedForSmallUI)
		{
			portraitSprite.RelativePosition += new Vector3(0f, portraitSprite.Size.y * 2f, 0f);
			portraitSprite.Size *= 2f;
			m_portraitAdjustedForSmallUI = false;
		}
		m_conversationBarSprite.IsVisible = true;
		textOption1.IsVisible = true;
		textOption1.Text = responses[0];
		reticleRight1.RelativePosition = reticleLeft1.RelativePosition.WithX(reticleLeft1.RelativePosition.x + reticleLeft1.Width + textOption1.GetAutosizeWidth() + 24f);
		if (responses != null && responses.Length > 1)
		{
			textOption2.IsVisible = true;
			textOption2.Text = responses[1];
			reticleRight2.RelativePosition = reticleLeft2.RelativePosition.WithX(reticleLeft2.RelativePosition.x + reticleLeft2.Width + textOption2.GetAutosizeWidth() + 24f);
		}
		else
		{
			textOption2.IsVisible = false;
			textOption2.Text = string.Empty;
		}
	}

	private void HandleOptionHover(dfControl control)
	{
		if (control == textOption1)
		{
			GameUIRoot.Instance.SetConversationResponse(0);
		}
		if (control == textOption2)
		{
			GameUIRoot.Instance.SetConversationResponse(1);
		}
	}

	private void HandleOptionClick(dfControl control)
	{
		if (control == textOption1)
		{
			GameUIRoot.Instance.SetConversationResponse(0);
		}
		if (control == textOption2)
		{
			GameUIRoot.Instance.SetConversationResponse(1);
		}
		GameUIRoot.Instance.SelectConversationResponse();
	}
}
