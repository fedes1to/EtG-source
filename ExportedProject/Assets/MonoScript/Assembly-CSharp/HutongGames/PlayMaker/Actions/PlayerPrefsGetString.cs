using Brave;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Returns the value corresponding to key in the preference file if it exists.")]
	[ActionCategory("PlayerPrefs")]
	public class PlayerPrefsGetString : FsmStateAction
	{
		[CompoundArray("Count", "Key", "Variable")]
		[Tooltip("Case sensitive key.")]
		public FsmString[] keys;

		[UIHint(UIHint.Variable)]
		public FsmString[] variables;

		public override void Reset()
		{
			keys = new FsmString[1];
			variables = new FsmString[1];
		}

		public override void OnEnter()
		{
			for (int i = 0; i < keys.Length; i++)
			{
				if (!keys[i].IsNone || !keys[i].Value.Equals(string.Empty))
				{
					variables[i].Value = PlayerPrefs.GetString(keys[i].Value, (!variables[i].IsNone) ? variables[i].Value : string.Empty);
				}
			}
			Finish();
		}
	}
}
