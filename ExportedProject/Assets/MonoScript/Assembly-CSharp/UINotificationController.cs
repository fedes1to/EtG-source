using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class UINotificationController : MonoBehaviour
{
	public enum NotificationColor
	{
		SILVER,
		GOLD,
		PURPLE
	}

	public tk2dBaseSprite notificationObjectSprite;

	public tk2dBaseSprite notificationSynergySprite;

	public dfSprite ObjectBoxSprite;

	public dfSprite CrosshairSprite;

	public dfSprite StickerSprite;

	public dfSprite BoxSprite;

	public dfLabel NameLabel;

	public dfLabel DescriptionLabel;

	public dfLabel CenterLabel;

	public dfAnimationClip SilverAnimClip;

	public dfAnimationClip GoldAnimClip;

	public dfAnimationClip PurpleAnimClip;

	[Header("Synergues")]
	public dfAnimationClip SynergyTransformClip;

	public dfAnimationClip SynergyBoxTransformClip;

	public dfAnimationClip SynergyCrosshairTransformClip;

	private tk2dSprite[] outlineSprites;

	private tk2dSprite[] synergyOutlineSprites;

	private dfPanel m_panel;

	private List<IEnumerator> m_queuedNotifications = new List<IEnumerator>();

	private List<NotificationParams> m_queuedNotificationParams = new List<NotificationParams>();

	private IEnumerator m_currentNotificationProcess;

	private bool m_doingNotification;

	private dfFontBase EnglishFont;

	private dfFontBase OtherLanguageFont;

	private Vector3 NameBasePos;

	private Vector3 DescBasePos;

	private StringTableManager.GungeonSupportedLanguages m_cachedLanguage;

	private bool m_isCurrentlyExpanded;

	private bool m_textsLowered;

	public dfPanel Panel
	{
		get
		{
			return m_panel;
		}
	}

	public bool IsDoingNotification
	{
		get
		{
			return m_doingNotification;
		}
	}

	private void Start()
	{
		if (EnglishFont == null)
		{
			EnglishFont = DescriptionLabel.DefaultAssignedFont;
			OtherLanguageFont = DescriptionLabel.GUIManager.DefaultFont;
			NameBasePos = NameLabel.RelativePosition;
			DescBasePos = DescriptionLabel.RelativePosition;
		}
	}

	public void Initialize()
	{
		m_panel = GetComponent<dfPanel>();
		GameUIRoot.Instance.AddControlToMotionGroups(m_panel, DungeonData.Direction.SOUTH, true);
		notificationObjectSprite.HeightOffGround = GameUIRoot.Instance.transform.position.y - 2f;
		notificationSynergySprite.HeightOffGround = GameUIRoot.Instance.transform.position.y - 1f;
		SpriteOutlineManager.AddOutlineToSprite(notificationObjectSprite, Color.white, -1f);
		outlineSprites = SpriteOutlineManager.GetOutlineSprites(notificationObjectSprite);
		for (int i = 0; i < outlineSprites.Length; i++)
		{
			tk2dSprite tk2dSprite2 = outlineSprites[i];
			if ((bool)tk2dSprite2)
			{
				tk2dSprite2.gameObject.layer = notificationObjectSprite.gameObject.layer;
				tk2dSprite2.renderer.enabled = notificationObjectSprite.renderer.enabled;
				tk2dSprite2.HeightOffGround = -0.25f;
			}
		}
		SpriteOutlineManager.AddOutlineToSprite(notificationSynergySprite, Color.white, -1f);
		synergyOutlineSprites = SpriteOutlineManager.GetOutlineSprites(notificationSynergySprite);
		for (int j = 0; j < synergyOutlineSprites.Length; j++)
		{
			tk2dSprite tk2dSprite3 = synergyOutlineSprites[j];
			if ((bool)tk2dSprite3)
			{
				tk2dSprite3.gameObject.layer = notificationSynergySprite.gameObject.layer;
				tk2dSprite3.renderer.enabled = notificationSynergySprite.renderer.enabled;
				tk2dSprite3.HeightOffGround = -0.25f;
			}
		}
		notificationObjectSprite.UpdateZDepth();
		notificationSynergySprite.UpdateZDepth();
		CheckLanguageFonts();
		StartCoroutine(BG_CoroutineProcessor());
	}

	public void ForceHide()
	{
		if (IsDoingNotification)
		{
			GameUIRoot.Instance.MoveNonCoreGroupOnscreen(m_panel, true);
			m_currentNotificationProcess = null;
			m_queuedNotifications.Clear();
		}
	}

	private IEnumerator BG_CoroutineProcessor()
	{
		while (true)
		{
			if (m_queuedNotifications.Count != m_queuedNotificationParams.Count)
			{
				m_queuedNotificationParams.Clear();
				m_queuedNotifications.Clear();
			}
			if (m_currentNotificationProcess != null)
			{
				m_doingNotification = true;
				if (!m_currentNotificationProcess.MoveNext())
				{
					m_currentNotificationProcess = null;
				}
			}
			if (m_currentNotificationProcess == null)
			{
				if (m_queuedNotificationParams.Count > 0)
				{
					while (m_queuedNotificationParams.Count > 0 && m_queuedNotificationParams[0].OnlyIfSynergy && !m_queuedNotificationParams[0].HasAttachedSynergy)
					{
						m_queuedNotificationParams.RemoveAt(0);
						m_queuedNotifications.RemoveAt(0);
					}
				}
				if (m_queuedNotifications.Count > 0)
				{
					m_currentNotificationProcess = m_queuedNotifications[0];
					m_queuedNotifications.RemoveAt(0);
					if (m_queuedNotificationParams.Count > 0)
					{
						m_queuedNotificationParams.RemoveAt(0);
					}
				}
				else
				{
					if (m_panel.IsVisible)
					{
						DisableRenderers();
					}
					m_doingNotification = false;
				}
			}
			yield return null;
		}
	}

	private float ActualSign(float f)
	{
		if (Mathf.Abs(f) < 0.0001f)
		{
			return 0f;
		}
		if (f < 0f)
		{
			return -1f;
		}
		if (f > 0f)
		{
			return 1f;
		}
		return 0f;
	}

	private void CheckLanguageFonts()
	{
		if (EnglishFont == null)
		{
			EnglishFont = DescriptionLabel.Font;
			OtherLanguageFont = DescriptionLabel.GUIManager.DefaultFont;
			NameBasePos = NameLabel.RelativePosition;
			DescBasePos = DescriptionLabel.RelativePosition;
		}
		if (StringTableManager.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH)
		{
			if (m_cachedLanguage != 0)
			{
				NameLabel.RelativePosition = NameBasePos;
				NameLabel.Font = EnglishFont;
				NameLabel.TextScale = 0.6f;
				DescriptionLabel.RelativePosition = DescBasePos;
				DescriptionLabel.Font = EnglishFont;
				DescriptionLabel.TextScale = 0.6f;
			}
		}
		else if (StringTableManager.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.JAPANESE)
		{
			if (m_cachedLanguage != StringTableManager.GungeonSupportedLanguages.JAPANESE)
			{
				NameLabel.RelativePosition = NameBasePos + new Vector3(0f, -9f, 0f);
				NameLabel.TextScale = 3f;
				DescriptionLabel.RelativePosition = DescBasePos + new Vector3(0f, -6f, 0f);
				DescriptionLabel.TextScale = 3f;
			}
		}
		else if (StringTableManager.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE)
		{
			if (StringTableManager.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE)
			{
				NameLabel.RelativePosition = NameBasePos + new Vector3(0f, -24f, 0f);
				NameLabel.TextScale = 3f;
				DescriptionLabel.RelativePosition = DescBasePos + new Vector3(0f, -12f, 0f);
				DescriptionLabel.TextScale = 3f;
			}
		}
		else if (StringTableManager.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.KOREAN)
		{
			if (m_cachedLanguage != StringTableManager.CurrentLanguage)
			{
				NameLabel.RelativePosition = NameBasePos + new Vector3(0f, -9f, 0f);
				DescriptionLabel.RelativePosition = DescBasePos + new Vector3(0f, -6f, 0f);
			}
		}
		else if (StringTableManager.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.RUSSIAN)
		{
			if (m_cachedLanguage != StringTableManager.CurrentLanguage)
			{
				NameLabel.RelativePosition = NameBasePos + new Vector3(0f, -12f, 0f);
				NameLabel.TextScale = 3f;
				DescriptionLabel.RelativePosition = DescBasePos + new Vector3(0f, -9f, 0f);
				DescriptionLabel.TextScale = 3f;
			}
		}
		else if (m_cachedLanguage != StringTableManager.CurrentLanguage)
		{
			NameLabel.RelativePosition = NameBasePos + new Vector3(0f, -12f, 0f);
			NameLabel.Font = OtherLanguageFont;
			NameLabel.TextScale = 3f;
			DescriptionLabel.RelativePosition = DescBasePos + new Vector3(0f, -9f, 0f);
			DescriptionLabel.Font = OtherLanguageFont;
			DescriptionLabel.TextScale = 3f;
		}
		m_cachedLanguage = StringTableManager.CurrentLanguage;
	}

	private void SetWidths()
	{
		bool flag = GameManager.Options.CurrentLanguage != 0 && GameManager.Options.CurrentLanguage != StringTableManager.GungeonSupportedLanguages.JAPANESE && GameManager.Options.CurrentLanguage != StringTableManager.GungeonSupportedLanguages.KOREAN && GameManager.Options.CurrentLanguage != StringTableManager.GungeonSupportedLanguages.CHINESE;
		if (flag && !m_isCurrentlyExpanded)
		{
			m_isCurrentlyExpanded = true;
			m_panel.Width += 126f;
			NameLabel.Width += 126f;
			DescriptionLabel.Width += 126f;
			BoxSprite.Width += 42f;
			m_panel.PerformLayout();
			GameUIRoot.Instance.MoveNonCoreGroupImmediately(m_panel);
			GameUIRoot.Instance.UpdateControlMotionGroup(m_panel);
			GameUIRoot.Instance.MoveNonCoreGroupImmediately(m_panel, true);
		}
		else if (!flag && m_isCurrentlyExpanded)
		{
			m_isCurrentlyExpanded = false;
			NameLabel.Width -= 126f;
			DescriptionLabel.Width -= 126f;
			BoxSprite.Width -= 42f;
			m_panel.Width -= 126f;
			m_panel.PerformLayout();
			GameUIRoot.Instance.MoveNonCoreGroupImmediately(m_panel);
			GameUIRoot.Instance.UpdateControlMotionGroup(m_panel);
			GameUIRoot.Instance.MoveNonCoreGroupImmediately(m_panel, true);
		}
	}

	public void DoNotification(EncounterTrackable trackable, bool onlyIfSynergy = false)
	{
		if ((bool)trackable)
		{
			CheckLanguageFonts();
			SetWidths();
			tk2dBaseSprite component = trackable.GetComponent<tk2dBaseSprite>();
			NotificationParams notificationParams = new NotificationParams();
			notificationParams.EncounterGuid = trackable.EncounterGuid;
			PickupObject component2 = trackable.GetComponent<PickupObject>();
			if ((bool)component2)
			{
				notificationParams.pickupId = component2.PickupObjectId;
			}
			notificationParams.SpriteCollection = component.Collection;
			notificationParams.SpriteID = component.spriteId;
			notificationParams = SetupTexts(trackable, notificationParams);
			notificationParams.OnlyIfSynergy = onlyIfSynergy;
			if (StringTableManager.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.JAPANESE)
			{
				NameLabel.RelativePosition = NameBasePos + new Vector3(0f, -9f, 0f);
				NameLabel.TextScale = 3f;
				DescriptionLabel.RelativePosition = DescBasePos + new Vector3(0f, -6f, 0f);
				DescriptionLabel.TextScale = 3f;
			}
			DoNotificationInternal(notificationParams);
		}
	}

	private void DoNotificationInternal(NotificationParams notifyParams)
	{
		m_queuedNotifications.Add(HandleNotification(notifyParams));
		m_queuedNotificationParams.Add(notifyParams);
		StartCoroutine(PruneQueuedNotifications());
	}

	private IEnumerator PruneQueuedNotifications()
	{
		yield return null;
		if (m_queuedNotifications.Count <= 1 || (m_queuedNotifications.Count == 2 && !IsDoingNotification))
		{
			yield break;
		}
		int startIndex = ((!IsDoingNotification) ? 1 : 0);
		if (startIndex >= m_queuedNotifications.Count - 1)
		{
			yield break;
		}
		for (int i = startIndex; i < m_queuedNotifications.Count - 1; i++)
		{
			NotificationParams notificationParams = m_queuedNotificationParams[i];
			if (!notificationParams.HasAttachedSynergy)
			{
				m_queuedNotificationParams.RemoveAt(i);
				m_queuedNotifications.RemoveAt(i);
				i--;
			}
		}
	}

	public void DoCustomNotification(string header, string description, tk2dSpriteCollectionData collection, int spriteId, NotificationColor notifyColor = NotificationColor.SILVER, bool allowQueueing = false, bool forceSingleLine = false)
	{
		CheckLanguageFonts();
		NotificationParams notificationParams = new NotificationParams();
		notificationParams.SpriteCollection = collection;
		notificationParams.SpriteID = spriteId;
		notificationParams.PrimaryTitleString = header.ToUpperInvariant();
		notificationParams.SecondaryDescriptionString = description;
		notificationParams.isSingleLine = forceSingleLine;
		notificationParams.forcedColor = notifyColor;
		DoNotificationInternal(notificationParams);
	}

	public void AttemptSynergyAttachment(AdvancedSynergyEntry e)
	{
		for (int num = m_queuedNotificationParams.Count - 1; num >= 0; num--)
		{
			NotificationParams notificationParams = m_queuedNotificationParams[num];
			if (!string.IsNullOrEmpty(notificationParams.EncounterGuid))
			{
				EncounterDatabaseEntry entry = EncounterDatabase.GetEntry(notificationParams.EncounterGuid);
				int num2 = ((entry == null) ? (-1) : entry.pickupObjectId);
				if (num2 >= 0 && e.ContainsPickup(num2))
				{
					notificationParams.HasAttachedSynergy = true;
					notificationParams.AttachedSynergy = e;
					m_queuedNotificationParams[num] = notificationParams;
					break;
				}
			}
		}
	}

	private NotificationParams SetupTexts(EncounterTrackable trackable, NotificationParams notifyParams)
	{
		string text = trackable.name;
		string secondaryDescriptionString = "???";
		if (!string.IsNullOrEmpty(trackable.journalData.GetPrimaryDisplayName()))
		{
			text = trackable.journalData.GetPrimaryDisplayName();
			if (text.Contains("®"))
			{
				text = text.Replace('®', '@');
			}
		}
		else
		{
			PickupObject component = trackable.GetComponent<PickupObject>();
			if (component != null)
			{
				text = component.DisplayName;
			}
		}
		if (trackable.GetComponent<SpiceItem>() != null)
		{
			int num = GameManager.Instance.PrimaryPlayer.spiceCount;
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				num += GameManager.Instance.SecondaryPlayer.spiceCount;
			}
			secondaryDescriptionString = trackable.journalData.GetCustomNotificationPanelDescription(Mathf.Min(num, 3));
		}
		else if (!string.IsNullOrEmpty(trackable.journalData.GetNotificationPanelDescription()))
		{
			secondaryDescriptionString = trackable.journalData.GetNotificationPanelDescription();
		}
		notifyParams.PrimaryTitleString = text.ToUpperInvariant();
		notifyParams.SecondaryDescriptionString = secondaryDescriptionString;
		return notifyParams;
	}

	private void ResetSynergySprite()
	{
		notificationSynergySprite.renderer.enabled = false;
		notificationSynergySprite.scale = GameUIUtility.GetCurrentTK2D_DFScale(m_panel.GetManager()) * Vector3.one;
		for (int i = 0; i < synergyOutlineSprites.Length; i++)
		{
			synergyOutlineSprites[i].renderer.enabled = false;
			synergyOutlineSprites[i].transform.localPosition = (new Vector3(ActualSign(synergyOutlineSprites[i].transform.localPosition.x) * 0.0625f, ActualSign(synergyOutlineSprites[i].transform.localPosition.y) * 0.0625f, 0f) * Pixelator.Instance.CurrentTileScale * 16f * GameUIRoot.Instance.PixelsToUnits()).WithZ(1f);
			synergyOutlineSprites[i].scale = notificationObjectSprite.scale;
		}
		Vector3 center = ObjectBoxSprite.GetCenter();
		notificationSynergySprite.PlaceAtPositionByAnchor(center, tk2dBaseSprite.Anchor.MiddleCenter);
	}

	private void SetupSynergySprite(tk2dSpriteCollectionData collection, int spriteId)
	{
		notificationSynergySprite.SetSprite(collection, spriteId);
		Vector3 center = ObjectBoxSprite.GetCenter();
		notificationSynergySprite.PlaceAtPositionByAnchor(center, tk2dBaseSprite.Anchor.MiddleCenter);
		notificationSynergySprite.transform.localPosition = notificationSynergySprite.transform.localPosition.Quantize(BoxSprite.PixelsToUnits() * 3f);
	}

	private void SetupSprite(tk2dSpriteCollectionData collection, int spriteId)
	{
		ResetSynergySprite();
		if (collection == null || spriteId < 0)
		{
			notificationObjectSprite.renderer.enabled = false;
			for (int i = 0; i < outlineSprites.Length; i++)
			{
				outlineSprites[i].renderer.enabled = false;
			}
			return;
		}
		notificationObjectSprite.renderer.enabled = false;
		notificationObjectSprite.SetSprite(collection, spriteId);
		notificationObjectSprite.scale = GameUIUtility.GetCurrentTK2D_DFScale(m_panel.GetManager()) * Vector3.one;
		for (int j = 0; j < outlineSprites.Length; j++)
		{
			outlineSprites[j].renderer.enabled = false;
			outlineSprites[j].transform.localPosition = (new Vector3(ActualSign(outlineSprites[j].transform.localPosition.x) * 0.0625f, ActualSign(outlineSprites[j].transform.localPosition.y) * 0.0625f, 0f) * Pixelator.Instance.CurrentTileScale * 16f * GameUIRoot.Instance.PixelsToUnits()).WithZ(1f);
			outlineSprites[j].scale = notificationObjectSprite.scale;
		}
		Vector3 center = ObjectBoxSprite.GetCenter();
		notificationObjectSprite.PlaceAtPositionByAnchor(center, tk2dBaseSprite.Anchor.MiddleCenter);
		notificationObjectSprite.transform.localPosition = notificationObjectSprite.transform.localPosition.Quantize(BoxSprite.PixelsToUnits() * 3f);
	}

	private void ToggleSynergyStatus(bool synergy)
	{
		if (synergy)
		{
			CrosshairSprite.SpriteName = "crosshair_synergy";
			CrosshairSprite.Size = CrosshairSprite.SpriteInfo.sizeInPixels * 3f;
			BoxSprite.SpriteName = "notification_box_synergy";
			ObjectBoxSprite.IsVisible = false;
			StickerSprite.IsVisible = false;
		}
	}

	private void ToggleGoldStatus(bool gold)
	{
		CrosshairSprite.SpriteName = ((!gold) ? "crosshair" : "crosshair_gold");
		CrosshairSprite.Size = CrosshairSprite.SpriteInfo.sizeInPixels * 3f;
		BoxSprite.SpriteName = ((!gold) ? "notification_box" : "notification_box_gold_001");
		ObjectBoxSprite.IsVisible = true;
		ObjectBoxSprite.SpriteName = ((!gold) ? "object_box" : "object_box_gold_001");
		StickerSprite.IsVisible = gold;
	}

	private void TogglePurpleStatus(bool purple)
	{
		if (purple)
		{
			CrosshairSprite.SpriteName = "crosshair_gold";
			CrosshairSprite.Size = CrosshairSprite.SpriteInfo.sizeInPixels * 3f;
			BoxSprite.SpriteName = "notification_box_purp_001";
			ObjectBoxSprite.IsVisible = true;
			ObjectBoxSprite.SpriteName = "object_box_purp_001";
			StickerSprite.IsVisible = false;
		}
	}

	private void DisableRenderers()
	{
		notificationObjectSprite.renderer.enabled = false;
		SpriteOutlineManager.ToggleOutlineRenderers(notificationObjectSprite, false);
		m_panel.IsVisible = false;
	}

	public void ForceToFront()
	{
		if ((bool)m_panel)
		{
			m_panel.Parent.BringToFront();
		}
	}

	private int GetIDOfOwnedSynergizingItem(int sourceID, AdvancedSynergyEntry syn)
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			for (int j = 0; j < playerController.inventory.AllGuns.Count; j++)
			{
				int pickupObjectId = playerController.inventory.AllGuns[j].PickupObjectId;
				if (pickupObjectId != sourceID && syn.ContainsPickup(pickupObjectId))
				{
					return pickupObjectId;
				}
			}
			for (int k = 0; k < playerController.activeItems.Count; k++)
			{
				int pickupObjectId2 = playerController.activeItems[k].PickupObjectId;
				if (pickupObjectId2 != sourceID && syn.ContainsPickup(pickupObjectId2))
				{
					return pickupObjectId2;
				}
			}
			for (int l = 0; l < playerController.passiveItems.Count; l++)
			{
				int pickupObjectId3 = playerController.passiveItems[l].PickupObjectId;
				if (pickupObjectId3 != sourceID && syn.ContainsPickup(pickupObjectId3))
				{
					return pickupObjectId3;
				}
			}
		}
		return -1;
	}

	private IEnumerator HandleNotification(NotificationParams notifyParams)
	{
		yield return null;
		SetupSprite(notifyParams.SpriteCollection, notifyParams.SpriteID);
		DescriptionLabel.ProcessMarkup = true;
		DescriptionLabel.ColorizeSymbols = true;
		NameLabel.Text = notifyParams.PrimaryTitleString.ToUpperInvariant();
		DescriptionLabel.Text = notifyParams.SecondaryDescriptionString;
		CenterLabel.Opacity = 1f;
		NameLabel.Opacity = 1f;
		DescriptionLabel.Opacity = 1f;
		CenterLabel.IsVisible = false;
		NameLabel.IsVisible = true;
		DescriptionLabel.IsVisible = true;
		dfSpriteAnimation component = BoxSprite.GetComponent<dfSpriteAnimation>();
		component.Stop();
		dfSpriteAnimation component2 = CrosshairSprite.GetComponent<dfSpriteAnimation>();
		component2.Stop();
		dfSpriteAnimation component3 = ObjectBoxSprite.GetComponent<dfSpriteAnimation>();
		component3.Stop();
		NotificationColor forcedColor = notifyParams.forcedColor;
		string trackableGuid = notifyParams.EncounterGuid;
		bool isGold = forcedColor == NotificationColor.GOLD || (!string.IsNullOrEmpty(trackableGuid) && GameStatsManager.Instance.QueryEncounterable(trackableGuid) == 1);
		bool isPurple = forcedColor == NotificationColor.PURPLE || (!string.IsNullOrEmpty(trackableGuid) && EncounterDatabase.GetEntry(trackableGuid).usesPurpleNotifications);
		ToggleGoldStatus(isGold);
		TogglePurpleStatus(isPurple);
		bool singleLineSansSprite = notifyParams.isSingleLine;
		if (singleLineSansSprite || notifyParams.SpriteCollection == null)
		{
			ObjectBoxSprite.IsVisible = false;
			StickerSprite.IsVisible = false;
		}
		if (singleLineSansSprite)
		{
			CenterLabel.IsVisible = true;
			NameLabel.IsVisible = false;
			DescriptionLabel.IsVisible = false;
			CenterLabel.Text = NameLabel.Text;
		}
		else
		{
			NameLabel.IsVisible = true;
			DescriptionLabel.IsVisible = true;
			CenterLabel.IsVisible = false;
		}
		m_doingNotification = true;
		m_panel.IsVisible = false;
		GameUIRoot.Instance.MoveNonCoreGroupOnscreen(m_panel);
		float elapsed2 = 0f;
		float duration3 = 5f;
		bool hasPlayedAnim = false;
		if (singleLineSansSprite)
		{
			notificationObjectSprite.renderer.enabled = false;
			SpriteOutlineManager.ToggleOutlineRenderers(notificationObjectSprite, false);
		}
		while (elapsed2 < ((!notifyParams.HasAttachedSynergy) ? duration3 : (duration3 - 2f)))
		{
			elapsed2 += BraveTime.DeltaTime;
			if (!hasPlayedAnim && elapsed2 > 0.75f)
			{
				BoxSprite.GetComponent<dfSpriteAnimation>().Clip = (isPurple ? PurpleAnimClip : ((!isGold) ? SilverAnimClip : GoldAnimClip));
				hasPlayedAnim = true;
				ObjectBoxSprite.Parent.GetComponent<dfSpriteAnimation>().Play();
			}
			yield return null;
			m_panel.IsVisible = true;
			if (!singleLineSansSprite && notifyParams.SpriteCollection != null)
			{
				notificationObjectSprite.renderer.enabled = true;
				SpriteOutlineManager.ToggleOutlineRenderers(notificationObjectSprite, true);
			}
		}
		if (notifyParams.HasAttachedSynergy)
		{
			AdvancedSynergyEntry pairedSynergy = notifyParams.AttachedSynergy;
			EncounterDatabaseEntry encounterSource = EncounterDatabase.GetEntry(trackableGuid);
			int pickupObjectId = ((encounterSource == null) ? (-1) : encounterSource.pickupObjectId);
			PickupObject puo = PickupObjectDatabase.GetById(pickupObjectId);
			if ((bool)puo)
			{
				int pID = GetIDOfOwnedSynergizingItem(puo.PickupObjectId, pairedSynergy);
				PickupObject puo2 = PickupObjectDatabase.GetById(pID);
				if ((bool)puo2 && (bool)puo2.sprite)
				{
					SetupSynergySprite(puo2.sprite.Collection, puo2.sprite.spriteId);
					elapsed2 = 0f;
					duration3 = 4f;
					notificationSynergySprite.renderer.enabled = true;
					SpriteOutlineManager.ToggleOutlineRenderers(notificationSynergySprite, true);
					dfSpriteAnimation boxSpriteAnimator = BoxSprite.GetComponent<dfSpriteAnimation>();
					boxSpriteAnimator.Clip = SynergyTransformClip;
					boxSpriteAnimator.Play();
					dfSpriteAnimation crosshairSpriteAnimator = CrosshairSprite.GetComponent<dfSpriteAnimation>();
					crosshairSpriteAnimator.Clip = SynergyCrosshairTransformClip;
					crosshairSpriteAnimator.Play();
					dfSpriteAnimation objectSpriteAnimator = ObjectBoxSprite.GetComponent<dfSpriteAnimation>();
					objectSpriteAnimator.Clip = SynergyBoxTransformClip;
					objectSpriteAnimator.Play();
					string synergyName = (string.IsNullOrEmpty(pairedSynergy.NameKey) ? string.Empty : StringTableManager.GetSynergyString(pairedSynergy.NameKey));
					bool synergyHasName = !string.IsNullOrEmpty(synergyName);
					if (synergyHasName)
					{
						CenterLabel.IsVisible = true;
						CenterLabel.Text = synergyName;
					}
					while (elapsed2 < duration3)
					{
						float baseSpriteLocalX = notificationObjectSprite.transform.localPosition.x;
						float synSpriteLocalX = notificationSynergySprite.transform.localPosition.x;
						CrosshairSprite.Size = CrosshairSprite.SpriteInfo.sizeInPixels * 3f;
						float p2u = BoxSprite.PixelsToUnits();
						Vector3 endPosition = ObjectBoxSprite.GetCenter();
						Vector3 startPosition = endPosition + new Vector3(0f, -120f * p2u, 0f);
						Vector3 startPosition2 = endPosition;
						Vector3 endPosition2 = endPosition + new Vector3(0f, 12f * p2u, 0f);
						endPosition -= new Vector3(0f, 21f * p2u, 0f);
						float t = elapsed2 / duration3;
						float quickT = elapsed2 / 1f;
						float smoothT = Mathf.SmoothStep(0f, 1f, quickT);
						if (synergyHasName)
						{
							float num = Mathf.SmoothStep(0f, 1f, elapsed2 / 0.5f);
							float opacity = Mathf.SmoothStep(0f, 1f, (elapsed2 - 0.5f) / 0.5f);
							NameLabel.Opacity = 1f - num;
							DescriptionLabel.Opacity = 1f - num;
							CenterLabel.Opacity = opacity;
						}
						Vector3 t2 = Vector3.Lerp(startPosition, endPosition, smoothT).Quantize(p2u * 3f).WithX(startPosition.x);
						Vector3 t3 = Vector3.Lerp(startPosition2, endPosition2, smoothT).Quantize(p2u * 3f).WithX(startPosition2.x);
						t3.y = Mathf.Max(startPosition2.y, t3.y);
						notificationSynergySprite.PlaceAtPositionByAnchor(t2, tk2dBaseSprite.Anchor.MiddleCenter);
						notificationSynergySprite.transform.position = notificationSynergySprite.transform.position + new Vector3(0f, 0f, -0.125f);
						notificationObjectSprite.PlaceAtPositionByAnchor(t3, tk2dBaseSprite.Anchor.MiddleCenter);
						notificationObjectSprite.transform.localPosition = notificationObjectSprite.transform.localPosition.WithX(baseSpriteLocalX);
						notificationSynergySprite.transform.localPosition = notificationSynergySprite.transform.localPosition.WithX(synSpriteLocalX);
						notificationSynergySprite.UpdateZDepth();
						notificationObjectSprite.UpdateZDepth();
						elapsed2 += BraveTime.DeltaTime;
						yield return null;
					}
				}
			}
		}
		GameUIRoot.Instance.MoveNonCoreGroupOnscreen(m_panel, true);
		elapsed2 = 0f;
		duration3 = 0.25f;
		while (elapsed2 < duration3)
		{
			elapsed2 += BraveTime.DeltaTime;
			yield return null;
		}
		CenterLabel.Opacity = 1f;
		NameLabel.Opacity = 1f;
		DescriptionLabel.Opacity = 1f;
		CenterLabel.IsVisible = false;
		NameLabel.IsVisible = true;
		DescriptionLabel.IsVisible = true;
		DisableRenderers();
		m_doingNotification = false;
	}
}
