using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public static class AkBankManager
{
	private class BankHandle
	{
		protected readonly string bankName;

		protected uint m_BankID;

		public int RefCount { get; private set; }

		public BankHandle(string name)
		{
			bankName = name;
		}

		public virtual AKRESULT DoLoadBank()
		{
			return AkSoundEngine.LoadBank(bankName, -1, out m_BankID);
		}

		public void LoadBank()
		{
			if (RefCount == 0)
			{
				if (BanksToUnload.Contains(this))
				{
					BanksToUnload.Remove(this);
				}
				else
				{
					AKRESULT result = DoLoadBank();
					LogLoadResult(result);
				}
			}
			IncRef();
		}

		public virtual void UnloadBank()
		{
			AkSoundEngine.UnloadBank(m_BankID, IntPtr.Zero, null, null);
		}

		public void IncRef()
		{
			RefCount++;
		}

		public void DecRef()
		{
			RefCount--;
			if (RefCount == 0)
			{
				BanksToUnload.Add(this);
			}
		}

		protected void LogLoadResult(AKRESULT result)
		{
			if (result != AKRESULT.AK_Success && AkSoundEngine.IsInitialized())
			{
				Debug.LogWarning(string.Concat("WwiseUnity: Bank ", bankName, " failed to load (", result, ")"));
			}
		}
	}

	private class AsyncBankHandle : BankHandle
	{
		private readonly AkCallbackManager.BankCallback bankCallback;

		public AsyncBankHandle(string name, AkCallbackManager.BankCallback callback)
			: base(name)
		{
			bankCallback = callback;
		}

		private static void GlobalBankCallback(uint in_bankID, IntPtr in_pInMemoryBankPtr, AKRESULT in_eLoadResult, uint in_memPoolId, object in_Cookie)
		{
			m_Mutex.WaitOne();
			AsyncBankHandle asyncBankHandle = (AsyncBankHandle)in_Cookie;
			AkCallbackManager.BankCallback bankCallback = asyncBankHandle.bankCallback;
			if (in_eLoadResult != AKRESULT.AK_Success)
			{
				asyncBankHandle.LogLoadResult(in_eLoadResult);
				m_BankHandles.Remove(asyncBankHandle.bankName);
			}
			m_Mutex.ReleaseMutex();
			if (bankCallback != null)
			{
				bankCallback(in_bankID, in_pInMemoryBankPtr, in_eLoadResult, in_memPoolId, null);
			}
		}

		public override AKRESULT DoLoadBank()
		{
			return AkSoundEngine.LoadBank(bankName, GlobalBankCallback, this, -1, out m_BankID);
		}
	}

	private class DecodableBankHandle : BankHandle
	{
		private readonly bool decodeBank = true;

		private readonly string decodedBankPath;

		private readonly bool saveDecodedBank;

		public DecodableBankHandle(string name, bool save)
			: base(name)
		{
			saveDecodedBank = save;
			string path = bankName + ".bnk";
			string currentLanguage = AkInitializer.GetCurrentLanguage();
			decodedBankPath = Path.Combine(AkSoundEngineController.GetDecodedBankFullPath(), currentLanguage);
			string path2 = Path.Combine(decodedBankPath, path);
			bool flag = File.Exists(path2);
			if (!flag)
			{
				decodedBankPath = AkSoundEngineController.GetDecodedBankFullPath();
				path2 = Path.Combine(decodedBankPath, path);
				flag = File.Exists(path2);
			}
			if (flag)
			{
				try
				{
					DateTime lastWriteTime = File.GetLastWriteTime(path2);
					string soundbankBasePath = AkBasePathGetter.GetSoundbankBasePath();
					string path3 = Path.Combine(soundbankBasePath, path);
					DateTime lastWriteTime2 = File.GetLastWriteTime(path3);
					decodeBank = lastWriteTime <= lastWriteTime2;
				}
				catch
				{
				}
			}
		}

		public override AKRESULT DoLoadBank()
		{
			if (decodeBank)
			{
				return AkSoundEngine.LoadAndDecodeBank(bankName, saveDecodedBank, out m_BankID);
			}
			AKRESULT aKRESULT = AKRESULT.AK_Success;
			if (!string.IsNullOrEmpty(decodedBankPath))
			{
				aKRESULT = AkSoundEngine.SetBasePath(decodedBankPath);
			}
			if (aKRESULT == AKRESULT.AK_Success)
			{
				aKRESULT = AkSoundEngine.LoadBank(bankName, -1, out m_BankID);
				if (!string.IsNullOrEmpty(decodedBankPath))
				{
					AkSoundEngine.SetBasePath(AkBasePathGetter.GetSoundbankBasePath());
				}
			}
			return aKRESULT;
		}

		public override void UnloadBank()
		{
			if (decodeBank && !saveDecodedBank)
			{
				AkSoundEngine.PrepareBank(AkPreparationType.Preparation_Unload, m_BankID);
			}
			else
			{
				base.UnloadBank();
			}
		}
	}

	private static readonly Dictionary<string, BankHandle> m_BankHandles = new Dictionary<string, BankHandle>();

	private static readonly List<BankHandle> BanksToUnload = new List<BankHandle>();

	private static readonly Mutex m_Mutex = new Mutex();

	internal static void DoUnloadBanks()
	{
		int count = BanksToUnload.Count;
		for (int i = 0; i < count; i++)
		{
			BanksToUnload[i].UnloadBank();
		}
		BanksToUnload.Clear();
	}

	internal static void Reset()
	{
		m_BankHandles.Clear();
		BanksToUnload.Clear();
	}

	public static void LoadBank(string name, bool decodeBank, bool saveDecodedBank)
	{
		m_Mutex.WaitOne();
		BankHandle value = null;
		if (!m_BankHandles.TryGetValue(name, out value))
		{
			value = ((!decodeBank) ? new BankHandle(name) : new DecodableBankHandle(name, saveDecodedBank));
			m_BankHandles.Add(name, value);
			m_Mutex.ReleaseMutex();
			value.LoadBank();
		}
		else
		{
			value.IncRef();
			m_Mutex.ReleaseMutex();
		}
	}

	public static void LoadBankAsync(string name, AkCallbackManager.BankCallback callback = null)
	{
		m_Mutex.WaitOne();
		BankHandle value = null;
		if (!m_BankHandles.TryGetValue(name, out value))
		{
			AsyncBankHandle asyncBankHandle = new AsyncBankHandle(name, callback);
			m_BankHandles.Add(name, asyncBankHandle);
			m_Mutex.ReleaseMutex();
			asyncBankHandle.LoadBank();
		}
		else
		{
			value.IncRef();
			m_Mutex.ReleaseMutex();
		}
	}

	public static void UnloadBank(string name)
	{
		m_Mutex.WaitOne();
		BankHandle value = null;
		if (m_BankHandles.TryGetValue(name, out value))
		{
			value.DecRef();
			if (value.RefCount == 0)
			{
				m_BankHandles.Remove(name);
			}
		}
		m_Mutex.ReleaseMutex();
	}
}
