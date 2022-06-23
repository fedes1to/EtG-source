using System.Collections;
using UnityEngine;

public class AppearedInTheGungeonController : MonoBehaviour
{
	public dfLabel[] itemNameLabels;

	public tk2dSprite itemSprite;

	private EncounterDatabaseEntry m_curTrackable;

	private bool m_isScalingDown;

	public void Appear(EncounterDatabaseEntry newPickup)
	{
		AkSoundEngine.PostEvent("Play_UI_card_open_01", base.gameObject);
		m_curTrackable = newPickup;
		dfPanel component = itemSprite.transform.parent.GetComponent<dfPanel>();
		tk2dSpriteCollectionData encounterIconCollection = AmmonomiconController.Instance.EncounterIconCollection;
		int spriteIdByName = encounterIconCollection.GetSpriteIdByName(newPickup.journalData.AmmonomiconSprite, 0);
		if (spriteIdByName < 0)
		{
			spriteIdByName = encounterIconCollection.GetSpriteIdByName(AmmonomiconController.AmmonomiconErrorSprite);
		}
		itemSprite.SetSprite(encounterIconCollection, spriteIdByName);
		itemSprite.transform.localScale = Vector3.one;
		Bounds untrimmedBounds = itemSprite.GetUntrimmedBounds();
		Vector2 vector = GameUIUtility.TK2DtoDF(untrimmedBounds.size.XY(), component.GUIManager.PixelsToUnits());
		itemSprite.scale = new Vector3(vector.x / untrimmedBounds.size.x, vector.y / untrimmedBounds.size.y, untrimmedBounds.size.z);
		itemSprite.ignoresTiltworldDepth = true;
		itemSprite.gameObject.SetLayerRecursively(LayerMask.NameToLayer("SecondaryGUI"));
		SpriteOutlineManager.AddScaledOutlineToSprite<tk2dSprite>(itemSprite, Color.black, 0.1f, 0.05f);
		itemSprite.PlaceAtLocalPositionByAnchor(Vector3.zero, tk2dBaseSprite.Anchor.MiddleCenter);
		for (int i = 0; i < itemNameLabels.Length; i++)
		{
			itemNameLabels[i].Text = newPickup.journalData.GetPrimaryDisplayName().ToUpperInvariant();
			float num = ((GameManager.Options.CurrentLanguage != StringTableManager.GungeonSupportedLanguages.JAPANESE) ? 1f : 3f);
			float num2 = 10f;
			if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.KOREAN || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.JAPANESE)
			{
				num2 = 6f;
			}
			if ((float)itemNameLabels[i].Text.Length > num2)
			{
				float t = ((float)itemNameLabels[i].Text.Length - num2) / num2;
				itemNameLabels[i].TextScale = Mathf.Lerp(2f, 1f, t) * num;
				itemNameLabels[i].RelativePosition = itemNameLabels[i].RelativePosition.WithY(Mathf.Lerp(51f, 72f, t).Quantize(3f));
			}
			else
			{
				itemNameLabels[i].TextScale = 2f * num;
				itemNameLabels[i].RelativePosition = itemNameLabels[i].RelativePosition.WithY(51f);
			}
		}
		itemNameLabels[0].PerformLayout();
		ShwoopOpen();
	}

	private void Update()
	{
		if (!AmmonomiconController.Instance.IsOpen && m_curTrackable != null && !m_isScalingDown)
		{
			ShwoopClosed();
			GameManager.Instance.AcknowledgeKnownTrackable(m_curTrackable);
			m_curTrackable = null;
		}
	}

	public void ShwoopOpen()
	{
		StartCoroutine(HandleShwoop(false));
	}

	private IEnumerator HandleShwoop(bool reverse)
	{
		if (!m_isScalingDown)
		{
			if (reverse)
			{
				m_isScalingDown = true;
			}
			float timer = 0.15f;
			float elapsed = 0f;
			Vector3 smallScale = new Vector3(0.01f, 0.01f, 1f);
			Vector3 bigScale = Vector3.one;
			AnimationCurve targetCurve = ((!reverse) ? GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().ShwoopInCurve : GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().ShwoopOutCurve);
			dfPanel m_panel = GetComponent<dfPanel>();
			while (elapsed < timer)
			{
				elapsed += GameManager.INVARIANT_DELTA_TIME;
				float t = elapsed / timer;
				m_panel.transform.localScale = smallScale + bigScale * Mathf.Clamp01(targetCurve.Evaluate(t));
				m_panel.MakePixelPerfect();
				yield return null;
			}
			if (!reverse)
			{
				m_panel.transform.localScale = Vector3.one;
				m_panel.MakePixelPerfect();
			}
			if (reverse)
			{
				m_panel.IsVisible = false;
				m_panel.IsInteractive = false;
				m_panel.IsEnabled = false;
				m_isScalingDown = false;
				Object.Destroy(m_panel.GUIManager.gameObject);
			}
		}
	}

	public void ShwoopClosed()
	{
		StartCoroutine(HandleShwoop(true));
	}
}
