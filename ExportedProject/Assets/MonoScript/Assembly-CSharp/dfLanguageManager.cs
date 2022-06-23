using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class dfLanguageManager : MonoBehaviour
{
	[SerializeField]
	private dfLanguageCode currentLanguage;

	[SerializeField]
	private TextAsset dataFile;

	[NonSerialized]
	private TextAsset backupDataFile;

	private Dictionary<string, string> strings = new Dictionary<string, string>();

	public dfLanguageCode CurrentLanguage
	{
		get
		{
			return currentLanguage;
		}
	}

	public TextAsset DataFile
	{
		get
		{
			return dataFile;
		}
		set
		{
			if (value != dataFile)
			{
				dataFile = value;
				LoadLanguage(currentLanguage);
			}
			if (backupDataFile == null)
			{
				backupDataFile = StringTableManager.GetBackupUIDataFile();
			}
		}
	}

	public static void ChangeGungeonLanguage()
	{
		dfLanguageCode languageCodeFromGungeon = GetLanguageCodeFromGungeon();
		dfLanguageManager[] array = UnityEngine.Object.FindObjectsOfType<dfLanguageManager>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].LoadLanguage(languageCodeFromGungeon, true);
		}
	}

	private static dfLanguageCode GetLanguageCodeFromGungeon()
	{
		switch (StringTableManager.CurrentLanguage)
		{
		case StringTableManager.GungeonSupportedLanguages.ENGLISH:
			return dfLanguageCode.EN;
		case StringTableManager.GungeonSupportedLanguages.BRAZILIANPORTUGUESE:
			return dfLanguageCode.PT;
		case StringTableManager.GungeonSupportedLanguages.FRENCH:
			return dfLanguageCode.FR;
		case StringTableManager.GungeonSupportedLanguages.GERMAN:
			return dfLanguageCode.DE;
		case StringTableManager.GungeonSupportedLanguages.ITALIAN:
			return dfLanguageCode.IT;
		case StringTableManager.GungeonSupportedLanguages.SPANISH:
			return dfLanguageCode.ES;
		case StringTableManager.GungeonSupportedLanguages.POLISH:
			return dfLanguageCode.PL;
		case StringTableManager.GungeonSupportedLanguages.RUSSIAN:
			return dfLanguageCode.RU;
		case StringTableManager.GungeonSupportedLanguages.JAPANESE:
			return dfLanguageCode.JA;
		case StringTableManager.GungeonSupportedLanguages.KOREAN:
			return dfLanguageCode.KO;
		case StringTableManager.GungeonSupportedLanguages.RUBEL_TEST:
			return dfLanguageCode.QU;
		case StringTableManager.GungeonSupportedLanguages.CHINESE:
			return dfLanguageCode.ZH;
		default:
			return dfLanguageCode.EN;
		}
	}

	public void Start()
	{
		currentLanguage = GetLanguageCodeFromGungeon();
		dfLanguageCode language = currentLanguage;
		if (currentLanguage == dfLanguageCode.None)
		{
			language = SystemLanguageToLanguageCode(Application.systemLanguage);
		}
		LoadLanguage(language, true);
	}

	private void BraveChangeDataFile(dfLanguageCode language)
	{
		dataFile = StringTableManager.GetUIDataFile();
		if (backupDataFile == null)
		{
			backupDataFile = StringTableManager.GetBackupUIDataFile();
		}
	}

	public void LoadLanguage(dfLanguageCode language, bool forceReload = false)
	{
		currentLanguage = language;
		strings.Clear();
		BraveChangeDataFile(language);
		if (dataFile != null)
		{
			parseDataFile();
		}
		if (!forceReload)
		{
			return;
		}
		dfControl[] componentsInChildren = GetComponentsInChildren<dfControl>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Localize();
		}
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			componentsInChildren[j].PerformLayout();
			if (componentsInChildren[j].LanguageChanged != null)
			{
				componentsInChildren[j].LanguageChanged(componentsInChildren[j]);
			}
		}
	}

	public string GetValue(string key)
	{
		if (strings.Count == 0)
		{
			dfLanguageCode language = currentLanguage;
			if (currentLanguage == dfLanguageCode.None)
			{
				language = SystemLanguageToLanguageCode(Application.systemLanguage);
			}
			LoadLanguage(language);
		}
		string value = string.Empty;
		if (strings.TryGetValue(key, out value))
		{
			return value;
		}
		return key;
	}

	private void parseDataFile()
	{
		string text = dataFile.text.Replace("\r\n", "\n").Trim();
		List<string> list = new List<string>();
		int num = parseLine(text, list, 0);
		int num2 = list.IndexOf(currentLanguage.ToString());
		if (num2 < 0)
		{
			return;
		}
		List<string> list2 = new List<string>();
		while (num < text.Length)
		{
			num = parseLine(text, list2, num);
			if (list2.Count != 0)
			{
				string key = list2[0];
				string value = ((num2 >= list2.Count) ? string.Empty : list2[num2]);
				strings[key] = value;
			}
		}
		string text2 = backupDataFile.text.Replace("\r\n", "\n").Trim();
		List<string> values = new List<string>();
		int num3 = parseLine(text2, values, 0);
		int num4 = 1;
		List<string> list3 = new List<string>();
		while (num3 < text2.Length)
		{
			num3 = parseLine(text2, list3, num3);
			if (list3.Count != 0)
			{
				string key2 = list3[0];
				string value2 = ((num4 >= list3.Count) ? string.Empty : list3[num4]);
				if (!strings.ContainsKey(key2))
				{
					strings[key2] = value2;
				}
			}
		}
	}

	private int parseLine(string data, List<string> values, int index)
	{
		values.Clear();
		bool flag = false;
		StringBuilder stringBuilder = new StringBuilder(256);
		for (; index < data.Length; index++)
		{
			char c = data[index];
			switch (c)
			{
			case '"':
				if (!flag)
				{
					flag = true;
				}
				else if (index + 1 < data.Length && data[index + 1] == c)
				{
					index++;
					stringBuilder.Append(c);
				}
				else
				{
					flag = false;
				}
				continue;
			case ',':
				if (flag)
				{
					stringBuilder.Append(c);
					continue;
				}
				values.Add(stringBuilder.ToString());
				stringBuilder.Length = 0;
				continue;
			case '\n':
				if (flag)
				{
					stringBuilder.Append(c);
					continue;
				}
				break;
			default:
				stringBuilder.Append(c);
				continue;
			}
			index++;
			break;
		}
		if (stringBuilder.Length > 0)
		{
			values.Add(stringBuilder.ToString());
		}
		return index;
	}

	private dfLanguageCode SystemLanguageToLanguageCode(SystemLanguage language)
	{
		switch (language)
		{
		case SystemLanguage.Afrikaans:
			return dfLanguageCode.AF;
		case SystemLanguage.Arabic:
			return dfLanguageCode.AR;
		case SystemLanguage.Basque:
			return dfLanguageCode.EU;
		case SystemLanguage.Belarusian:
			return dfLanguageCode.BE;
		case SystemLanguage.Bulgarian:
			return dfLanguageCode.BG;
		case SystemLanguage.Catalan:
			return dfLanguageCode.CA;
		case SystemLanguage.Chinese:
			return dfLanguageCode.ZH;
		case SystemLanguage.Czech:
			return dfLanguageCode.CS;
		case SystemLanguage.Danish:
			return dfLanguageCode.DA;
		case SystemLanguage.Dutch:
			return dfLanguageCode.NL;
		case SystemLanguage.English:
			return dfLanguageCode.EN;
		case SystemLanguage.Estonian:
			return dfLanguageCode.ES;
		case SystemLanguage.Faroese:
			return dfLanguageCode.FO;
		case SystemLanguage.Finnish:
			return dfLanguageCode.FI;
		case SystemLanguage.French:
			return dfLanguageCode.FR;
		case SystemLanguage.German:
			return dfLanguageCode.DE;
		case SystemLanguage.Greek:
			return dfLanguageCode.EL;
		case SystemLanguage.Hebrew:
			return dfLanguageCode.HE;
		case SystemLanguage.Hungarian:
			return dfLanguageCode.HU;
		case SystemLanguage.Icelandic:
			return dfLanguageCode.IS;
		case SystemLanguage.Indonesian:
			return dfLanguageCode.ID;
		case SystemLanguage.Italian:
			return dfLanguageCode.IT;
		case SystemLanguage.Japanese:
			return dfLanguageCode.JA;
		case SystemLanguage.Korean:
			return dfLanguageCode.KO;
		case SystemLanguage.Latvian:
			return dfLanguageCode.LV;
		case SystemLanguage.Lithuanian:
			return dfLanguageCode.LT;
		case SystemLanguage.Norwegian:
			return dfLanguageCode.NO;
		case SystemLanguage.Polish:
			return dfLanguageCode.PL;
		case SystemLanguage.Portuguese:
			return dfLanguageCode.PT;
		case SystemLanguage.Romanian:
			return dfLanguageCode.RO;
		case SystemLanguage.Russian:
			return dfLanguageCode.RU;
		case SystemLanguage.SerboCroatian:
			return dfLanguageCode.SH;
		case SystemLanguage.Slovak:
			return dfLanguageCode.SK;
		case SystemLanguage.Slovenian:
			return dfLanguageCode.SL;
		case SystemLanguage.Spanish:
			return dfLanguageCode.ES;
		case SystemLanguage.Swedish:
			return dfLanguageCode.SV;
		case SystemLanguage.Thai:
			return dfLanguageCode.TH;
		case SystemLanguage.Turkish:
			return dfLanguageCode.TR;
		case SystemLanguage.Ukrainian:
			return dfLanguageCode.UK;
		case SystemLanguage.Unknown:
			return dfLanguageCode.EN;
		case SystemLanguage.Vietnamese:
			return dfLanguageCode.VI;
		default:
			throw new ArgumentException("Unknown system language: " + language);
		}
	}
}
