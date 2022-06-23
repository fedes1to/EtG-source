using Brave;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("PlayerPrefs")]
	[Tooltip("Returns the value corresponding to key in the preference file if it exists.")]
	public class PlayerPrefsGetFloat : FsmStateAction
	{
		[Tooltip("Case sensitive key.")]
		[CompoundArray("Count", "Key", "Variable")]
		public FsmString[] keys;

		[UIHint(UIHint.Variable)]
		public FsmFloat[] variables;

		public override void Reset()
		{
			keys = new FsmString[1];
			variables = new FsmFloat[1];
		}

		public override void OnEnter()
		{
			for (int i = 0; i < keys.Length; i++)
			{
				if (!keys[i].IsNone || !keys[i].Value.Equals(string.Empty))
				{
					variables[i].Value = PlayerPrefs.GetFloat(keys[i].Value, (!variables[i].IsNone) ? variables[i].Value : 0f);
				}
			}
			Finish();
		}
	}
}
