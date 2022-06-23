using HutongGames.PlayMaker.Actions;
using InControl;
using UnityEngine;

public class FoyerCharacterSelectFlag : BraveBehaviour
{
	public GameObject OverheadElement;

	public string CharacterPrefabPath;

	public bool IsCoopCharacter;

	public bool IsEevee;

	public bool IsGunslinger;

	public DungeonPrerequisite[] prerequisites;

	public tk2dSpriteAnimation AltCostumeLibrary;

	public string AltCostumeClipName;

	private dfControl m_extantOverheadUIElement;

	private bool m_active = true;

	private bool m_isAlternateCostume;

	public bool IsAlternateCostume
	{
		get
		{
			return m_isAlternateCostume;
		}
	}

	public bool PrerequisitesFulfilled()
	{
		bool result = true;
		for (int i = 0; i < prerequisites.Length; i++)
		{
			if (!prerequisites[i].CheckConditionsFulfilled())
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public bool CanBeSelected()
	{
		if (IsEevee && GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.META_CURRENCY) < 5f)
		{
			return false;
		}
		if (IsGunslinger && GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.META_CURRENCY) < 7f)
		{
			return false;
		}
		return true;
	}

	private void EnsureStartingEquipmentEncountered()
	{
		if (!PrerequisitesFulfilled() || string.IsNullOrEmpty(CharacterPrefabPath))
		{
			return;
		}
		GameObject gameObject = (GameObject)BraveResources.Load(CharacterPrefabPath);
		if (!gameObject)
		{
			return;
		}
		PlayerController component = gameObject.GetComponent<PlayerController>();
		if (!component)
		{
			return;
		}
		if (component.startingGunIds != null)
		{
			for (int i = 0; i < component.startingGunIds.Count; i++)
			{
				Gun gun = PickupObjectDatabase.GetById(component.startingGunIds[i]) as Gun;
				if ((bool)gun && (bool)gun.encounterTrackable)
				{
					GameStatsManager.Instance.HandleEncounteredObjectRaw(gun.encounterTrackable.EncounterGuid);
				}
			}
		}
		if (component.startingActiveItemIds != null)
		{
			for (int j = 0; j < component.startingActiveItemIds.Count; j++)
			{
				PlayerItem playerItem = PickupObjectDatabase.GetById(component.startingActiveItemIds[j]) as PlayerItem;
				if ((bool)playerItem && (bool)playerItem.encounterTrackable)
				{
					GameStatsManager.Instance.HandleEncounteredObjectRaw(playerItem.encounterTrackable.EncounterGuid);
				}
			}
		}
		if (component.startingPassiveItemIds == null)
		{
			return;
		}
		for (int k = 0; k < component.startingPassiveItemIds.Count; k++)
		{
			PlayerItem playerItem2 = PickupObjectDatabase.GetById(component.startingPassiveItemIds[k]) as PlayerItem;
			if ((bool)playerItem2 && (bool)playerItem2.encounterTrackable)
			{
				GameStatsManager.Instance.HandleEncounteredObjectRaw(playerItem2.encounterTrackable.EncounterGuid);
			}
		}
	}

	public void Start()
	{
		EnsureStartingEquipmentEncountered();
	}

	private void ToggleSelf(bool activate)
	{
		m_active = activate;
		base.specRigidbody.enabled = activate;
		base.renderer.enabled = activate;
		base.talkDoer.IsInteractable = activate;
		base.talkDoer.ShowOutlines = activate;
		SetNpcVisibility.SetVisible(base.talkDoer, activate);
		SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, activate);
	}

	private void Update()
	{
		if (IsCoopCharacter)
		{
			if (m_active && InputManager.Devices.Count == 0)
			{
				ToggleSelf(false);
			}
			else if (!m_active && InputManager.Devices.Count > 0)
			{
				ToggleSelf(true);
			}
		}
	}

	public void ToggleOverheadElementVisibility(bool value)
	{
		if (!m_extantOverheadUIElement || m_extantOverheadUIElement.IsVisible == value)
		{
			return;
		}
		m_extantOverheadUIElement.IsVisible = value;
		FoyerInfoPanelController component = m_extantOverheadUIElement.GetComponent<FoyerInfoPanelController>();
		if ((bool)component.arrow && component.arrow.transform.childCount > 0)
		{
			MeshRenderer component2 = component.arrow.transform.GetChild(0).GetComponent<MeshRenderer>();
			if ((bool)component2)
			{
				component2.enabled = value;
			}
		}
	}

	public void ChangeToAlternateCostume()
	{
		if (AltCostumeLibrary != null && !m_isAlternateCostume)
		{
			CharacterSelectIdleDoer component = GetComponent<CharacterSelectIdleDoer>();
			if ((bool)component)
			{
				component.enabled = false;
			}
			m_isAlternateCostume = true;
			tk2dSpriteAnimation library = base.spriteAnimator.Library;
			base.spriteAnimator.Library = AltCostumeLibrary;
			base.spriteAnimator.Play(AltCostumeClipName);
			AltCostumeLibrary = library;
		}
		else if (AltCostumeLibrary != null)
		{
			m_isAlternateCostume = false;
			tk2dSpriteAnimation library2 = base.spriteAnimator.Library;
			base.spriteAnimator.Library = AltCostumeLibrary;
			base.spriteAnimator.Play("select_idle");
			AltCostumeLibrary = library2;
		}
	}

	public FoyerInfoPanelController CreateOverheadElement()
	{
		m_extantOverheadUIElement = GameUIRoot.Instance.Manager.AddPrefab(OverheadElement);
		FoyerInfoPanelController component = m_extantOverheadUIElement.GetComponent<FoyerInfoPanelController>();
		if ((bool)component)
		{
			component.followTransform = base.transform;
			component.offset = new Vector3(0.75f, 1.625f, 0f);
		}
		return component;
	}

	private void OnDisable()
	{
		ClearOverheadElement();
	}

	public void OnCoopChangedCallback()
	{
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			base.gameObject.SetActive(false);
			GetComponent<SpeculativeRigidbody>().enabled = false;
			return;
		}
		base.gameObject.SetActive(true);
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
		base.specRigidbody.enabled = true;
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(base.specRigidbody);
		CharacterSelectIdleDoer component = GetComponent<CharacterSelectIdleDoer>();
		component.enabled = true;
	}

	public void OnSelectedCharacterCallback(PlayerController newCharacter)
	{
		Debug.Log(string.Concat(newCharacter.name, "|", newCharacter.characterIdentity, " <===="));
		if (newCharacter.gameObject.name.Contains(CharacterPrefabPath))
		{
			base.gameObject.SetActive(false);
			GetComponent<SpeculativeRigidbody>().enabled = false;
			if (IsEevee)
			{
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.META_CURRENCY, -5f);
			}
			if (IsGunslinger)
			{
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.META_CURRENCY, -7f);
			}
		}
		else if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(true);
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
			base.specRigidbody.enabled = true;
			PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(base.specRigidbody);
			if (!m_isAlternateCostume)
			{
				CharacterSelectIdleDoer component = GetComponent<CharacterSelectIdleDoer>();
				component.enabled = true;
			}
		}
	}

	public void ClearOverheadElement()
	{
		if (m_extantOverheadUIElement != null)
		{
			Object.Destroy(m_extantOverheadUIElement.gameObject);
			m_extantOverheadUIElement = null;
		}
	}
}
