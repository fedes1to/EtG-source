using System.Collections;
using Dungeonator;
using UnityEngine;

public class LevelNameUIManager : MonoBehaviour
{
	public dfLabel levelNameLabel;

	public dfLabel levelNumberLabel;

	public dfSlicedSprite levelNameShadow;

	public dfSlicedSprite levelNumberShadow;

	public dfSlicedSprite hyphen;

	public float initialDelay = 0.5f;

	public float fadeInTime = 1f;

	public float displayTime = 3f;

	public float fadeOutTime = 1f;

	private const float c_widthBoost = 6f;

	private const float c_minWidth = 40f;

	private dfPanel m_panel;

	private bool m_displaying;

	public void ShowLevelName(Dungeon d)
	{
		StartCoroutine(ShowLevelName_CR(d));
	}

	public IEnumerator ShowLevelName_CR(Dungeon d)
	{
		while (GameManager.Instance.IsLoadingLevel)
		{
			yield return null;
		}
		m_displaying = true;
		yield return null;
		while (GameManager.Instance.IsSelectingCharacter || GameManager.Instance.IsLoadingLevel)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.25f);
		if (!m_displaying)
		{
			yield break;
		}
		m_panel = GetComponent<dfPanel>();
		if (d.DungeonFloorLevelTextOverride == "no override")
		{
			int num = -1;
			GameLevelDefinition lastLoadedLevelDefinition = GameManager.Instance.GetLastLoadedLevelDefinition();
			if (lastLoadedLevelDefinition != null)
			{
				num = GameManager.Instance.dungeonFloors.IndexOf(lastLoadedLevelDefinition);
			}
			if (lastLoadedLevelDefinition != null && lastLoadedLevelDefinition.lastSelectedFlowEntry != null && lastLoadedLevelDefinition.lastSelectedFlowEntry.overridesLevelDetailText)
			{
				levelNumberLabel.Text = levelNumberLabel.ForceGetLocalizedValue(lastLoadedLevelDefinition.lastSelectedFlowEntry.overrideLevelDetailText);
			}
			else
			{
				levelNumberLabel.Text = levelNumberLabel.ForceGetLocalizedValue("#LEVEL") + ((num <= 0) ? "???" : num.ToString());
			}
			levelNumberShadow.Width = levelNumberLabel.GetAutosizeWidth() / 3f;
		}
		else if (string.IsNullOrEmpty(d.DungeonFloorLevelTextOverride))
		{
			levelNumberLabel.Text = string.Empty;
			levelNumberShadow.Width = 0f;
			levelNumberShadow.IsVisible = false;
		}
		else
		{
			levelNumberLabel.Text = levelNumberLabel.ForceGetLocalizedValue(d.DungeonFloorLevelTextOverride);
			levelNumberShadow.Width = levelNumberLabel.GetAutosizeWidth() / 3f;
		}
		levelNameLabel.Text = levelNameLabel.ForceGetLocalizedValue(d.DungeonFloorName);
		levelNameShadow.Width = levelNameLabel.GetAutosizeWidth() / 3f;
		hyphen.Width = (Mathf.Max(levelNameLabel.GetAutosizeWidth(), levelNumberLabel.GetAutosizeWidth()) + 88f) / 3f;
		levelNumberShadow.Width = Mathf.Max(40f, levelNumberShadow.Width + 6f);
		levelNameShadow.Width = Mathf.Max(40f, levelNameShadow.Width + 6f);
		if (string.IsNullOrEmpty(levelNumberLabel.Text))
		{
			levelNumberLabel.IsVisible = false;
			levelNumberShadow.IsVisible = false;
		}
		StartCoroutine(HandleLevelName());
	}

	public void ShowCustomLevelName(string name, string levelNumText = "Level 0")
	{
		m_panel = GetComponent<dfPanel>();
		levelNameLabel.Text = name;
		levelNameShadow.Width = levelNameLabel.GetAutosizeWidth() / 3f;
		levelNumberLabel.Text = levelNumText;
		levelNumberShadow.Width = levelNumberLabel.GetAutosizeWidth() / 3f;
		hyphen.Width = (Mathf.Max(levelNameLabel.GetAutosizeWidth(), levelNumberLabel.GetAutosizeWidth()) + 88f) / 3f;
		m_displaying = true;
		StartCoroutine(HandleLevelName());
	}

	public void BanishLevelNameText()
	{
		if (m_displaying)
		{
			m_displaying = false;
		}
	}

	private IEnumerator HandleLevelName()
	{
		yield return new WaitForSeconds(initialDelay);
		m_panel.Opacity = 0f;
		m_panel.IsVisible = true;
		float elapsed3 = 0f;
		while (elapsed3 < fadeInTime && m_displaying)
		{
			elapsed3 += BraveTime.DeltaTime;
			float t2 = Mathf.Clamp01(elapsed3 / fadeInTime);
			m_panel.Opacity = t2;
			yield return null;
		}
		elapsed3 = 0f;
		while (elapsed3 < displayTime && m_displaying)
		{
			elapsed3 += BraveTime.DeltaTime;
			yield return null;
		}
		elapsed3 = 0f;
		while (elapsed3 < fadeOutTime && m_displaying)
		{
			elapsed3 += BraveTime.DeltaTime;
			float t = Mathf.Clamp01(elapsed3 / fadeOutTime);
			m_panel.Opacity = 1f - t;
			yield return null;
		}
		m_panel.IsVisible = false;
	}
}
