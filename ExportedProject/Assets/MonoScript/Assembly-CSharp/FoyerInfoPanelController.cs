using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoyerInfoPanelController : MonoBehaviour
{
	public static bool IsTransitioning;

	private static FoyerInfoPanelController m_extantPanelController;

	public PlayableCharacters characterIdentity;

	public tk2dSprite[] scaledSprites;

	public tk2dSprite arrow;

	public dfPanel textPanel;

	public dfPanel itemsPanel;

	public Transform followTransform;

	public Vector3 offset;

	public Vector3 AdditionalDaveOffset;

	private dfPanel m_panel;

	private void SetBadgeVisibility()
	{
	}

	private void ProcessSprite(dfSprite targetSprite, bool playerHas, bool anyHas)
	{
		if (playerHas)
		{
			targetSprite.IsVisible = true;
			targetSprite.Color = Color.white;
		}
		else if (anyHas)
		{
			targetSprite.IsVisible = true;
			targetSprite.Color = new Color(0.35f, 0f, 0f);
		}
		else
		{
			targetSprite.IsVisible = false;
		}
	}

	private bool AnyPlayerElement(int elementIndex)
	{
		return false;
	}

	private IEnumerator Start()
	{
		m_panel = GetComponent<dfPanel>();
		tk2dBaseSprite[] allSprites = GetComponentsInChildren<tk2dBaseSprite>();
		arrow.GetComponent<Renderer>().enabled = false;
		arrow.gameObject.layer = LayerMask.NameToLayer("BG_Critical");
		CharacterSelectFacecardIdleDoer componentInChildren = arrow.GetComponentInChildren<CharacterSelectFacecardIdleDoer>();
		componentInChildren.transform.localPosition = componentInChildren.transform.localPosition.WithY(0f);
		for (int i = 0; i < allSprites.Length; i++)
		{
			allSprites[i].ignoresTiltworldDepth = true;
			allSprites[i].transform.position = allSprites[i].transform.position.WithZ(0f);
		}
		for (int j = 0; j < scaledSprites.Length; j++)
		{
			scaledSprites[j].transform.localScale = GameUIUtility.GetCurrentTK2D_DFScale(m_panel.GetManager()) * Vector3.one;
		}
		base.transform.position = dfFollowObject.ConvertWorldSpaces(followTransform.position + offset + AdditionalDaveOffset, GameManager.Instance.MainCameraController.Camera, m_panel.GUIManager.RenderCamera).WithZ(0f);
		base.transform.position = base.transform.position.Quantize(3f * m_panel.PixelsToUnits());
		if (m_extantPanelController != null)
		{
			if (GameManager.Instance.IsSelectingCharacter)
			{
				yield return StartCoroutine(HandleTransition());
			}
			else
			{
				UnityEngine.Object.Destroy(m_extantPanelController.gameObject);
			}
		}
		m_extantPanelController = this;
		yield return null;
		StartCoroutine(HandleOpen());
	}

	private IEnumerator HandleTransition()
	{
		IsTransitioning = true;
		arrow.gameObject.SetActive(false);
		dfPanel currentTextPanel = m_extantPanelController.m_panel;
		Vector3 initialPosition = m_extantPanelController.arrow.transform.position;
		Vector3 targetPosition = arrow.transform.position;
		float elapsed = 0f;
		float duration = 0.15f;
		currentTextPanel.IsVisible = false;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
			Vector3 currentPosition = Vector3.Lerp(initialPosition, targetPosition, t);
			m_extantPanelController.arrow.transform.position = currentPosition;
			currentTextPanel.IsVisible = false;
			yield return null;
		}
		int targetReticleFrame = m_extantPanelController.arrow.GetComponent<tk2dSpriteAnimator>().CurrentFrame + 1;
		m_extantPanelController.arrow.gameObject.SetActive(false);
		m_extantPanelController.arrow.transform.position = initialPosition;
		currentTextPanel.IsVisible = false;
		arrow.gameObject.SetActive(true);
		arrow.GetComponent<tk2dSpriteAnimator>().Play();
		arrow.GetComponent<tk2dSpriteAnimator>().SetFrame(targetReticleFrame);
		UnityEngine.Object.Destroy(m_extantPanelController.gameObject);
		IsTransitioning = false;
	}

	private void Update()
	{
		if (GameManager.Instance.IsPaused && (bool)arrow && arrow.transform.childCount > 0)
		{
			MeshRenderer component = arrow.transform.GetChild(0).GetComponent<MeshRenderer>();
			if ((bool)component)
			{
				component.enabled = false;
			}
		}
		for (int i = 0; i < scaledSprites.Length; i++)
		{
			scaledSprites[i].transform.localScale = GameUIUtility.GetCurrentTK2D_DFScale(m_panel.GetManager()) * Vector3.one;
		}
	}

	private void OnDestroy()
	{
		if (m_extantPanelController == this)
		{
			m_extantPanelController = null;
		}
	}

	private void LateUpdate()
	{
		base.transform.position = dfFollowObject.ConvertWorldSpaces(followTransform.position + offset + AdditionalDaveOffset, GameManager.Instance.MainCameraController.Camera, m_panel.GUIManager.RenderCamera).WithZ(0f);
		base.transform.position = base.transform.position.QuantizeFloor(m_panel.PixelsToUnits() / (Pixelator.Instance.ScaleTileScale / Pixelator.Instance.CurrentTileScale));
	}

	private IEnumerator HandleOpen()
	{
		float elapsed = 0f;
		float duration = 0.7f;
		textPanel.Width = 1f;
		textPanel.IsVisible = true;
		textPanel.ResolutionChangedPostLayout = null;
		dfPanel obj = textPanel;
		obj.ResolutionChangedPostLayout = (Action<dfControl, Vector3, Vector3>)Delegate.Combine(obj.ResolutionChangedPostLayout, new Action<dfControl, Vector3, Vector3>(ResolutionChangedPanel));
		yield return new WaitForSeconds(0.45f);
		ResolutionChangedPanel(textPanel, Vector3.zero, Vector3.zero);
		if (characterIdentity == PlayableCharacters.Eevee)
		{
			dfLabel component = textPanel.transform.Find("PastKilledLabel").GetComponent<dfLabel>();
			component.ProcessMarkup = true;
			component.ColorizeSymbols = true;
			component.ModifyLocalizedText(component.Text + " (" + 5 + "[sprite \"hbux_text_icon\"])");
		}
		else if (characterIdentity == PlayableCharacters.Gunslinger)
		{
			dfLabel component2 = textPanel.transform.Find("PastKilledLabel").GetComponent<dfLabel>();
			component2.ProcessMarkup = true;
			component2.ColorizeSymbols = true;
			component2.ModifyLocalizedText(component2.Text + " (" + 7 + "[sprite \"hbux_text_icon\"])");
		}
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
			textPanel.Width = (int)Mathf.Lerp(1f, 850f, t);
			yield return null;
		}
	}

	private void ResolutionChangedPanel(dfControl newTextPanel, Vector3 previousRelativePosition, Vector3 newRelativePosition)
	{
		dfLabel component = newTextPanel.transform.Find("NameLabel").GetComponent<dfLabel>();
		dfLabel component2 = newTextPanel.transform.Find("DescLabel").GetComponent<dfLabel>();
		dfLabel component3 = newTextPanel.transform.Find("GunLabel").GetComponent<dfLabel>();
		dfLabel component4 = newTextPanel.transform.Find("PastKilledLabel").GetComponent<dfLabel>();
		if (characterIdentity == PlayableCharacters.Eevee || characterIdentity == PlayableCharacters.Gunslinger || GameStatsManager.Instance.TestPastBeaten(characterIdentity))
		{
			component4.IsVisible = true;
		}
		else
		{
			component4.IsVisible = false;
		}
		float currentTileScale = Pixelator.Instance.CurrentTileScale;
		int num = Mathf.FloorToInt(currentTileScale);
		tk2dBaseSprite sprite = newTextPanel.Parent.GetComponentsInChildren<CharacterSelectFacecardIdleDoer>(true)[0].sprite;
		newTextPanel.transform.position = sprite.transform.position + new Vector3(18f * currentTileScale * component.PixelsToUnits(), 41f * currentTileScale * component.PixelsToUnits(), 0f);
		if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH)
		{
			component.Padding = new RectOffset(2 * num, 2 * num, -2 * num, 0 * num);
			component2.Padding = new RectOffset(2 * num, 2 * num, -2 * num, 0 * num);
			component3.Padding = new RectOffset(2 * num, 2 * num, -2 * num, 0 * num);
			component4.Padding = new RectOffset(2 * num, 2 * num, -2 * num, 0 * num);
		}
		else
		{
			component.Padding = new RectOffset(2 * num, 2 * num, 0, 0);
			component2.Padding = new RectOffset(2 * num, 2 * num, 0, 0);
			component3.Padding = new RectOffset(2 * num, 2 * num, 0, 0);
			component4.Padding = new RectOffset(2 * num, 2 * num, 0, 0);
		}
		component.RelativePosition = new Vector3(currentTileScale * 2f, currentTileScale, 0f);
		component2.RelativePosition = new Vector3(0f, currentTileScale + component.Size.y, 0f) + component.RelativePosition;
		component3.RelativePosition = new Vector3(0f, currentTileScale + component2.Size.y, 0f) + component2.RelativePosition;
		component4.RelativePosition = new Vector3(0f, currentTileScale + component3.Size.y, 0f) + component3.RelativePosition;
		if (!(itemsPanel != null))
		{
			return;
		}
		itemsPanel.RelativePosition = component2.RelativePosition;
		List<dfSprite> list = new List<dfSprite>();
		for (int i = 0; i < itemsPanel.Controls.Count; i++)
		{
			itemsPanel.Controls[i].RelativePosition = itemsPanel.Controls[i].RelativePosition.WithY(((itemsPanel.Height - itemsPanel.Controls[i].Height) / 2f).Quantize(num));
			if (list.Count == 0)
			{
				list.Add(itemsPanel.Controls[i] as dfSprite);
				continue;
			}
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				if (itemsPanel.Controls[i].RelativePosition.x < list[j].RelativePosition.x)
				{
					list.Insert(j, itemsPanel.Controls[i] as dfSprite);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(itemsPanel.Controls[i] as dfSprite);
			}
		}
		itemsPanel.CenterChildControls();
		float width = 0f;
		for (int k = 0; k < list.Count; k++)
		{
			if (k == 0)
			{
				list[k].RelativePosition = list[k].RelativePosition.WithX(num * 4);
			}
			else
			{
				dfSprite dfSprite2 = list[k];
				dfSprite2.RelativePosition = dfSprite2.RelativePosition.WithX(list[k - 1].RelativePosition.x + list[k - 1].Size.x + (float)(num * 4));
			}
			list[k].RelativePosition = list[k].RelativePosition.Quantize(num);
			width = list[k].RelativePosition.x + list[k].Size.x + (float)(num * 4);
		}
		itemsPanel.Width = width;
		component4.RelativePosition = component.RelativePosition + new Vector3(component.Width + (float)num, 0f, 0f);
		if (!component4.Text.StartsWith("("))
		{
			component4.Text = "(" + component4.Text + ")";
		}
		component4.Color = new Color(0.6f, 0.6f, 0.6f);
	}
}
