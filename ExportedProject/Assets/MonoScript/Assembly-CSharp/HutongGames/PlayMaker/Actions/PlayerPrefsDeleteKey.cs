using Brave;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("PlayerPrefs")]
	[Tooltip("Removes key and its corresponding value from the preferences.")]
	public class PlayerPrefsDeleteKey : FsmStateAction
	{
		public FsmString key;

		public override void Reset()
		{
			key = string.Empty;
		}

		public override void OnEnter()
		{
			if (!key.IsNone && !key.Value.Equals(string.Empty))
			{
				PlayerPrefs.DeleteKey(key.Value);
			}
			Finish();
		}
	}
}
