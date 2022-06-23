using System;

namespace AK.Wwise
{
	[Serializable]
	public class Bank : BaseType
	{
		public string name;

		public void Load(bool decodeBank = false, bool saveDecodedBank = false)
		{
			if (IsValid())
			{
				AkBankManager.LoadBank(name, decodeBank, saveDecodedBank);
			}
		}

		public void LoadAsync(AkCallbackManager.BankCallback callback = null)
		{
			if (IsValid())
			{
				AkBankManager.LoadBankAsync(name, callback);
			}
		}

		public void Unload()
		{
			if (IsValid())
			{
				AkBankManager.UnloadBank(name);
			}
		}

		public override bool IsValid()
		{
			return name.Length != 0 || base.IsValid();
		}
	}
}
