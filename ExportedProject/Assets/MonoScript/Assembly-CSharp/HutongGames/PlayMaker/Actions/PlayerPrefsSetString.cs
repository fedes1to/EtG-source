using Brave;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Sets the value of the preference identified by key.")]
	[ActionCategory("PlayerPrefs")]
	public class PlayerPrefsSetString : FsmStateAction
	{
		[CompoundArray("Count", "Key", "Value")]
		[Tooltip("Case sensitive key.")]
		public FsmString[] keys;

		public FsmString[] values;

		public override void Reset()
		{
			keys = new FsmString[1];
			values = new FsmString[1];
		}

		public override void OnEnter()
		{
			for (int i = 0; i < keys.Length; i++)
			{
				if (!keys[i].IsNone || !keys[i].Value.Equals(string.Empty))
				{
					PlayerPrefs.SetString(keys[i].Value, (!values[i].IsNone) ? values[i].Value : string.Empty);
				}
			}
			Finish();
		}
	}
}
