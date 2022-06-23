using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using _003CPrivateImplementationDetails_003E_007BDE5600AD_002D6212_002D4D84_002D9A32_002D9D951E3289D1_007D;
using Brave;
using FullInspector;
using UnityEngine;

public static class SaveManager
{
	public class SaveType
	{
		public string legacyFilePattern;

		public string filePattern;

		public bool encrypted;

		public int backupCount;

		public int backupMinTimeMin;

		public string backupPattern;
	}

	public enum SaveSlot
	{
		A,
		B,
		C,
		D
	}

	public static SaveType GameSave = new SaveType
	{
		filePattern = "Slot{0}.save",
		encrypted = true,
		backupCount = 3,
		backupPattern = "Slot{0}.backup.{1}",
		backupMinTimeMin = 45,
		legacyFilePattern = "gameStatsSlot{0}.txt"
	};

	public static SaveType OptionsSave = new SaveType
	{
		filePattern = "Slot{0}.options",
		legacyFilePattern = "optionsSlot{0}.txt"
	};

	public static SaveType MidGameSave = new SaveType
	{
		filePattern = "Active{0}.game",
		legacyFilePattern = "activeSlot{0}.txt",
		encrypted = true,
		backupCount = 0,
		backupPattern = "Active{0}.backup.{1}",
		backupMinTimeMin = 60
	};

	public static SaveSlot CurrentSaveSlot;

	public static SaveSlot? TargetSaveSlot;

	public static bool ResetSaveSlot;

	public static bool PreventMidgameSaveDeletionOnExit;

	private static bool s_hasBeenInitialized;

	public static string OldSavePath = Path.Combine(Application.dataPath, "saves");

	public static string SavePath = Application.persistentDataPath;

	private static string secret = "bruh";
	//{
		/*107, 164, 67, 89, 49, 25, 207, 88, 51, 60,
		248, 156, 50, 78, 62, 211, 54, 174, 103, 13,
		39, 68, 125, 41, 212, 32, 206, 226, 34, 63,
		197, 19, 19, 117, 113, 209, 103, 3, 1, 163,
		61, 192, 126, 0, 244, 203, 3, 4, 11, 108,
		159, 196, 108, 214, 227, 208, 152, 145, 17, 137,
		89, 180, 195, 87, 96, 118, 244, 44, 199, 223,
		239, 184, 22, 82, 128, 135, 64, 240, 94, 185,
		88, 205, 243, 96, 62, 87, 155, 104, 144, 192,
		34, 70, 1, 239, 161, 188, 14, 153, 124, 2,
		246, 184, 50, 132, 244, 9, 206, 79, 200, 158,
		157, 211, 245, 131, 63, 188, 198, 235, 132, 123,
		7, 13, 79, 198, 171, 90, 107, 236, 70, 239,
		119, 197, 158, 76, 83, 10, 84, 218, 232, 25,
		170, 217, 88, 66, 198, 250, 184, 192, 176, 105,
		243, 82, 25, 247, 177, 63, 181, 102, 253, 247,
		214, 105, 219, 211, 176, 131, 156, 84, 224, 32,
		229, 183, 82, 186, 243, 41, 165, 59, 238, 55,
		229, 239, 53, 57, 253, 139, 100, 135, 34, 235,
		11, 133, 93, 172, 63, 83, 0, 152, 227, 53,
		44, 3, 123, 81, 39, 204, 1, 22, 52, 97,
		222, 255, 125, 39, 214, 138, 77, 75, 103, 7,
		156, 155, 67, 97, 184, 169, 80, 31, 69, 109,
		67, 226, 79, 110, 76, 182, 224, 186, 22, 101,
		232, 81, 224, 77, 4, 98, 97, 103, 131, 61,
		71, 4, 99, 206, 0, 14, 95, 73, 235, 147,
		40, 79, 233, 6, 102, 85, 70, 225, 163, 63,
		160, 182, 233, 37, 148, 56, 205, 109, 155, 0,
		10, 243, 34, 10, 12, 97, 103, 208, 119, 134,
		48, 61, 52, 69, 172, 234, 68, 57, 166, 56,
		200, 156, 208, 23, 44, 65, 247, 229, 41, 254,
		213, 44, 138, 242, 224, 126, 192, 90, 108, 194,
		124, 130, 123, 166, 114, 136, 36, 173, 235, 13,
		82, 108, 19, 120, 168, 62, 61, 122, 111, 176,
		173, 186, 40, 90, 80, 74, 253, 219, 206, 156,
		117, 12, 28, 77, 229, 173, 218, 10, 33, 44,
		207, 111, 164, 212, 133, 237, 87, 0, 233, 201,
		143, 214, 221, 233, 86, 153, 49, 64, 151, 69,
		1, 17, 50, 191, 59, 239, 43, 243, 197, 129,
		190, 47, 237, 161, 69, 195, 136, 223, 174, 98,
		171, 255, 75, 174, 101, 177, 69, 71, 115, 63,
		228, 67, 89, 114, 66, 42, 160, 226, 61, 213,
		254, 151, 66, 222, 47, 247, 59, 130, 47, 53,
		101, 12, 140, 207, 11, 150, 172, 9, 147, 162,
		240, 61, 29, 156, 223, 49, 162, 105, 19, 232,
		212, 177, 184, 91, 49, 106, 8, 130, 151, 213,
		81, 23, 154, 45, 8, 252, 212, 186, 70, 94,
		51, 148, 7, 99, 155, 117, 74, 51, 30, 170,
		203, 200, 46, 51, 146, 214, 94, 14, 84, 30,
		89, 23, 193, 141, 63, 13, 162, 19, 27, 199,
		80, 206, 186, 115, 52, 128, 227, 139, 123, 247,
		24, 20*/
		
	//}

	public static void Init()
	{
		if (s_hasBeenInitialized)
		{
			return;
		}
		if (string.IsNullOrEmpty(SavePath))
		{
			Debug.LogError("Application.persistentDataPath FAILED! " + SavePath);
			SavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "../LocalLow/Dodge Roll/Enter the Gungeon");
		}
		if (!Directory.Exists(SavePath))
		{
			try
			{
				Debug.LogWarning("Manually create default save directory!");
				Directory.CreateDirectory(SavePath);
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to create default save directory: " + ex.Message);
			}
		}
		int num = Brave.PlayerPrefs.GetInt("saveslot", 0);
		Brave.PlayerPrefs.SetInt("saveslot", num);
		if (num < 0 || num > 3)
		{
			num = 0;
		}
		CurrentSaveSlot = (SaveSlot)num;
		for (int i = 0; i < 3; i++)
		{
			SaveSlot saveSlot = (SaveSlot)i;
			SafeMove(Path.Combine(OldSavePath, string.Format(GameSave.legacyFilePattern, saveSlot)), Path.Combine(OldSavePath, string.Format(GameSave.filePattern, saveSlot)));
			SafeMove(Path.Combine(OldSavePath, string.Format(OptionsSave.legacyFilePattern, saveSlot)), Path.Combine(OldSavePath, string.Format(OptionsSave.filePattern, saveSlot)));
			SafeMove(Path.Combine(OldSavePath, string.Format(GameSave.filePattern, saveSlot)), Path.Combine(SavePath, string.Format(GameSave.filePattern, saveSlot)));
			SafeMove(Path.Combine(OldSavePath, string.Format(OptionsSave.filePattern, saveSlot)), Path.Combine(SavePath, string.Format(OptionsSave.filePattern, saveSlot)));
			SafeMove(PathCombine(SavePath, "01", string.Format(GameSave.filePattern, saveSlot)), Path.Combine(SavePath, string.Format(GameSave.filePattern, saveSlot)), true);
			SafeMove(PathCombine(SavePath, "01", string.Format(OptionsSave.filePattern, saveSlot)), Path.Combine(SavePath, string.Format(OptionsSave.filePattern, saveSlot)), true);
		}
		s_hasBeenInitialized = true;
	}

	private static string PathCombine(string a, string b, string c)
	{
		return Path.Combine(Path.Combine(a, b), c);
	}

	public static void ChangeSlot(SaveSlot newSaveSlot)
	{
		if (!s_hasBeenInitialized)
		{
			Debug.LogErrorFormat("Tried to change save slots before before SaveManager was initialized! {0}", newSaveSlot);
		}
		CurrentSaveSlot = newSaveSlot;
		Brave.PlayerPrefs.SetInt("saveslot", (int)CurrentSaveSlot);
	}

	public static void DeleteCurrentSlotMidGameSave(SaveSlot? overrideSaveSlot = null)
	{
		Debug.LogError("DELETING CURRENT MID GAME SAVE");
		if (GameStatsManager.HasInstance)
		{
			GameStatsManager.Instance.midGameSaveGuid = null;
		}
		string path = string.Format(MidGameSave.filePattern, (!overrideSaveSlot.HasValue) ? CurrentSaveSlot : overrideSaveSlot.Value);
		string path2 = Path.Combine(SavePath, path);
		if (File.Exists(path2))
		{
			File.Delete(path2);
		}
	}

	public static bool Save<T>(T obj, SaveType saveType, int playTimeMin, uint versionNumber = 0u, SaveSlot? overrideSaveSlot = null)
	{
		bool encrypted = saveType.encrypted;
		if (!s_hasBeenInitialized)
		{
			Debug.LogErrorFormat("Tried to save data before SaveManager was initialized! {0} {1}", obj.GetType(), saveType.filePattern);
			return false;
		}
		string path = string.Format(saveType.filePattern, (!overrideSaveSlot.HasValue) ? CurrentSaveSlot : overrideSaveSlot.Value);
		string text = Path.Combine(SavePath, path);
		string text2;
		try
		{
			bool prettyPrintSerializedJson = fiSettings.PrettyPrintSerializedJson;
			fiSettings.PrettyPrintSerializedJson = !encrypted;
			text2 = SerializationHelpers.SerializeToContent<T, FullSerializerSerializer>(obj);
			fiSettings.PrettyPrintSerializedJson = prettyPrintSerializedJson;
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to serialize save data: " + ex.Message);
			return false;
		}
		if (encrypted)
		{
			text2 = Encrypt(text2);
		}
		text2 = string.Format("version: {0}\n{1}", versionNumber, text2);
		if (!Directory.Exists(SavePath))
		{
			Directory.CreateDirectory(SavePath);
		}
		bool flag = false;
		if (File.Exists(text))
		{
			try
			{
				File.Copy(text, text + ".temp", true);
				flag = true;
			}
			catch (Exception ex2)
			{
				Debug.LogError("Failed to create a temporary copy of current save: " + ex2.Message);
				return false;
			}
		}
		try
		{
			WriteAllText(text, text2);
		}
		catch (Exception ex3)
		{
			Debug.LogError("Failed to write new save data: " + ex3.Message);
			try
			{
				File.Delete(text);
				File.Move(text + ".temp", text);
			}
			catch (Exception ex4)
			{
				Debug.LogError("Failed to restore temp save data: " + ex4.Message);
			}
			return false;
		}
		if (flag)
		{
			try
			{
				if (File.Exists(text + ".temp"))
				{
					File.Delete(text + ".temp");
				}
			}
			catch (Exception ex5)
			{
				Debug.LogError("Failed to replace temp save file: " + ex5.Message);
			}
		}
		if (saveType.backupCount > 0)
		{
			int latestBackupPlaytimeMinutes = GetLatestBackupPlaytimeMinutes(saveType, overrideSaveSlot);
			if (playTimeMin >= latestBackupPlaytimeMinutes + saveType.backupMinTimeMin)
			{
				string arg = string.Format("{0}h{1}m", playTimeMin / 60, playTimeMin % 60);
				string path2 = string.Format(saveType.backupPattern, (!overrideSaveSlot.HasValue) ? CurrentSaveSlot : overrideSaveSlot.Value, arg);
				string path3 = Path.Combine(SavePath, path2);
				try
				{
					WriteAllText(path3, text2);
				}
				catch (Exception ex6)
				{
					Debug.LogError("Failed to create new save backup: " + ex6.Message);
				}
				DeleteOldBackups(saveType, overrideSaveSlot);
			}
		}
		return true;
	}

	public static bool Load<T>(SaveType saveType, out T obj, bool allowDecrypted, uint expectedVersion = 0u, Func<string, uint, string> versionUpdater = null, SaveSlot? overrideSaveSlot = null)
	{
		obj = default(T);
		if (!s_hasBeenInitialized)
		{
			Debug.LogErrorFormat("Tried to load data before SaveManager was initialized! {0} {1}", saveType.filePattern, typeof(T));
			return false;
		}
		string text = string.Format(saveType.filePattern, (!overrideSaveSlot.HasValue) ? CurrentSaveSlot : overrideSaveSlot.Value);
		string text2 = Path.Combine(SavePath, text);
		if (!File.Exists(text2))
		{
			Debug.LogWarning("Save data doesn't exist: " + text);
			return false;
		}
		string text3;
		try
		{
			text3 = ReadAllText(text2);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to read save data: " + ex.Message);
			return false;
		}
		uint num = 0u;
		if (text3.StartsWith("version: "))
		{
			StringReader stringReader = new StringReader(text3);
			string text4 = stringReader.ReadLine();
			uint result;
			if (!uint.TryParse(text4.Substring(9), out result))
			{
				Debug.LogErrorFormat("Failed to read save version number (expected [{0}], got [{1}]", expectedVersion, text4.Substring(9));
				return false;
			}
			num = result;
			text3 = stringReader.ReadToEnd();
		}
		if (IsDataEncrypted(text3))
		{
			text3 = Decrypt(text3);
		}
		else if (!allowDecrypted)
		{
			Debug.LogError("Save file corrupted!  Copying to a new file");
			text3 = string.Format("version: {0}\n{1}", num, text3);
			try
			{
				WriteAllText(text2 + ".corrupt", text3);
			}
			catch (Exception ex2)
			{
				Debug.LogError("Failed to save off the corrupted file: " + ex2.Message);
			}
			return false;
		}
		if (num < expectedVersion && versionUpdater != null)
		{
			text3 = versionUpdater(text3, num);
		}
		obj = SerializationHelpers.DeserializeFromContent<T, FullSerializerSerializer>(text3);
		if (obj == null)
		{
			Debug.LogError("Save file corrupted!  Copying to a new file");
			try
			{
				text3 = ReadAllText(text2);
			}
			catch (Exception ex3)
			{
				Debug.LogError("Failed to read corrupted save data: " + ex3.Message);
			}
			try
			{
				WriteAllText(text2 + ".corrupt", text3);
			}
			catch (Exception ex4)
			{
				Debug.LogError("Failed to save off the corrupted file: " + ex4.Message);
			}
			return false;
		}
		return true;
	}

	public static void WriteAllText(string path, string contents)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(contents);
		string text = "null";
		try
		{
			text = Path.GetTempFileName();
			if (Directory.Exists(Path.GetDirectoryName(text)))
			{
				using (FileStream fileStream = File.Create(text, 4096, FileOptions.WriteThrough))
				{
					fileStream.Write(bytes, 0, bytes.Length);
				}
				File.Delete(path);
				File.Move(text, path);
				return;
			}
		}
		catch (Exception ex)
		{
			Debug.LogErrorFormat("Failed to write to temp file {0}: {1}", text, ex);
		}
		text = Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(text));
		using (FileStream fileStream2 = File.Create(text, 4096, FileOptions.WriteThrough))
		{
			fileStream2.Write(bytes, 0, bytes.Length);
		}
		File.Delete(path);
		File.Move(text, path);
	}

	public static string ReadAllText(string path)
	{
		return File.ReadAllText(path, Encoding.UTF8);
	}

	private static int GetLatestBackupPlaytimeMinutes(SaveType saveType, SaveSlot? overrideSaveSlot = null)
	{
		string text = string.Format(saveType.backupPattern, (!overrideSaveSlot.HasValue) ? CurrentSaveSlot : overrideSaveSlot.Value, string.Empty);
		string pattern = text + "(?<hour>\\d+)h(?<min>\\d+)m";
		string[] files = Directory.GetFiles(SavePath);
		int num = 0;
		for (int i = 0; i < files.Length; i++)
		{
			Match match = Regex.Match(files[i], pattern, RegexOptions.Multiline);
			if (match.Success)
			{
				int num2 = Convert.ToInt32(match.Groups["hour"].Captures[0].Value) * 60;
				num2 += Convert.ToInt32(match.Groups["min"].Captures[0].Value);
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	private static void SafeMove(string oldPath, string newPath, bool allowOverwritting = false)
	{
		if (File.Exists(oldPath) && (allowOverwritting || !File.Exists(newPath)))
		{
			string contents = ReadAllText(oldPath);
			try
			{
				WriteAllText(newPath, contents);
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("Failed to move {0} to {1}: {2}", oldPath, newPath, ex);
				return;
			}
			try
			{
				File.Delete(oldPath);
			}
			catch (Exception ex2)
			{
				Debug.LogErrorFormat("Failed to delete old file {0}: {1}", oldPath, newPath, ex2);
				return;
			}
			if (File.Exists(oldPath + ".bak"))
			{
				File.Delete(oldPath + ".bak");
			}
		}
	}

	public static void DeleteAllBackups(SaveType saveType, SaveSlot? overrideSaveSlot = null)
	{
		string text = string.Format(saveType.backupPattern, (!overrideSaveSlot.HasValue) ? CurrentSaveSlot : overrideSaveSlot.Value, string.Empty);
		string pattern = text + "(?<hour>\\d+)h(?<min>\\d+)m";
		string[] files = Directory.GetFiles(SavePath);
		for (int i = 0; i < files.Length; i++)
		{
			Match match = Regex.Match(files[i], pattern, RegexOptions.Multiline);
			if (match.Success)
			{
				try
				{
					File.Delete(files[i]);
				}
				catch (Exception ex)
				{
					Debug.LogError("Failed to remove backup file: " + ex.Message);
					break;
				}
			}
		}
	}

	private static void SafeMoveBackups(SaveType saveType, string oldPath, string newPath, SaveSlot? overrideSaveSlot = null)
	{
		string text = string.Format(saveType.backupPattern, (!overrideSaveSlot.HasValue) ? CurrentSaveSlot : overrideSaveSlot.Value, string.Empty);
		string pattern = text + "(?<hour>\\d+)h(?<min>\\d+)m";
		string[] files = Directory.GetFiles(oldPath);
		for (int i = 0; i < files.Length; i++)
		{
			Match match = Regex.Match(files[i], pattern, RegexOptions.Multiline);
			if (match.Success)
			{
				SafeMove(files[i], Path.Combine(newPath, Path.GetFileName(files[i])));
			}
		}
	}

	private static void DeleteOldBackups(SaveType saveType, SaveSlot? overrideSaveSlot = null)
	{
		string text = string.Format(saveType.backupPattern, (!overrideSaveSlot.HasValue) ? CurrentSaveSlot : overrideSaveSlot.Value, string.Empty);
		string pattern = text + "(?<hour>\\d+)h(?<min>\\d+)m";
		List<Tuple<string, int>> list = new List<Tuple<string, int>>();
		string[] files = Directory.GetFiles(SavePath);
		for (int i = 0; i < files.Length; i++)
		{
			Match match = Regex.Match(files[i], pattern, RegexOptions.Multiline);
			if (match.Success)
			{
				int num = Convert.ToInt32(match.Groups["hour"].Captures[0].Value) * 60;
				num += Convert.ToInt32(match.Groups["min"].Captures[0].Value);
				list.Add(Tuple.Create(files[i], num));
			}
		}
		list.Sort((Tuple<string, int> a, Tuple<string, int> b) => b.Second - a.Second);
		while (list.Count > saveType.backupCount && list.Count > 0)
		{
			try
			{
				File.Delete(list[list.Count - 1].First);
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to remove backup file: " + ex.Message);
				break;
			}
			list.RemoveAt(list.Count - 1);
		}
	}

	private static string Encrypt(string plaintext)
	{
		FixSecret();
		StringBuilder stringBuilder = new StringBuilder(plaintext.Length);
		for (int i = 0; i < plaintext.Length; i++)
		{
			stringBuilder.Append((char)(plaintext[i] ^ secret[i % secret.Length]));
		}
		return stringBuilder.ToString();
	}

	private static string Decrypt(string cypertext)
	{
		FixSecret();
		return Encrypt(cypertext);
	}

	private static bool IsDataEncrypted(string data)
	{
		FixSecret();
		if (data.StartsWith('{'.ToString()))
		{
			return false;
		}
		if (data.StartsWith(((char)(0x7Bu ^ secret[0])).ToString()))
		{
			return true;
		}
		return false;
	}

	private static void FixSecret()
	{
		if (secret.StartsWith("å") && secret.EndsWith("å"))
		{
			secret = secret.Substring(1, secret.Length - 2);
		}
	}
}
