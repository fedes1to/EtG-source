using System;
using System.Collections.Generic;
using UnityEngine;

public class AmmonomiconPokedexEntry : MonoBehaviour
{
	public enum EncounterState
	{
		ENCOUNTERED,
		KNOWN,
		UNKNOWN
	}

	public bool IsEquipmentPage;

	public bool ForceEncounterState;

	public EncounterState encounterState;

	public EncounterDatabaseEntry linkedEncounterTrackable;

	public int pickupID = -1;

	public dfSprite questionMarkSprite;

	public List<AdvancedSynergyEntry> activeSynergies;

	private tk2dClippedSprite m_childSprite;

	private dfButton m_button;

	private dfSlicedSprite m_bgSprite;

	[NonSerialized]
	public bool IsGunderfury;

	private const string c_flatSprite = "big_box_page_flat_001";

	private const string c_raisedSprite = "big_box_page_raised_001";

	private const string c_raisedSelectedSprite = "big_box_page_raised_selected_001";

	private dfInputManager m_inputAdapter;

	private List<tk2dClippedSprite> extantSynergyArrows = new List<tk2dClippedSprite>();

	public tk2dClippedSprite ChildSprite
	{
		get
		{
			return m_childSprite;
		}
	}

	private void Awake()
	{
		m_button = GetComponent<dfButton>();
		m_bgSprite = GetComponentInChildren<dfSlicedSprite>();
		m_button.PrecludeUpdateCycle = true;
		m_bgSprite.PrecludeUpdateCycle = true;
		questionMarkSprite.PrecludeUpdateCycle = true;
		m_button.MouseHover += m_button_MouseHover;
		m_button.Click += m_button_Click;
		m_button.LostFocus += m_button_LostFocus;
		m_button.GotFocus += m_button_GotFocus;
		m_button.ControlClippingChanged += m_button_ControlClippingChanged;
	}

	private void m_button_ControlClippingChanged(dfControl control, bool value)
	{
		if (encounterState == EncounterState.UNKNOWN)
		{
			return;
		}
		m_childSprite.renderer.enabled = !value;
		if (IsGunderfury)
		{
			string text = "gunderfury_LV" + (GunderfuryController.GetCurrentTier() + 1) + "0_idle_001";
			int spriteIdByName = m_childSprite.Collection.GetSpriteIdByName(text, -1);
			if (spriteIdByName != m_childSprite.spriteId)
			{
				m_childSprite.SetSprite(spriteIdByName);
				m_childSprite.PlaceAtLocalPositionByAnchor(Vector3.zero, tk2dBaseSprite.Anchor.MiddleCenter);
			}
		}
		UpdateClipping(m_childSprite);
		for (int i = 0; i < extantSynergyArrows.Count; i++)
		{
			UpdateClipping(extantSynergyArrows[i]);
		}
	}

	private void UpdateClipping(tk2dClippedSprite targetSprite)
	{
		if (GameManager.Instance.IsLoadingLevel || !m_button.IsVisible)
		{
			return;
		}
		Vector3[] corners = m_button.Parent.Parent.Parent.GetCorners();
		float x = corners[0].x;
		float y = corners[0].y;
		float x2 = corners[3].x;
		float y2 = corners[3].y;
		Bounds untrimmedBounds = targetSprite.GetUntrimmedBounds();
		untrimmedBounds.center += targetSprite.transform.position;
		float x3 = Mathf.Clamp01((x - untrimmedBounds.min.x) / untrimmedBounds.size.x);
		float y3 = Mathf.Clamp01((y2 - untrimmedBounds.min.y) / untrimmedBounds.size.y);
		float x4 = Mathf.Clamp01((x2 - untrimmedBounds.min.x) / untrimmedBounds.size.x);
		float y4 = Mathf.Clamp01((y - untrimmedBounds.min.y) / untrimmedBounds.size.y);
		targetSprite.clipBottomLeft = new Vector2(x3, y3);
		targetSprite.clipTopRight = new Vector2(x4, y4);
		if (SpriteOutlineManager.HasOutline(targetSprite))
		{
			tk2dClippedSprite[] outlineSprites = SpriteOutlineManager.GetOutlineSprites<tk2dClippedSprite>(targetSprite);
			for (int i = 0; i < outlineSprites.Length; i++)
			{
				untrimmedBounds = outlineSprites[i].GetUntrimmedBounds();
				untrimmedBounds.center += outlineSprites[i].transform.position;
				x3 = Mathf.Clamp01((x - untrimmedBounds.min.x) / untrimmedBounds.size.x);
				y3 = Mathf.Clamp01((y2 - untrimmedBounds.min.y) / untrimmedBounds.size.y);
				x4 = Mathf.Clamp01((x2 - untrimmedBounds.min.x) / untrimmedBounds.size.x);
				y4 = Mathf.Clamp01((y - untrimmedBounds.min.y) / untrimmedBounds.size.y);
				outlineSprites[i].clipBottomLeft = new Vector2(x3, y3);
				outlineSprites[i].clipTopRight = new Vector2(x4, y4);
			}
		}
	}

	private void LateUpdate()
	{
		UpdateClipping(m_childSprite);
		for (int i = 0; i < extantSynergyArrows.Count; i++)
		{
			UpdateClipping(extantSynergyArrows[i]);
		}
	}

	public void UpdateEncounterState()
	{
		if (GameStatsManager.Instance.QueryEncounterable(linkedEncounterTrackable) == 0)
		{
			if (linkedEncounterTrackable.PrerequisitesMet() && !linkedEncounterTrackable.journalData.SuppressKnownState && !linkedEncounterTrackable.journalData.IsEnemy)
			{
				SetEncounterState(EncounterState.KNOWN);
			}
			else
			{
				SetEncounterState(EncounterState.UNKNOWN);
			}
		}
		else if (linkedEncounterTrackable.PrerequisitesMet())
		{
			SetEncounterState(EncounterState.ENCOUNTERED);
		}
		else
		{
			SetEncounterState(EncounterState.UNKNOWN);
		}
	}

	public void ForceFocus()
	{
		m_button.Focus();
	}

	public void SetEncounterState(EncounterState st)
	{
		if (!IsEquipmentPage)
		{
			if (!ForceEncounterState)
			{
				encounterState = st;
			}
			switch (encounterState)
			{
			case EncounterState.ENCOUNTERED:
				m_childSprite.usesOverrideMaterial = true;
				m_childSprite.renderer.material.shader = ShaderCache.Acquire("Brave/AmmonomiconSpriteListShader");
				m_childSprite.renderer.material.DisableKeyword("BRIGHTNESS_CLAMP_ON");
				m_childSprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_OFF");
				m_childSprite.renderer.material.SetFloat("_SpriteScale", m_childSprite.scale.x);
				m_childSprite.renderer.material.SetFloat("_Saturation", 1f);
				m_childSprite.renderer.material.SetColor("_OverrideColor", new Color(0.4f, 0.4f, 0.4f, 0f));
				m_childSprite.renderer.enabled = true;
				questionMarkSprite.IsVisible = false;
				break;
			case EncounterState.KNOWN:
				m_childSprite.usesOverrideMaterial = true;
				m_childSprite.renderer.material.shader = ShaderCache.Acquire("Brave/AmmonomiconSpriteListShader");
				m_childSprite.renderer.material.DisableKeyword("BRIGHTNESS_CLAMP_ON");
				m_childSprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_OFF");
				m_childSprite.renderer.material.SetFloat("_SpriteScale", m_childSprite.scale.x);
				m_childSprite.renderer.material.SetFloat("_Saturation", 0f);
				m_childSprite.renderer.material.SetColor("_OverrideColor", new Color(0.4f, 0.4f, 0.4f, 0f));
				m_childSprite.renderer.enabled = true;
				questionMarkSprite.IsVisible = false;
				break;
			case EncounterState.UNKNOWN:
				m_childSprite.renderer.enabled = false;
				questionMarkSprite.IsVisible = true;
				break;
			}
		}
	}

	private void m_button_GotFocus(dfControl control, dfFocusEventArgs args)
	{
		AkSoundEngine.PostEvent("Play_UI_menu_select_01", base.gameObject);
		if (SpriteOutlineManager.HasOutline(m_childSprite))
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(m_childSprite, true);
			SpriteOutlineManager.AddScaledOutlineToSprite<tk2dClippedSprite>(m_childSprite, Color.white, 0.1f, 0f);
		}
		m_bgSprite.SpriteName = "big_box_page_raised_selected_001";
		AmmonomiconController.Instance.BestInteractingLeftPageRenderer.LastFocusTarget = m_button;
		if (encounterState == EncounterState.ENCOUNTERED)
		{
			AmmonomiconController.Instance.BestInteractingRightPageRenderer.SetRightDataPageTexts(m_childSprite, linkedEncounterTrackable);
		}
		else if (encounterState == EncounterState.KNOWN)
		{
			AmmonomiconController.Instance.BestInteractingRightPageRenderer.SetRightDataPageUnknown();
			AmmonomiconController.Instance.BestInteractingRightPageRenderer.SetRightDataPageName(m_childSprite, linkedEncounterTrackable);
		}
		else
		{
			AmmonomiconController.Instance.BestInteractingRightPageRenderer.SetRightDataPageUnknown();
		}
		if (AmmonomiconController.Instance.BestInteractingLeftPageRenderer.pageType == AmmonomiconPageRenderer.PageType.EQUIPMENT_LEFT)
		{
			UpdateSynergyHighlights();
		}
	}

	private void UpdateSynergyHighlights()
	{
		if (GameManager.Instance.IsSelectingCharacter)
		{
			return;
		}
		List<AmmonomiconPokedexEntry> pokedexEntries = AmmonomiconController.Instance.BestInteractingLeftPageRenderer.GetPokedexEntries();
		List<AmmonomiconPokedexEntry> list = new List<AmmonomiconPokedexEntry>();
		if (activeSynergies == null)
		{
			return;
		}
		for (int i = 0; i < activeSynergies.Count; i++)
		{
			PlayerController bestActivePlayer = GameManager.Instance.BestActivePlayer;
			for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
			{
				if (GameManager.Instance.AllPlayers[j].PlayerIDX == GameManager.Instance.LastPausingPlayerID)
				{
					bestActivePlayer = GameManager.Instance.AllPlayers[j];
				}
			}
			AdvancedSynergyEntry advancedSynergyEntry = activeSynergies[i];
			if (!advancedSynergyEntry.ContainsPickup(pickupID) || pokedexEntries == null)
			{
				continue;
			}
			for (int k = 0; k < pokedexEntries.Count; k++)
			{
				if (pokedexEntries[k].pickupID >= 0 && pokedexEntries[k].pickupID != pickupID && advancedSynergyEntry.ContainsPickup(pokedexEntries[k].pickupID))
				{
					tk2dClippedSprite tk2dClippedSprite2 = AmmonomiconController.Instance.CurrentLeftPageRenderer.AddSpriteToPage<tk2dClippedSprite>(AmmonomiconController.Instance.EncounterIconCollection, AmmonomiconController.Instance.EncounterIconCollection.GetSpriteIdByName("synergy_ammonomicon_arrow_001"));
					tk2dClippedSprite2.SetSprite("synergy_ammonomicon_arrow_001");
					Bounds bounds = pokedexEntries[k].m_childSprite.GetBounds();
					Bounds untrimmedBounds = pokedexEntries[k].m_childSprite.GetUntrimmedBounds();
					Vector3 size = bounds.size;
					tk2dClippedSprite2.transform.position = (pokedexEntries[k].m_childSprite.WorldCenter.ToVector3ZisY() + new Vector3(-8f * pokedexEntries[k].m_bgSprite.PixelsToUnits(), size.y / 2f + 32f * pokedexEntries[k].m_bgSprite.PixelsToUnits(), 0f)).WithZ(-0.65f);
					tk2dClippedSprite2.transform.parent = m_childSprite.transform.parent;
					extantSynergyArrows.Add(tk2dClippedSprite2);
					pokedexEntries[k].ChangeOutlineColor(SynergyDatabase.SynergyBlue);
					list.Add(pokedexEntries[k]);
				}
			}
		}
		if (pokedexEntries == null)
		{
			return;
		}
		for (int l = 0; l < pokedexEntries.Count; l++)
		{
			if (pokedexEntries[l] != this && !list.Contains(pokedexEntries[l]) && SpriteOutlineManager.HasOutline(pokedexEntries[l].m_childSprite))
			{
				SpriteOutlineManager.RemoveOutlineFromSprite(pokedexEntries[l].m_childSprite, true);
				SpriteOutlineManager.AddScaledOutlineToSprite<tk2dClippedSprite>(pokedexEntries[l].m_childSprite, Color.black, 0.1f, 0.05f);
			}
		}
	}

	private void m_button_LostFocus(dfControl control, dfFocusEventArgs args)
	{
		for (int i = 0; i < extantSynergyArrows.Count; i++)
		{
			UnityEngine.Object.Destroy(extantSynergyArrows[i].gameObject);
		}
		extantSynergyArrows.Clear();
		if (SpriteOutlineManager.HasOutline(m_childSprite))
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(m_childSprite, true);
			SpriteOutlineManager.AddScaledOutlineToSprite<tk2dClippedSprite>(m_childSprite, Color.black, 0.1f, 0.05f);
		}
		m_bgSprite.SpriteName = "big_box_page_flat_001";
	}

	private void m_button_Click(dfControl control, dfMouseEventArgs mouseEvent)
	{
		m_button.Focus();
	}

	private void m_button_MouseHover(dfControl control, dfMouseEventArgs mouseEvent)
	{
	}

	public void ChangeOutlineColor(Color targetColor)
	{
		SpriteOutlineManager.RemoveOutlineFromSprite(m_childSprite, true);
		SpriteOutlineManager.AddScaledOutlineToSprite<tk2dClippedSprite>(m_childSprite, targetColor, 0.1f, 0f);
	}

	public void AssignSprite(tk2dClippedSprite sprit)
	{
		m_childSprite = sprit;
		m_childSprite.ignoresTiltworldDepth = true;
		m_childSprite.transform.position += new Vector3(0f, 0f, -0.5f);
		if (encounterState != EncounterState.UNKNOWN)
		{
			m_childSprite.renderer.enabled = !m_button.IsControlClipped;
		}
	}
}
