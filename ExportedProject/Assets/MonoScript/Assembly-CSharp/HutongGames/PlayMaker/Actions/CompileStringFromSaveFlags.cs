using System.Collections.Generic;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Sets the value of a String Variable, based upon GungeonFlags.")]
	[ActionCategory(ActionCategory.String)]
	public class CompileStringFromSaveFlags : FsmStateAction
	{
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmString stringVariable;

		public FsmString[] stringComponents;

		public GungeonFlags[] flagComponents;

		public FsmBool[] valueComponents;

		public override void Reset()
		{
			stringVariable = null;
			stringComponents = new FsmString[0];
			flagComponents = new GungeonFlags[0];
			valueComponents = new FsmBool[0];
		}

		public override void OnEnter()
		{
			DoSetStringValue();
			Finish();
		}

		private void DoSetStringValue()
		{
			if (stringVariable == null)
			{
				return;
			}
			List<string> list = new List<string>();
			for (int i = 0; i < stringComponents.Length; i++)
			{
				if (flagComponents[i] == GungeonFlags.NONE || GameStatsManager.Instance.GetFlag(flagComponents[i]) == valueComponents[i].Value)
				{
					list.Add(StringTableManager.GetString(stringComponents[i].Value));
				}
			}
			string text = string.Empty;
			if (list.Count > 0)
			{
				text += list[0];
				char[] array = text.ToCharArray();
				array[0] = char.ToUpper(array[0]);
				text = new string(array);
				for (int j = 1; j < list.Count; j++)
				{
					if (list.Count == 2)
					{
						string text2 = text;
						text = text2 + " " + StringTableManager.GetString("#AND") + " " + list[j];
					}
					else if (j == list.Count - 1)
					{
						string text2 = text;
						text = text2 + ", " + StringTableManager.GetString("#AND") + " " + list[j];
					}
					else
					{
						text = text + ", " + list[j];
					}
				}
				text += ".";
			}
			stringVariable.Value = text;
		}
	}
}
