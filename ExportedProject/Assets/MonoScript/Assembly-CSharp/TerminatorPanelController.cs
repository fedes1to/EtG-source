using System;
using System.Collections;
using UnityEngine;

public class TerminatorPanelController : MonoBehaviour
{
	public dfLabel leftLabel;

	public dfLabel rightLabel;

	public dfLabel bottomLabel;

	[NonSerialized]
	public bool IsActive;

	public float HangTime = 3f;

	private dfPanel m_panel;

	private void Awake()
	{
		m_panel = GetComponent<dfPanel>();
		m_panel.Opacity = 0f;
	}

	public void Trigger()
	{
		IsActive = true;
		StartCoroutine(HandleTrigger());
	}

	private int GetNumberOfDigitsNotContainingOne(int digits)
	{
		int num = 0;
		for (int i = 0; i < digits; i++)
		{
			int num2 = UnityEngine.Random.Range(2, 10);
			num += num2 * (int)Mathf.Pow(10f, i);
		}
		return num;
	}

	private string GenerateLeftString(string lsb, string lsfb)
	{
		string text = string.Format(lsfb, GetNumberOfDigitsNotContainingOne(6), GetNumberOfDigitsNotContainingOne(3), GetNumberOfDigitsNotContainingOne(2));
		string text2 = string.Format(lsfb, GetNumberOfDigitsNotContainingOne(6), GetNumberOfDigitsNotContainingOne(3), GetNumberOfDigitsNotContainingOne(2));
		string text3 = string.Format(lsfb, GetNumberOfDigitsNotContainingOne(6), GetNumberOfDigitsNotContainingOne(3), GetNumberOfDigitsNotContainingOne(2));
		string text4 = string.Format(lsfb, GetNumberOfDigitsNotContainingOne(6), GetNumberOfDigitsNotContainingOne(3), GetNumberOfDigitsNotContainingOne(2));
		string text5 = string.Format(lsfb, GetNumberOfDigitsNotContainingOne(6), GetNumberOfDigitsNotContainingOne(3), GetNumberOfDigitsNotContainingOne(2));
		string text6 = string.Format(lsfb, GetNumberOfDigitsNotContainingOne(6), GetNumberOfDigitsNotContainingOne(3), GetNumberOfDigitsNotContainingOne(2));
		string text7 = string.Format(lsfb, GetNumberOfDigitsNotContainingOne(6), GetNumberOfDigitsNotContainingOne(3), GetNumberOfDigitsNotContainingOne(2));
		return string.Format(lsb, text, text2, text3, text4, text5, text6, text7);
	}

	private IEnumerator HandleTrigger()
	{
		m_panel.Opacity = 1f;
		float elapsed = 0f;
		float duration = 5f;
		string rightStringBase = "SCAN MODE {0}\nSIZE ASSESSMENT\n\nASSESSMENT COMPLETE\nFIT PROBABILITY 0.99\n\nRESET TO ACQUISITION\nMODE SPEECH LEVEL 76\n\nPRIORITY OVERRIDE\nDEFENSE SYSTEMS SET\nACTIVE STATUS\nLEVEL {1} MAX";
		string leftStringBase = "ANALYSIS:\n******************\n{0}\n{1}\n{2}\n{3}\n\n{4}\n{5}\n{6}";
		string leftStringFragBase = "{0} {1} {2}";
		string rightString2 = string.Format(rightStringBase, 43894, UnityEngine.Random.Range(1000000, 9999999));
		string leftString2 = GenerateLeftString(leftStringBase, leftStringFragBase);
		int frameCount = 0;
		while (elapsed < duration)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			frameCount++;
			leftLabel.Height = 84 + 42 * Mathf.Min(8, Mathf.FloorToInt(elapsed / duration * 10f));
			if (frameCount % 5 == 0)
			{
				rightString2 = string.Format(rightStringBase, 43894, UnityEngine.Random.Range(1000000, 9999999));
				leftString2 = GenerateLeftString(leftStringBase, leftStringFragBase);
				rightLabel.Text = rightString2;
				leftLabel.Text = leftString2;
			}
			yield return null;
		}
		yield return new WaitForSeconds(HangTime);
		base.gameObject.SetActive(false);
		IsActive = false;
	}
}
