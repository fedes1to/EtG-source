using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class ResourcefulRatIntroDoer : SpecificIntroDoer
{
	private bool m_isFinished;

	public override Vector2? OverrideIntroPosition
	{
		get
		{
			return base.transform.position + new Vector3(2.4375f, 4f);
		}
	}

	public override bool IsIntroFinished
	{
		get
		{
			return m_isFinished && base.IsIntroFinished;
		}
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
		base.StartIntro(animators);
		StartCoroutine(DoIntro());
	}

	public IEnumerator DoIntro()
	{
		TextBoxManager.TIME_INVARIANT = true;
		bool multiline = false;
		string introKey = SelectIntroString(out multiline);
		yield return StartCoroutine(DoRatTalk(introKey, multiline));
		TextBoxManager.TIME_INVARIANT = false;
		m_isFinished = true;
	}

	private string SelectIntroString(out bool multiline)
	{
		multiline = false;
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.BOSSKILLED_RESOURCEFULRAT))
		{
			return "#RATFIGHTINTRO_START_POSTVICTORY";
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.HAS_ATTEMPTED_RESOURCEFUL_RAT))
		{
			if (GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_COMPLETE) && !GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_INTRO_SIX_ALT))
			{
				GameStatsManager.Instance.SetFlag(GungeonFlags.RESOURCEFUL_RAT_INTRO_SIX_ALT, true);
				return "#RATFIGHTINTRO_6_NOTES_ATTEMPT_002";
			}
			return "#RATFIGHTINTRO_REPEATED_ATTEMPTS";
		}
		GameStatsManager.Instance.SetFlag(GungeonFlags.HAS_ATTEMPTED_RESOURCEFUL_RAT, true);
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_06))
		{
			multiline = true;
			return "#RATFIGHTINTRO_6_NOTES_ATTEMPT_001";
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_05))
		{
			return "#RATFIGHTINTRO_5_NOTES";
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_04))
		{
			return "#RATFIGHTINTRO_4_NOTES";
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_01))
		{
			float value = Random.value;
			if (value < 0.33f)
			{
				return "#RATFIGHTINTRO_3_OR_LESS_NOTES_03";
			}
			if (value < 0.66f)
			{
				return "#RATFIGHTINTRO_3_OR_LESS_NOTES_02";
			}
			return "#RATFIGHTINTRO_3_OR_LESS_NOTES_01";
		}
		multiline = true;
		return "#RATFIGHTINTRO_FOUND_ZERO_NOTES";
	}

	public IEnumerator DoRatTalk(string stringKey, bool multiline)
	{
		GetComponent<GenericIntroDoer>().SuppressSkipping = true;
		if (multiline)
		{
			int numLines = StringTableManager.GetNumStrings(stringKey);
			for (int i = 0; i < numLines; i++)
			{
				yield return StartCoroutine(TalkRaw(StringTableManager.GetExactString(stringKey, i)));
			}
		}
		else
		{
			yield return StartCoroutine(TalkRaw(StringTableManager.GetString(stringKey)));
		}
		yield return null;
		GetComponent<GenericIntroDoer>().SuppressSkipping = false;
	}

	private IEnumerator TalkRaw(string plaintext)
	{
		TextBoxManager.ShowTextBox(base.transform.position + new Vector3(2.25f, 7.5f, 0f), base.transform, 5f, plaintext, "ratboss", false);
		bool advancedPressed = false;
		while (!advancedPressed)
		{
			advancedPressed = BraveInput.GetInstanceForPlayer(0).WasAdvanceDialoguePressed() || BraveInput.GetInstanceForPlayer(1).WasAdvanceDialoguePressed();
			yield return null;
		}
		TextBoxManager.ClearTextBox(base.transform);
	}
}
