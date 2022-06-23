using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using InControl;
using UnityEngine;

public static class StringTableManager
{
	public enum GungeonSupportedLanguages
	{
		ENGLISH,
		RUBEL_TEST,
		FRENCH,
		SPANISH,
		ITALIAN,
		GERMAN,
		BRAZILIANPORTUGUESE,
		JAPANESE,
		KOREAN,
		RUSSIAN,
		POLISH,
		CHINESE
	}

	public abstract class StringCollection
	{
		public abstract int Count();

		public abstract void AddString(string value, float weight);

		public abstract string GetCombinedString();

		public abstract string GetExactString(int index);

		public abstract string GetWeightedString();

		public abstract string GetWeightedStringSequential(ref int lastIndex, out bool isLast, bool repeatLast = false);
	}

	public class SimpleStringCollection : StringCollection
	{
		private string singleString;

		public override int Count()
		{
			return 1;
		}

		public override void AddString(string value, float weight)
		{
			singleString = value;
		}

		public override string GetCombinedString()
		{
			return singleString;
		}

		public override string GetExactString(int index)
		{
			return singleString;
		}

		public override string GetWeightedString()
		{
			return singleString;
		}

		public override string GetWeightedStringSequential(ref int lastIndex, out bool isLast, bool repeatLast = false)
		{
			isLast = true;
			return singleString;
		}
	}

	public class ComplexStringCollection : StringCollection
	{
		private List<string> strings;

		private List<float> weights;

		private float maxWeight;

		public ComplexStringCollection()
		{
			strings = new List<string>();
			weights = new List<float>();
			maxWeight = 0f;
		}

		public override int Count()
		{
			return strings.Count;
		}

		public override void AddString(string value, float weight)
		{
			strings.Add(value);
			weights.Add(weight);
			maxWeight += weight;
		}

		public override string GetCombinedString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < strings.Count; i++)
			{
				stringBuilder.AppendLine(strings[i]);
			}
			return stringBuilder.ToString();
		}

		public override string GetExactString(int index)
		{
			if (strings.Count == 0)
			{
				return string.Empty;
			}
			return strings[index % strings.Count];
		}

		public override string GetWeightedString()
		{
			float num = Random.value * maxWeight;
			float num2 = 0f;
			for (int i = 0; i < strings.Count; i++)
			{
				num2 += weights[i];
				if (num2 >= num)
				{
					return strings[i];
				}
			}
			if (strings.Count == 0)
			{
				return null;
			}
			return strings[0];
		}

		public override string GetWeightedStringSequential(ref int lastIndex, out bool isLast, bool repeatLast = false)
		{
			int num = lastIndex + 1;
			isLast = num == strings.Count - 1;
			if (num >= strings.Count)
			{
				if (repeatLast)
				{
					num = lastIndex;
					isLast = true;
				}
				else
				{
					num = 0;
				}
			}
			if (num < 0)
			{
				num = 0;
			}
			if (num >= strings.Count)
			{
				num = strings.Count - 1;
			}
			if (strings.Count == 0)
			{
				return string.Empty;
			}
			lastIndex = num;
			return strings[num];
		}
	}

	private static string m_currentFile = "english";

	private static string m_currentSubDirectory = "english_items";

	private static Stack<List<string>> m_tokenLists;

	private static Stack<StringBuilder> m_postprocessors;

	private static Dictionary<string, StringCollection> m_coreTable;

	private static Dictionary<string, StringCollection> m_itemsTable;

	private static Dictionary<string, StringCollection> m_enemiesTable;

	private static Dictionary<string, StringCollection> m_uiTable;

	private static Dictionary<string, StringCollection> m_introTable;

	private static Dictionary<string, StringCollection> m_synergyTable;

	private static Dictionary<string, StringCollection> m_backupCoreTable;

	private static Dictionary<string, StringCollection> m_backupItemsTable;

	private static Dictionary<string, StringCollection> m_backupEnemiesTable;

	private static Dictionary<string, StringCollection> m_backupUiTable;

	private static Dictionary<string, StringCollection> m_backupIntroTable;

	private static Dictionary<string, StringCollection> m_backupSynergyTable;

	public static GungeonSupportedLanguages CurrentLanguage
	{
		get
		{
			return GameManager.Options.CurrentLanguage;
		}
		set
		{
			SetNewLanguage(value, true);
		}
	}

	public static Dictionary<string, StringCollection> IntroTable
	{
		get
		{
			if (m_introTable == null)
			{
				m_introTable = LoadIntroTable(m_currentSubDirectory);
			}
			if (m_backupIntroTable == null)
			{
				m_backupIntroTable = LoadIntroTable("english_items");
			}
			return m_introTable;
		}
	}

	public static Dictionary<string, StringCollection> CoreTable
	{
		get
		{
			if (m_coreTable == null)
			{
				m_coreTable = LoadTables(m_currentFile);
			}
			if (m_backupCoreTable == null)
			{
				m_backupCoreTable = LoadTables("english");
			}
			return m_coreTable;
		}
	}

	public static Dictionary<string, StringCollection> ItemTable
	{
		get
		{
			if (m_itemsTable == null)
			{
				m_itemsTable = LoadItemsTable(m_currentSubDirectory);
			}
			if (m_backupItemsTable == null)
			{
				m_backupItemsTable = LoadItemsTable("english_items");
			}
			return m_itemsTable;
		}
	}

	public static Dictionary<string, StringCollection> EnemyTable
	{
		get
		{
			if (m_enemiesTable == null)
			{
				m_enemiesTable = LoadEnemiesTable(m_currentSubDirectory);
			}
			if (m_backupEnemiesTable == null)
			{
				m_backupEnemiesTable = LoadEnemiesTable("english_items");
			}
			return m_enemiesTable;
		}
	}

	public static void ReloadAllTables()
	{
		m_coreTable = null;
		m_enemiesTable = null;
		m_itemsTable = null;
		m_uiTable = null;
		m_introTable = null;
		m_synergyTable = null;
		m_backupCoreTable = null;
		m_backupEnemiesTable = null;
		m_backupIntroTable = null;
		m_backupItemsTable = null;
		m_backupSynergyTable = null;
		m_backupUiTable = null;
	}

	public static string GetString(string key)
	{
		if (m_coreTable == null)
		{
			m_coreTable = LoadTables(m_currentFile);
		}
		if (m_backupCoreTable == null)
		{
			m_backupCoreTable = LoadTables("english");
		}
		if (m_coreTable.ContainsKey(key))
		{
			return PostprocessString(m_coreTable[key].GetWeightedString());
		}
		if (m_backupCoreTable.ContainsKey(key))
		{
			return PostprocessString(m_backupCoreTable[key].GetWeightedString());
		}
		return "STRING_NOT_FOUND";
	}

	public static string GetExactString(string key, int index)
	{
		if (m_coreTable == null)
		{
			m_coreTable = LoadTables(m_currentFile);
		}
		if (m_backupCoreTable == null)
		{
			m_backupCoreTable = LoadTables("english");
		}
		if (m_coreTable.ContainsKey(key))
		{
			return PostprocessString(m_coreTable[key].GetExactString(index));
		}
		if (m_backupCoreTable.ContainsKey(key))
		{
			return PostprocessString(m_backupCoreTable[key].GetExactString(index));
		}
		return "STRING_NOT_FOUND";
	}

	public static string GetEnemiesLongDescription(string key)
	{
		if (m_enemiesTable == null)
		{
			m_enemiesTable = LoadEnemiesTable(m_currentSubDirectory);
		}
		if (m_backupEnemiesTable == null)
		{
			m_backupEnemiesTable = LoadEnemiesTable("english_items");
		}
		if (m_enemiesTable.ContainsKey(key))
		{
			return PostprocessString(m_enemiesTable[key].GetCombinedString());
		}
		if (m_backupEnemiesTable.ContainsKey(key))
		{
			return PostprocessString(m_backupEnemiesTable[key].GetCombinedString());
		}
		return "ENEMIES_STRING_NOT_FOUND";
	}

	public static string GetItemsLongDescription(string key)
	{
		if (m_itemsTable == null)
		{
			m_itemsTable = LoadItemsTable(m_currentSubDirectory);
		}
		if (m_backupItemsTable == null)
		{
			m_backupItemsTable = LoadItemsTable("english_items");
		}
		if (m_itemsTable.ContainsKey(key))
		{
			return PostprocessString(m_itemsTable[key].GetCombinedString());
		}
		if (m_backupItemsTable.ContainsKey(key))
		{
			return PostprocessString(m_backupItemsTable[key].GetCombinedString());
		}
		return "ITEMS_STRING_NOT_FOUND";
	}

	public static string GetEnemiesString(string key, int index = -1)
	{
		if (m_enemiesTable == null)
		{
			m_enemiesTable = LoadEnemiesTable(m_currentSubDirectory);
		}
		if (m_backupEnemiesTable == null)
		{
			m_backupEnemiesTable = LoadEnemiesTable("english_items");
		}
		if (m_enemiesTable.ContainsKey(key))
		{
			if (index == -1)
			{
				string weightedString = m_enemiesTable[key].GetWeightedString();
				return PostprocessString(weightedString);
			}
			return PostprocessString(m_enemiesTable[key].GetExactString(index));
		}
		if (m_backupEnemiesTable.ContainsKey(key))
		{
			if (index == -1)
			{
				string weightedString2 = m_backupEnemiesTable[key].GetWeightedString();
				return PostprocessString(weightedString2);
			}
			return PostprocessString(m_backupEnemiesTable[key].GetExactString(index));
		}
		return "ENEMIES_STRING_NOT_FOUND";
	}

	public static string GetIntroString(string key)
	{
		if (m_introTable == null)
		{
			m_introTable = LoadIntroTable(m_currentSubDirectory);
		}
		if (m_backupIntroTable == null)
		{
			m_backupIntroTable = LoadIntroTable("english_items");
		}
		if (m_introTable.ContainsKey(key))
		{
			string weightedString = m_introTable[key].GetWeightedString();
			return PostprocessString(weightedString);
		}
		if (m_backupIntroTable.ContainsKey(key))
		{
			string weightedString2 = m_backupIntroTable[key].GetWeightedString();
			return PostprocessString(weightedString2);
		}
		return "INTRO_STRING_NOT_FOUND";
	}

	public static string GetItemsString(string key, int index = -1)
	{
		if (m_itemsTable == null)
		{
			m_itemsTable = LoadItemsTable(m_currentSubDirectory);
		}
		if (m_backupItemsTable == null)
		{
			m_backupItemsTable = LoadItemsTable("english_items");
		}
		if (m_itemsTable.ContainsKey(key))
		{
			if (index == -1)
			{
				string weightedString = m_itemsTable[key].GetWeightedString();
				return PostprocessString(weightedString);
			}
			return PostprocessString(m_itemsTable[key].GetExactString(index));
		}
		if (m_backupItemsTable.ContainsKey(key))
		{
			if (index == -1)
			{
				string weightedString2 = m_backupItemsTable[key].GetWeightedString();
				return PostprocessString(weightedString2);
			}
			return PostprocessString(m_backupItemsTable[key].GetExactString(index));
		}
		return "ITEMS_STRING_NOT_FOUND";
	}

	public static string GetUIString(string key, int index = -1)
	{
		if (m_uiTable == null)
		{
			m_uiTable = LoadUITable(m_currentSubDirectory);
		}
		if (m_backupUiTable == null)
		{
			m_backupUiTable = LoadUITable(m_currentSubDirectory);
		}
		if (m_uiTable.ContainsKey(key))
		{
			if (index == -1)
			{
				return PostprocessString(m_uiTable[key].GetWeightedString());
			}
			return PostprocessString(m_uiTable[key].GetExactString(index));
		}
		if (m_backupUiTable.ContainsKey(key))
		{
			if (index == -1)
			{
				return PostprocessString(m_backupUiTable[key].GetWeightedString());
			}
			return PostprocessString(m_backupUiTable[key].GetExactString(index));
		}
		return "ITEMS_STRING_NOT_FOUND";
	}

	public static string GetSynergyString(string key, int index = -1)
	{
		if (m_synergyTable == null)
		{
			m_synergyTable = LoadSynergyTable(m_currentSubDirectory);
		}
		if (m_backupSynergyTable == null)
		{
			m_backupSynergyTable = LoadSynergyTable("english_items");
		}
		if (m_synergyTable.ContainsKey(key))
		{
			if (index == -1)
			{
				string weightedString = m_synergyTable[key].GetWeightedString();
				return PostprocessString(weightedString);
			}
			return PostprocessString(m_synergyTable[key].GetExactString(index));
		}
		return string.Empty;
	}

	public static string GetStringSequential(string key, ref int lastIndex, bool repeatLast = false)
	{
		bool isLast;
		return GetStringSequential(key, ref lastIndex, out isLast, repeatLast);
	}

	public static string GetStringSequential(string key, ref int lastIndex, out bool isLast, bool repeatLast = false)
	{
		isLast = false;
		if (m_coreTable == null)
		{
			m_coreTable = LoadTables(m_currentFile);
		}
		if (m_backupCoreTable == null)
		{
			m_backupCoreTable = LoadTables("english");
		}
		if (m_coreTable.ContainsKey(key))
		{
			string weightedStringSequential = m_coreTable[key].GetWeightedStringSequential(ref lastIndex, out isLast, repeatLast);
			return PostprocessString(weightedStringSequential);
		}
		if (m_backupCoreTable.ContainsKey(key))
		{
			string weightedStringSequential2 = m_backupCoreTable[key].GetWeightedStringSequential(ref lastIndex, out isLast, repeatLast);
			return PostprocessString(weightedStringSequential2);
		}
		return "STRING_NOT_FOUND";
	}

	public static string GetStringPersistentSequential(string key)
	{
		if (m_coreTable == null)
		{
			m_coreTable = LoadTables(m_currentFile);
		}
		if (m_backupCoreTable == null)
		{
			m_backupCoreTable = LoadTables("english");
		}
		if (m_coreTable.ContainsKey(key))
		{
			int lastIndex = GameStatsManager.Instance.GetPersistentStringLastIndex(key);
			bool isLast;
			string weightedStringSequential = m_coreTable[key].GetWeightedStringSequential(ref lastIndex, out isLast);
			GameStatsManager.Instance.SetPersistentStringLastIndex(key, lastIndex);
			return PostprocessString(weightedStringSequential);
		}
		if (m_backupCoreTable.ContainsKey(key))
		{
			int lastIndex2 = GameStatsManager.Instance.GetPersistentStringLastIndex(key);
			bool isLast2;
			string weightedStringSequential2 = m_backupCoreTable[key].GetWeightedStringSequential(ref lastIndex2, out isLast2);
			GameStatsManager.Instance.SetPersistentStringLastIndex(key, lastIndex2);
			return PostprocessString(weightedStringSequential2);
		}
		return "STRING_NOT_FOUND";
	}

	public static int GetNumStrings(string key)
	{
		if (m_coreTable == null)
		{
			m_coreTable = LoadTables(m_currentFile);
		}
		if (m_backupCoreTable == null)
		{
			m_backupCoreTable = LoadTables("english");
		}
		if (m_coreTable.ContainsKey(key))
		{
			return m_coreTable[key].Count();
		}
		if (m_backupCoreTable.ContainsKey(key))
		{
			return m_backupCoreTable[key].Count();
		}
		return 0;
	}

	public static string GetLongString(string key)
	{
		if (m_coreTable == null)
		{
			m_coreTable = LoadTables(m_currentFile);
		}
		if (m_backupCoreTable == null)
		{
			m_backupCoreTable = LoadTables("english");
		}
		if (m_coreTable.ContainsKey(key))
		{
			return PostprocessString(m_coreTable[key].GetCombinedString());
		}
		if (m_backupCoreTable.ContainsKey(key))
		{
			return PostprocessString(m_backupCoreTable[key].GetCombinedString());
		}
		return "STRING_NOT_FOUND";
	}

	public static void LoadTablesIfNecessary()
	{
		if (m_coreTable == null)
		{
			m_coreTable = LoadTables(m_currentFile);
		}
		if (m_backupCoreTable == null)
		{
			m_backupCoreTable = LoadTables("english");
		}
	}

	public static Dictionary<string, StringCollection> LoadTables(string currentFile)
	{
		TextAsset textAsset = (TextAsset)BraveResources.Load("strings/" + currentFile, typeof(TextAsset), ".txt");
		if (textAsset == null)
		{
			Debug.LogError("Failed to load string table.");
			return null;
		}
		StringReader stringReader = new StringReader(textAsset.text);
		Dictionary<string, StringCollection> dictionary = new Dictionary<string, StringCollection>();
		StringCollection stringCollection = null;
		string text;
		while ((text = stringReader.ReadLine()) != null)
		{
			if (text.StartsWith("//"))
			{
				continue;
			}
			if (text.StartsWith("#"))
			{
				stringCollection = new ComplexStringCollection();
				if (dictionary.ContainsKey(text))
				{
					Debug.LogError("Attempting to add the key " + text + " twice to the string table!");
				}
				else
				{
					dictionary.Add(text, stringCollection);
				}
				continue;
			}
			string[] array = text.Split('|');
			if (array.Length == 1)
			{
				stringCollection.AddString(array[0], 1f);
				continue;
			}
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			stringCollection.AddString(array[1], float.Parse(array[0], NumberStyles.Any, invariantCulture.NumberFormat));
		}
		return dictionary;
	}

	public static void SetNewLanguage(GungeonSupportedLanguages language, bool force = false)
	{
		if (force || CurrentLanguage != language)
		{
			switch (language)
			{
			case GungeonSupportedLanguages.ENGLISH:
				m_currentFile = "english";
				m_currentSubDirectory = "english_items";
				break;
			case GungeonSupportedLanguages.BRAZILIANPORTUGUESE:
				m_currentFile = "portuguese";
				m_currentSubDirectory = "portuguese_items";
				break;
			case GungeonSupportedLanguages.FRENCH:
				m_currentFile = "french";
				m_currentSubDirectory = "french_items";
				break;
			case GungeonSupportedLanguages.GERMAN:
				m_currentFile = "german";
				m_currentSubDirectory = "german_items";
				break;
			case GungeonSupportedLanguages.ITALIAN:
				m_currentFile = "italian";
				m_currentSubDirectory = "italian_items";
				break;
			case GungeonSupportedLanguages.SPANISH:
				m_currentFile = "spanish";
				m_currentSubDirectory = "spanish_items";
				break;
			case GungeonSupportedLanguages.RUSSIAN:
				m_currentFile = "russian";
				m_currentSubDirectory = "russian_items";
				break;
			case GungeonSupportedLanguages.POLISH:
				m_currentFile = "polish";
				m_currentSubDirectory = "polish_items";
				break;
			case GungeonSupportedLanguages.JAPANESE:
				m_currentFile = "japanese";
				m_currentSubDirectory = "japanese_items";
				break;
			case GungeonSupportedLanguages.KOREAN:
				m_currentFile = "korean";
				m_currentSubDirectory = "korean_items";
				break;
			case GungeonSupportedLanguages.CHINESE:
				m_currentFile = "chinese";
				m_currentSubDirectory = "chinese_items";
				break;
			default:
				m_currentFile = "english";
				m_currentSubDirectory = "english_items";
				break;
			}
			ReloadAllTables();
			dfLanguageManager.ChangeGungeonLanguage();
			JournalEntry.ReloadDataSemaphore++;
		}
	}

	private static Dictionary<string, StringCollection> LoadEnemiesTable(string subDirectory)
	{
		TextAsset textAsset = (TextAsset)BraveResources.Load("strings/" + subDirectory + "/enemies", typeof(TextAsset), ".txt");
		if (textAsset == null)
		{
			Debug.LogError("Failed to load string table: ENEMIES.");
			return null;
		}
		StringReader stringReader = new StringReader(textAsset.text);
		Dictionary<string, StringCollection> dictionary = new Dictionary<string, StringCollection>();
		StringCollection stringCollection = null;
		string text;
		while ((text = stringReader.ReadLine()) != null)
		{
			if (text.StartsWith("//"))
			{
				continue;
			}
			if (text.StartsWith("#"))
			{
				stringCollection = new ComplexStringCollection();
				if (dictionary.ContainsKey(text))
				{
					Debug.LogError("Failed to add duplicate key to items table: " + text);
				}
				else
				{
					dictionary.Add(text, stringCollection);
				}
				continue;
			}
			string[] array = text.Split('|');
			if (array.Length == 1)
			{
				stringCollection.AddString(array[0], 1f);
			}
			else
			{
				stringCollection.AddString(array[1], float.Parse(array[0]));
			}
		}
		return dictionary;
	}

	public static TextAsset GetUIDataFile()
	{
		return (TextAsset)BraveResources.Load("strings/" + m_currentSubDirectory + "/ui", typeof(TextAsset), ".txt");
	}

	public static TextAsset GetBackupUIDataFile()
	{
		return (TextAsset)BraveResources.Load("strings/english_items/ui", typeof(TextAsset), ".txt");
	}

	private static Dictionary<string, StringCollection> LoadSynergyTable(string subDirectory)
	{
		TextAsset textAsset = (TextAsset)BraveResources.Load("strings/" + subDirectory + "/synergies", typeof(TextAsset), ".txt");
		Dictionary<string, StringCollection> dictionary = new Dictionary<string, StringCollection>();
		if (textAsset == null)
		{
			Debug.LogError("Failed to load string table: ITEMS.");
			return dictionary;
		}
		StringReader stringReader = new StringReader(textAsset.text);
		StringCollection stringCollection = null;
		string text;
		while ((text = stringReader.ReadLine()) != null)
		{
			if (text.StartsWith("//"))
			{
				continue;
			}
			if (text.StartsWith("#"))
			{
				stringCollection = new ComplexStringCollection();
				if (dictionary.ContainsKey(text))
				{
					Debug.LogError("Failed to add duplicate key to synergies table: " + text);
				}
				else
				{
					dictionary.Add(text, stringCollection);
				}
				continue;
			}
			string[] array = text.Split('|');
			if (array.Length == 1)
			{
				stringCollection.AddString(array[0], 1f);
			}
			else
			{
				stringCollection.AddString(array[1], float.Parse(array[0]));
			}
		}
		return dictionary;
	}

	private static Dictionary<string, StringCollection> LoadUITable(string subDirectory)
	{
		TextAsset textAsset = (TextAsset)BraveResources.Load("strings/" + subDirectory + "/ui", typeof(TextAsset), ".txt");
		if (textAsset == null)
		{
			Debug.LogError("Failed to load string table: ITEMS.");
			return null;
		}
		StringReader stringReader = new StringReader(textAsset.text);
		Dictionary<string, StringCollection> dictionary = new Dictionary<string, StringCollection>();
		StringCollection stringCollection = null;
		string text;
		while ((text = stringReader.ReadLine()) != null)
		{
			if (text.StartsWith("//"))
			{
				continue;
			}
			if (text.StartsWith("#"))
			{
				stringCollection = new ComplexStringCollection();
				if (dictionary.ContainsKey(text))
				{
					Debug.LogError("Failed to add duplicate key to items table: " + text);
				}
				else
				{
					dictionary.Add(text, stringCollection);
				}
				continue;
			}
			string[] array = text.Split('|');
			if (array.Length == 1)
			{
				stringCollection.AddString(array[0], 1f);
			}
			else
			{
				stringCollection.AddString(array[1], float.Parse(array[0]));
			}
		}
		return dictionary;
	}

	private static Dictionary<string, StringCollection> LoadIntroTable(string subDirectory)
	{
		TextAsset textAsset = (TextAsset)BraveResources.Load("strings/" + subDirectory + "/intro", typeof(TextAsset), ".txt");
		if (textAsset == null)
		{
			Debug.LogError("Failed to load string table: INTRO.");
			return null;
		}
		StringReader stringReader = new StringReader(textAsset.text);
		Dictionary<string, StringCollection> dictionary = new Dictionary<string, StringCollection>();
		StringCollection stringCollection = null;
		string text;
		while ((text = stringReader.ReadLine()) != null)
		{
			if (text.StartsWith("//"))
			{
				continue;
			}
			if (text.StartsWith("#"))
			{
				stringCollection = new ComplexStringCollection();
				if (dictionary.ContainsKey(text))
				{
					Debug.LogError("Failed to add duplicate key to items table: " + text);
				}
				else
				{
					dictionary.Add(text, stringCollection);
				}
				continue;
			}
			string[] array = text.Split('|');
			if (array.Length == 1)
			{
				stringCollection.AddString(array[0], 1f);
			}
			else
			{
				stringCollection.AddString(array[1], float.Parse(array[0]));
			}
		}
		return dictionary;
	}

	private static Dictionary<string, StringCollection> LoadItemsTable(string subDirectory)
	{
		TextAsset textAsset = (TextAsset)BraveResources.Load("strings/" + subDirectory + "/items", typeof(TextAsset), ".txt");
		if (textAsset == null)
		{
			Debug.LogError("Failed to load string table: ITEMS.");
			return null;
		}
		StringReader stringReader = new StringReader(textAsset.text);
		Dictionary<string, StringCollection> dictionary = new Dictionary<string, StringCollection>();
		StringCollection stringCollection = null;
		string text;
		while ((text = stringReader.ReadLine()) != null)
		{
			if (text.StartsWith("//"))
			{
				continue;
			}
			if (text.StartsWith("#"))
			{
				stringCollection = new ComplexStringCollection();
				if (dictionary.ContainsKey(text))
				{
					Debug.LogError("Failed to add duplicate key to items table: " + text);
				}
				else
				{
					dictionary.Add(text, stringCollection);
				}
				continue;
			}
			string[] array = text.Split('|');
			if (array.Length == 1)
			{
				stringCollection.AddString(array[0], 1f);
			}
			else
			{
				stringCollection.AddString(array[1], float.Parse(array[0]));
			}
		}
		return dictionary;
	}

	public static string GetBindingText(GungeonActions.GungeonActionType ActionType)
	{
		GungeonActions gungeonActions = null;
		gungeonActions = (GameManager.Instance.IsSelectingCharacter ? BraveInput.PlayerlessInstance.ActiveActions : BraveInput.GetInstanceForPlayer(GameManager.Instance.PrimaryPlayer.PlayerIDX).ActiveActions);
		if (gungeonActions == null)
		{
			return string.Empty;
		}
		PlayerAction actionFromType = gungeonActions.GetActionFromType(ActionType);
		if (actionFromType == null || actionFromType.Bindings == null)
		{
			return string.Empty;
		}
		bool flag = false;
		string text = "-";
		for (int i = 0; i < actionFromType.Bindings.Count; i++)
		{
			BindingSource bindingSource = actionFromType.Bindings[i];
			if ((bindingSource.BindingSourceType == BindingSourceType.KeyBindingSource || bindingSource.BindingSourceType == BindingSourceType.MouseBindingSource) && !flag)
			{
				flag = true;
				text = bindingSource.Name;
				break;
			}
		}
		return text.Trim();
	}

	private static PlayerController GetTalkingPlayer()
	{
		List<TalkDoerLite> allNpcs = StaticReferenceManager.AllNpcs;
		for (int i = 0; i < allNpcs.Count; i++)
		{
			if ((bool)allNpcs[i] && (!allNpcs[i].IsTalking || !allNpcs[i].TalkingPlayer || GameManager.Instance.HasPlayer(allNpcs[i].TalkingPlayer)) && allNpcs[i].IsTalking && (bool)allNpcs[i].TalkingPlayer)
			{
				return allNpcs[i].TalkingPlayer;
			}
		}
		return GameManager.Instance.PrimaryPlayer;
	}

	private static string GetTalkingPlayerName()
	{
		PlayerController talkingPlayer = GetTalkingPlayer();
		if (talkingPlayer.IsThief)
		{
			return "#THIEF_NAME";
		}
		if (talkingPlayer.characterIdentity == PlayableCharacters.Eevee)
		{
			return "#PLAYER_NAME_RANDOM";
		}
		if (talkingPlayer.characterIdentity == PlayableCharacters.Gunslinger)
		{
			return "#PLAYER_NAME_GUNSLINGER";
		}
		return "#PLAYER_NAME_" + talkingPlayer.characterIdentity.ToString().ToUpperInvariant();
	}

	private static string GetTalkingPlayerNick()
	{
		PlayerController talkingPlayer = GetTalkingPlayer();
		if (talkingPlayer.IsThief)
		{
			return "#THIEF_NAME";
		}
		if (talkingPlayer.characterIdentity == PlayableCharacters.Eevee)
		{
			return "#PLAYER_NICK_RANDOM";
		}
		if (talkingPlayer.characterIdentity == PlayableCharacters.Gunslinger)
		{
			return "#PLAYER_NICK_GUNSLINGER";
		}
		return "#PLAYER_NICK_" + talkingPlayer.characterIdentity.ToString().ToUpperInvariant();
	}

	public static string GetPlayerName(PlayableCharacters player)
	{
		return GetString("#PLAYER_NAME_" + player.ToString().ToUpperInvariant());
	}

	public static string EvaluateReplacementToken(string input)
	{
		BraveInput primaryPlayerInstance = BraveInput.PrimaryPlayerInstance;
		GungeonActions gungeonActions = ((!(primaryPlayerInstance != null)) ? null : primaryPlayerInstance.ActiveActions);
		switch (input)
		{
		case "%META_CURRENCY_SYMBOL":
			return "[sprite \"hbux_text_icon\"]";
		case "%CURRENCY_SYMBOL":
			return "[sprite \"ui_coin\"]";
		case "%KEY_SYMBOL":
			return "[sprite \"ui_key\"]";
		case "%BLANK_SYMBOL":
			return "[sprite \"ui_blank\"]";
		case "%PLAYER_NAME":
			return GetString(GetTalkingPlayerName());
		case "%PLAYER_NICK":
			return GetString(GetTalkingPlayerNick());
		case "%BRACELETRED_ENCNAME":
			return GetItemsString("#BRACELETRED_ENCNAME");
		case "%PLAYER_THIEF":
			return GetString("#THIEF_NAME");
		case "%INSULT":
			return GetString("#INSULT_NAME");
		case "%CONTROL_INTERACT_MAP":
			if (primaryPlayerInstance.IsKeyboardAndMouse())
			{
				return GetBindingText(GungeonActions.GungeonActionType.Interact);
			}
			return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.Action1, BraveInput.PlayerOneCurrentSymbology);
		case "%CONTROL_INTERACT":
			if (primaryPlayerInstance.IsKeyboardAndMouse())
			{
				return GetBindingText(GungeonActions.GungeonActionType.Interact);
			}
			if (gungeonActions != null && gungeonActions.InteractAction.Bindings.Count > 0)
			{
				DeviceBindingSource deviceBindingSource2 = gungeonActions.InteractAction.Bindings[0] as DeviceBindingSource;
				if (deviceBindingSource2 != null && deviceBindingSource2.Control != 0)
				{
					return UIControllerButtonHelper.GetUnifiedControllerButtonTag(deviceBindingSource2.Control, BraveInput.PlayerOneCurrentSymbology);
				}
			}
			return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.Action1, BraveInput.PlayerOneCurrentSymbology);
		case "%CONTROL_DODGEROLL":
			if (primaryPlayerInstance.IsKeyboardAndMouse())
			{
				return GetBindingText(GungeonActions.GungeonActionType.DodgeRoll);
			}
			if (gungeonActions != null && gungeonActions.DodgeRollAction.Bindings.Count > 0)
			{
				DeviceBindingSource deviceBindingSource3 = gungeonActions.DodgeRollAction.Bindings[0] as DeviceBindingSource;
				if (deviceBindingSource3 != null && deviceBindingSource3.Control != 0)
				{
					return UIControllerButtonHelper.GetUnifiedControllerButtonTag(deviceBindingSource3.Control, BraveInput.PlayerOneCurrentSymbology);
				}
			}
			if (GameManager.Options.CurrentControlPreset == GameOptions.ControlPreset.FLIPPED_TRIGGERS)
			{
				return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.LeftTrigger, BraveInput.PlayerOneCurrentSymbology);
			}
			return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.LeftBumper, BraveInput.PlayerOneCurrentSymbology);
		case "%CONTROL_PAUSE":
			if (primaryPlayerInstance.IsKeyboardAndMouse())
			{
				return GetBindingText(GungeonActions.GungeonActionType.Pause);
			}
			if (gungeonActions != null && gungeonActions.PauseAction.Bindings.Count > 0)
			{
				DeviceBindingSource deviceBindingSource7 = gungeonActions.PauseAction.Bindings[0] as DeviceBindingSource;
				if (deviceBindingSource7 != null && deviceBindingSource7.Control != 0)
				{
					return UIControllerButtonHelper.GetUnifiedControllerButtonTag(deviceBindingSource7.Control, BraveInput.PlayerOneCurrentSymbology);
				}
			}
			return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.Start, BraveInput.PlayerOneCurrentSymbology);
		case "%CONTROL_USEITEM":
			if (primaryPlayerInstance.IsKeyboardAndMouse())
			{
				return GetBindingText(GungeonActions.GungeonActionType.UseItem);
			}
			if (gungeonActions != null && gungeonActions.UseItemAction.Bindings.Count > 0)
			{
				DeviceBindingSource deviceBindingSource9 = gungeonActions.UseItemAction.Bindings[0] as DeviceBindingSource;
				if (deviceBindingSource9 != null && deviceBindingSource9.Control != 0)
				{
					return UIControllerButtonHelper.GetUnifiedControllerButtonTag(deviceBindingSource9.Control, BraveInput.PlayerOneCurrentSymbology);
				}
			}
			if (GameManager.Options.CurrentControlPreset == GameOptions.ControlPreset.FLIPPED_TRIGGERS)
			{
				return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.RightBumper, BraveInput.PlayerOneCurrentSymbology);
			}
			return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.RightTrigger, BraveInput.PlayerOneCurrentSymbology);
		case "%CONTROL_USEBLANK":
			if (gungeonActions != null && gungeonActions.BlankAction.Bindings.Count > 0)
			{
				DeviceBindingSource deviceBindingSource6 = gungeonActions.BlankAction.Bindings[0] as DeviceBindingSource;
				if (deviceBindingSource6 != null && deviceBindingSource6.Control != 0)
				{
					return UIControllerButtonHelper.GetUnifiedControllerButtonTag(deviceBindingSource6.Control, BraveInput.PlayerOneCurrentSymbology);
				}
			}
			return GetBindingText(GungeonActions.GungeonActionType.Blank);
		case "%CONTROL_R_STICK_DOWN":
			if (BraveInput.PlayerOneCurrentSymbology == GameOptions.ControllerSymbology.Xbox)
			{
				return "[sprite \"xbone_RS\"]";
			}
			if (BraveInput.PlayerOneCurrentSymbology == GameOptions.ControllerSymbology.Switch)
			{
				return "[sprite \"switch_r3\"]";
			}
			return "[sprite \"ps4_R3\"]";
		case "%CONTROL_L_STICK_DOWN":
			if (BraveInput.PlayerOneCurrentSymbology == GameOptions.ControllerSymbology.Xbox)
			{
				return "[sprite \"xbone_LS\"]";
			}
			if (BraveInput.PlayerOneCurrentSymbology == GameOptions.ControllerSymbology.Switch)
			{
				return "[sprite \"switch_l3\"]";
			}
			return "[sprite \"ps4_L3\"]";
		case "%CONTROL_ALT_DODGEROLL":
			if (primaryPlayerInstance.IsKeyboardAndMouse())
			{
				return "Circle";
			}
			return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.Action2, BraveInput.PlayerOneCurrentSymbology);
		case "%CONTROL_AIM":
			if (primaryPlayerInstance.IsKeyboardAndMouse())
			{
				return "Mouse";
			}
			return UIControllerButtonHelper.GetUnifiedControllerButtonTag("RightStick", BraveInput.PlayerOneCurrentSymbology);
		case "%SYMBOL_TELEPORTER":
			return UIControllerButtonHelper.GetUnifiedControllerButtonTag("Teleporter", BraveInput.PlayerOneCurrentSymbology);
		case "%CONTROL_FIRE":
			if (primaryPlayerInstance.IsKeyboardAndMouse())
			{
				return GetBindingText(GungeonActions.GungeonActionType.Shoot);
			}
			if (gungeonActions != null && gungeonActions.ShootAction.Bindings.Count > 0)
			{
				DeviceBindingSource deviceBindingSource4 = gungeonActions.ShootAction.Bindings[0] as DeviceBindingSource;
				if (deviceBindingSource4 != null && deviceBindingSource4.Control != 0)
				{
					return UIControllerButtonHelper.GetUnifiedControllerButtonTag(deviceBindingSource4.Control, BraveInput.PlayerOneCurrentSymbology);
				}
			}
			if (GameManager.Options.CurrentControlPreset == GameOptions.ControlPreset.FLIPPED_TRIGGERS)
			{
				return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.RightTrigger, BraveInput.PlayerOneCurrentSymbology);
			}
			return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.RightBumper, BraveInput.PlayerOneCurrentSymbology);
		case "%CONTROL_MAP":
			if (primaryPlayerInstance.IsKeyboardAndMouse())
			{
				return GetBindingText(GungeonActions.GungeonActionType.Map);
			}
			if (gungeonActions != null && gungeonActions.MapAction.Bindings.Count > 0)
			{
				DeviceBindingSource deviceBindingSource8 = gungeonActions.MapAction.Bindings[0] as DeviceBindingSource;
				if (deviceBindingSource8 != null && deviceBindingSource8.Control != 0)
				{
					return UIControllerButtonHelper.GetUnifiedControllerButtonTag(deviceBindingSource8.Control, BraveInput.PlayerOneCurrentSymbology);
				}
			}
			if (GameManager.Options.CurrentControlPreset == GameOptions.ControlPreset.FLIPPED_TRIGGERS)
			{
				return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.LeftBumper, BraveInput.PlayerOneCurrentSymbology);
			}
			return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.LeftTrigger, BraveInput.PlayerOneCurrentSymbology);
		case "%CONTROL_RELOAD":
			if (primaryPlayerInstance.IsKeyboardAndMouse())
			{
				return GetBindingText(GungeonActions.GungeonActionType.Reload);
			}
			if (gungeonActions != null && gungeonActions.ReloadAction.Bindings.Count > 0)
			{
				DeviceBindingSource deviceBindingSource5 = gungeonActions.ReloadAction.Bindings[0] as DeviceBindingSource;
				if (deviceBindingSource5 != null && deviceBindingSource5.Control != 0)
				{
					return UIControllerButtonHelper.GetUnifiedControllerButtonTag(deviceBindingSource5.Control, BraveInput.PlayerOneCurrentSymbology);
				}
			}
			return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.Action3, BraveInput.PlayerOneCurrentSymbology);
		case "%CONTROL_QUICKSWITCHGUN":
			if (primaryPlayerInstance.IsKeyboardAndMouse())
			{
				return GetBindingText(GungeonActions.GungeonActionType.GunQuickEquip);
			}
			if (gungeonActions != null && gungeonActions.GunQuickEquipAction.Bindings.Count > 0)
			{
				DeviceBindingSource deviceBindingSource = gungeonActions.GunQuickEquipAction.Bindings[0] as DeviceBindingSource;
				if (deviceBindingSource != null && deviceBindingSource.Control != 0)
				{
					return UIControllerButtonHelper.GetUnifiedControllerButtonTag(deviceBindingSource.Control, BraveInput.PlayerOneCurrentSymbology);
				}
			}
			return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.Action4, BraveInput.PlayerOneCurrentSymbology);
		case "%CONTROL_SWITCHGUN_ALT":
			if (primaryPlayerInstance.IsKeyboardAndMouse())
			{
				return GetBindingText(GungeonActions.GungeonActionType.DropGun);
			}
			return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.DPadDown, BraveInput.PlayerOneCurrentSymbology);
		case "%CONTROL_CIRCLE":
			return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.Action2, BraveInput.PlayerOneCurrentSymbology);
		case "%CONTROL_L1":
			return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.LeftBumper, BraveInput.PlayerOneCurrentSymbology);
		case "%CONTROL_L2":
			return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.LeftTrigger, BraveInput.PlayerOneCurrentSymbology);
		case "%CONTROL_ALL_FACE_BUTTONS":
			return "[sprite \"switch_single_all\"]";
		case "%ESCAPE_ROPE_SYMBOL":
			return "[sprite \"escape_rope_text_icon_001\"]";
		case "%ARMOR_SYMBOL":
			return "[sprite \"armor_money_icon_001\"]";
		case "%CHAMBER1_MASTERY_TOKEN_SYMBOL":
			return "[sprite \"master_token_icon_001\"]";
		case "%CHAMBER2_MASTERY_TOKEN_SYMBOL":
			return "[sprite \"master_token_icon_002\"]";
		case "%CHAMBER3_MASTERY_TOKEN_SYMBOL":
			return "[sprite \"master_token_icon_003\"]";
		case "%CHAMBER4_MASTERY_TOKEN_SYMBOL":
			return "[sprite \"master_token_icon_004\"]";
		case "%CHAMBER5_MASTERY_TOKEN_SYMBOL":
			return "[sprite \"master_token_icon_005\"]";
		case "%HEART_SYMBOL":
			return "[sprite \"heart_big_idle_001\"]";
		case "%JUNK_SYMBOL":
			return "[sprite \"poopsack_001\"]";
		case "%BTCKTP_PRIMER":
			return "[sprite \"forged_bullet_primer_001\"]";
		case "%BTCKTP_POWDER":
			return "[sprite \"forged_bullet_powder_001\"]";
		case "%BTCKTP_SLUG":
			return "[sprite \"forged_bullet_slug_001\"]";
		case "%BTCKTP_CASING":
			return "[sprite \"forged_bullet_case_001\"]";
		case "%PLAYTIMEHOURS":
			return string.Format("{0:0.0}", GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.TIME_PLAYED) / 3600f);
		default:
			return input;
		}
	}

	private static bool CharIsEnglishAlphaNumeric(char c)
	{
		return char.IsLetterOrDigit(c) && c < '׀';
	}

	public static string PostprocessString(string input)
	{
		if (m_postprocessors == null)
		{
			m_postprocessors = new Stack<StringBuilder>();
		}
		if (m_tokenLists == null)
		{
			m_tokenLists = new Stack<List<string>>();
		}
		StringBuilder stringBuilder = ((m_postprocessors.Count <= 0) ? new StringBuilder() : m_postprocessors.Pop());
		List<string> list = ((m_tokenLists.Count <= 0) ? new List<string>() : m_tokenLists.Pop());
		int num = 0;
		for (int i = 0; i < input.Length; i++)
		{
			char c = input[i];
			if (!CharIsEnglishAlphaNumeric(c) && c != '_')
			{
				list.Add(input.Substring(num, i - num));
				num = i;
			}
		}
		list.Add(input.Substring(num, input.Length - num));
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j] == null || list[j].Length == 0)
			{
				continue;
			}
			if (list[j][0] == '%')
			{
				bool flag = false;
				if (j < list.Count - 1 && list[j + 1].Length > 0 && list[j] == "%KEY_SYMBOL" && list[j + 1][0] == '?')
				{
					flag = true;
				}
				string value = EvaluateReplacementToken(list[j]);
				stringBuilder.Append(value);
				if (flag)
				{
					stringBuilder.Append(' ');
				}
			}
			else
			{
				stringBuilder.Append(list[j]);
			}
		}
		string result = stringBuilder.ToString();
		stringBuilder.Length = 0;
		list.Clear();
		m_postprocessors.Push(stringBuilder);
		m_tokenLists.Push(list);
		return result;
	}
}
