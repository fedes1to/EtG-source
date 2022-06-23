using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIBossHealthController : MonoBehaviour
{
	private const float c_minTimeBetweenHitVfx = 0.25f;

	public dfSlicedSprite tankSprite;

	public List<dfSlicedSprite> barSprites;

	public dfLabel bossNameLabel;

	public float fillTime = 1.5f;

	public GameObject damagedVFX;

	public BossCardUIController bossCardUIPrefab;

	public bool IsVertical;

	[NonSerialized]
	public float Opacity = 1f;

	[NonSerialized]
	public float OpacityChangeSpeed = 2f;

	[NonSerialized]
	public float MinOpacity = 0.6f;

	private Vector3? m_defaultBossNameRelativePosition;

	private dfAtlas EnglishAtlas;

	private dfFontBase EnglishFont;

	private dfAtlas OtherLanguageAtlas;

	private dfFontBase OtherLanguageFont;

	private StringTableManager.GungeonSupportedLanguages m_cachedLanguage;

	private List<bool> m_barsActive = new List<bool>();

	private List<float> m_cachedPercentHealths = new List<float>();

	private List<HealthHaver> m_healthHavers = new List<HealthHaver>();

	private int m_activeBarSprites;

	private bool m_isAnimating;

	private float m_targetPercent;

	private float m_vfxTimer;

	private BossCardUIController m_extantBosscard;

	public bool IsActive
	{
		get
		{
			return m_barsActive[0];
		}
	}

	private void Awake()
	{
		while (m_cachedPercentHealths.Count < barSprites.Count)
		{
			m_cachedPercentHealths.Add(1f);
		}
		while (m_barsActive.Count < barSprites.Count)
		{
			m_barsActive.Add(false);
		}
	}

	private void CheckLanguageFonts()
	{
		if (EnglishFont == null)
		{
			EnglishFont = bossNameLabel.Font;
			EnglishAtlas = bossNameLabel.Atlas;
			OtherLanguageFont = bossNameLabel.GUIManager.DefaultFont;
			OtherLanguageAtlas = bossNameLabel.GUIManager.DefaultAtlas;
		}
		if (StringTableManager.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH)
		{
			if (m_cachedLanguage != 0)
			{
				bossNameLabel.Atlas = EnglishAtlas;
				bossNameLabel.Font = EnglishFont;
			}
		}
		else if (StringTableManager.CurrentLanguage != StringTableManager.GungeonSupportedLanguages.JAPANESE && StringTableManager.CurrentLanguage != StringTableManager.GungeonSupportedLanguages.CHINESE && StringTableManager.CurrentLanguage != StringTableManager.GungeonSupportedLanguages.KOREAN && StringTableManager.CurrentLanguage != StringTableManager.GungeonSupportedLanguages.RUSSIAN && m_cachedLanguage != StringTableManager.CurrentLanguage)
		{
			bossNameLabel.Atlas = OtherLanguageAtlas;
			bossNameLabel.Font = OtherLanguageFont;
		}
		m_cachedLanguage = StringTableManager.CurrentLanguage;
	}

	public void LateUpdate()
	{
		if (!m_defaultBossNameRelativePosition.HasValue)
		{
			m_defaultBossNameRelativePosition = bossNameLabel.RelativePosition;
		}
		m_vfxTimer -= BraveTime.DeltaTime;
		if (m_healthHavers.Count > 0)
		{
			for (int i = 0; i < m_healthHavers.Count; i++)
			{
				if ((bool)m_healthHavers[i])
				{
					UpdateBarSizes(i);
					UpdateBossHealth(barSprites[i], m_healthHavers[i].GetCurrentHealth() / m_healthHavers[i].GetMaxHealth() / (float)barSprites.Count);
				}
			}
		}
		else if (m_barsActive[0] && m_cachedPercentHealths[0] > 0f)
		{
			UpdateBossHealth(barSprites[0], 0f);
		}
		if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.KOREAN || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE)
		{
			bossNameLabel.RelativePosition = m_defaultBossNameRelativePosition.Value + new Vector3(0f, -12f, 0f);
		}
		else if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.JAPANESE || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.RUSSIAN)
		{
			bossNameLabel.RelativePosition = m_defaultBossNameRelativePosition.Value;
		}
		else
		{
			bossNameLabel.RelativePosition = m_defaultBossNameRelativePosition.Value + new Vector3(0f, -12f, 0f);
		}
		bool flag = false;
		float num = BraveCameraUtility.ASPECT / 1.77777779f;
		PlayerController[] allPlayers = GameManager.Instance.AllPlayers;
		for (int j = 0; j < allPlayers.Length; j++)
		{
			Vector2 unitCenter = allPlayers[j].specRigidbody.GetUnitCenter(ColliderType.HitBox);
			Vector2 vector = BraveUtility.WorldPointToViewport(unitCenter, ViewportType.Camera);
			float num2 = (vector.x - 0.5f) * num;
			float num3 = (1f - vector.x) * num;
			if (GameManager.Options.SmallUIEnabled)
			{
				if (IsVertical && num3 < 0.04f && num3 > -0.05f && vector.y > 0.32f && vector.y < 0.7f)
				{
					flag = true;
					break;
				}
				if (!IsVertical && num2 >= -0.115f && num2 <= 0.115f && vector.y > -0.05f && vector.y < 0.06f)
				{
					flag = true;
					break;
				}
			}
			else
			{
				if (IsVertical && num3 < 0.075f && num3 > -0.05f && vector.y > 0.14f && vector.y < 0.9f)
				{
					flag = true;
					break;
				}
				if (!IsVertical && num2 >= -0.23f && num2 <= 0.23f && vector.y > -0.05f && vector.y < 0.11f)
				{
					flag = true;
					break;
				}
			}
		}
		Opacity = Mathf.MoveTowards(Opacity, (!flag) ? 1f : MinOpacity, OpacityChangeSpeed * BraveTime.DeltaTime);
		for (int k = 0; k < barSprites.Count; k++)
		{
			barSprites[k].Opacity = Opacity;
		}
		tankSprite.Opacity = Opacity;
	}

	private void UpdateBarSizes(int barIndex)
	{
		barSprites[barIndex].RelativePosition = barSprites[barIndex].RelativePosition.WithX(barSprites[0].RelativePosition.x + barSprites[0].Size.x / (float)m_activeBarSprites * (float)barIndex);
	}

	public void SetBossName(string bossName)
	{
		CheckLanguageFonts();
		if ((bool)GameManager.Instance.Dungeon)
		{
			bossNameLabel.Glitchy = GameManager.Instance.Dungeon.IsGlitchDungeon;
		}
		bossNameLabel.Text = bossName;
	}

	public void RegisterBossHealthHaver(HealthHaver healthHaver, string bossName = null)
	{
		if (bossName != null)
		{
			SetBossName(bossName);
		}
		if (!m_healthHavers.Contains(healthHaver))
		{
			m_healthHavers.Add(healthHaver);
			if (m_healthHavers.Count > barSprites.Count)
			{
				dfSlicedSprite component = barSprites[0].Parent.AddPrefab(barSprites[0].gameObject).GetComponent<dfSlicedSprite>();
				component.RelativePosition = barSprites[0].RelativePosition;
				barSprites.Add(component);
				m_cachedPercentHealths.Add(1f);
				m_barsActive.Add(false);
			}
			else
			{
				int index = m_healthHavers.Count - 1;
				m_cachedPercentHealths[index] = 1f;
				m_barsActive[index] = false;
			}
		}
	}

	public void DeregisterBossHealthHaver(HealthHaver healthHaver)
	{
		int num = m_healthHavers.IndexOf(healthHaver);
		if (num >= 0)
		{
			UpdateBossHealth(barSprites[num], 0f);
			m_healthHavers[num] = null;
		}
		for (int i = 0; i < m_healthHavers.Count; i++)
		{
			if ((bool)m_healthHavers[i] && m_healthHavers[i].IsAlive)
			{
				return;
			}
		}
		ClearExtraBarData();
		m_healthHavers.Clear();
	}

	private void ClearExtraBarData()
	{
		int num;
		for (num = 1; num < barSprites.Count; num++)
		{
			UnityEngine.Object.Destroy(barSprites[num].gameObject);
			barSprites.RemoveAt(num);
			if (num < m_cachedPercentHealths.Count)
			{
				m_cachedPercentHealths.RemoveAt(num);
			}
			if (num < m_barsActive.Count)
			{
				m_barsActive.RemoveAt(num);
			}
			num--;
		}
	}

	public void ForceUpdateBossHealth(float currentBossHealth, float maxBossHealth, string bossName = null)
	{
		if (bossName != null)
		{
			SetBossName(bossName);
		}
		UpdateBossHealth(barSprites[0], currentBossHealth / maxBossHealth);
	}

	public void DisableBossHealth()
	{
		for (int num = barSprites.Count - 1; num >= 0; num--)
		{
			m_barsActive[num] = false;
			m_cachedPercentHealths[num] = 0f;
		}
		ClearExtraBarData();
		m_activeBarSprites = 0;
		m_healthHavers.Clear();
		tankSprite.IsVisible = false;
	}

	public IEnumerator TriggerBossPortraitCR(PortraitSlideSettings pss)
	{
		GameObject instantiatedBossCardPrefab = UnityEngine.Object.Instantiate(bossCardUIPrefab.gameObject, new Vector3(-100f, -100f, 0f), Quaternion.identity);
		BossCardUIController bosscard = instantiatedBossCardPrefab.GetComponent<BossCardUIController>();
		bosscard.InitializeTexts(pss);
		m_extantBosscard = bosscard;
		yield return StartCoroutine(bosscard.CoreSequence(pss));
	}

	public void EndBossPortraitEarly()
	{
		if ((bool)m_extantBosscard)
		{
			m_extantBosscard.BreakSequence();
			UnityEngine.Object.Destroy(m_extantBosscard.gameObject);
		}
	}

	private void UpdateBossHealth(dfSlicedSprite barSprite, float percent)
	{
		int index = barSprites.IndexOf(barSprite);
		if (percent <= 0f)
		{
			if (!m_barsActive[index])
			{
				Debug.LogError("uh... activating a boss health bar at 0 health. this seems dumb");
				return;
			}
			m_targetPercent = 0f;
			if (!m_isAnimating)
			{
				barSprite.FillAmount = percent;
			}
		}
		else if (!m_barsActive[index])
		{
			TriggerBossHealth(barSprite, percent);
		}
		else
		{
			if (percent > m_cachedPercentHealths[barSprites.IndexOf(barSprite)])
			{
				StartCoroutine(FillBossBar(barSprite));
			}
			m_targetPercent = percent;
			if (!m_isAnimating)
			{
				barSprite.FillAmount = percent;
			}
		}
		if (m_cachedPercentHealths[barSprites.IndexOf(barSprite)] > percent && damagedVFX != null && m_vfxTimer <= 0f)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(damagedVFX);
			dfSprite component = gameObject.GetComponent<dfSprite>();
			dfSpriteAnimation component2 = gameObject.GetComponent<dfSpriteAnimation>();
			component.BringToFront();
			component.Size = component.SpriteInfo.sizeInPixels * Pixelator.Instance.CurrentTileScale;
			barSprite.GetManager().AddControl(component);
			Bounds bounds = barSprite.GetBounds();
			if (IsVertical)
			{
				float y = (bounds.max.y - bounds.min.y) * barSprite.FillAmount + bounds.min.y;
				component.transform.position = new Vector3(bounds.center.x, y, bounds.center.z);
			}
			else
			{
				float x = (bounds.max.x - bounds.min.x) * barSprite.FillAmount + bounds.min.x;
				component.transform.position = new Vector3(x, bounds.center.y, bounds.center.z);
			}
			component.BringToFront();
			component.Opacity = Opacity;
			component2.Play();
			m_vfxTimer = 0.25f;
		}
		m_cachedPercentHealths[barSprites.IndexOf(barSprite)] = percent;
	}

	private void TriggerBossHealth(dfSlicedSprite barSprite, float targetPercent)
	{
		int index = barSprites.IndexOf(barSprite);
		if (!m_barsActive[index])
		{
			m_barsActive[index] = true;
			m_activeBarSprites++;
			for (int i = 0; i < m_healthHavers.Count; i++)
			{
				UpdateBarSizes(i);
			}
			barSprite.FillAmount = 0f;
			tankSprite.IsVisible = true;
			tankSprite.Invalidate();
			barSprite.Invalidate();
			m_targetPercent = targetPercent;
			StartCoroutine(FillBossBar(barSprite));
		}
	}

	private IEnumerator FillBossBar(dfSlicedSprite barSprite)
	{
		float elapsed = 0f;
		m_isAnimating = true;
		float startPercent = barSprite.FillAmount;
		while (elapsed < fillTime)
		{
			elapsed += BraveTime.DeltaTime;
			if ((bool)barSprite)
			{
				barSprite.FillAmount = Mathf.SmoothStep(startPercent, m_targetPercent, elapsed / fillTime);
			}
			yield return null;
		}
		m_isAnimating = false;
	}
}
