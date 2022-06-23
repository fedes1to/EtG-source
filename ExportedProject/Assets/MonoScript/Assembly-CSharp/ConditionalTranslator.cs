using UnityEngine;

public class ConditionalTranslator : MonoBehaviour
{
	public string EnglishText;

	public string NonEnglishItemsKey;

	public bool useUiTable;

	private dfControl m_control;

	private void Start()
	{
		m_control = GetComponent<dfControl>();
		if ((bool)m_control)
		{
			m_control.IsEnabledChanged += HandleTranslation;
			m_control.IsVisibleChanged += HandleTranslation;
		}
	}

	private void SetText(string targetText)
	{
		if (m_control is dfLabel)
		{
			(m_control as dfLabel).Text = targetText;
		}
	}

	private void HandleTranslation(dfControl control, bool value)
	{
		if (StringTableManager.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH)
		{
			m_control.IsLocalized = false;
			SetText(EnglishText);
		}
		else if (useUiTable)
		{
			m_control.IsLocalized = true;
			SetText(NonEnglishItemsKey);
		}
		else
		{
			m_control.IsLocalized = false;
			SetText(StringTableManager.GetItemsString(NonEnglishItemsKey));
		}
	}
}
