using System.Collections;
using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/General/Animate Popup")]
public class AnimatePopup : MonoBehaviour
{
	private const float ANIMATION_LENGTH = 0.15f;

	private dfListbox target;

	private void OnDropdownOpen(dfDropdown dropdown, dfListbox popup)
	{
		if (target != null)
		{
			StopCoroutine("animateOpen");
			StopCoroutine("animateClose");
			Object.Destroy(target.gameObject);
		}
		target = popup;
		StartCoroutine(animateOpen(popup));
	}

	private void OnDropdownClose(dfDropdown dropdown, dfListbox popup)
	{
		StartCoroutine(animateClose(popup));
	}

	private IEnumerator animateOpen(dfListbox popup)
	{
		float runningTime = 0f;
		float startAlpha = 0f;
		float endAlpha = 1f;
		float startHeight = 20f;
		float endHeight = popup.Height;
		while (target == popup && runningTime < 0.15f)
		{
			runningTime = Mathf.Min(runningTime + BraveTime.DeltaTime, 0.15f);
			popup.Opacity = Mathf.Lerp(startAlpha, endAlpha, runningTime / 0.15f);
			float height = (popup.Height = Mathf.Lerp(startHeight, endHeight, runningTime / 0.15f));
			yield return null;
		}
		popup.Opacity = 1f;
		popup.Height = endHeight;
		yield return null;
		popup.Invalidate();
	}

	private IEnumerator animateClose(dfListbox popup)
	{
		float runningTime = 0f;
		float startAlpha = 1f;
		float endAlpha = 0f;
		float startHeight = popup.Height;
		float endHeight = 20f;
		while (target == popup && runningTime < 0.15f)
		{
			runningTime = Mathf.Min(runningTime + BraveTime.DeltaTime, 0.15f);
			popup.Opacity = Mathf.Lerp(startAlpha, endAlpha, runningTime / 0.15f);
			float height = (popup.Height = Mathf.Lerp(startHeight, endHeight, runningTime / 0.15f));
			yield return null;
		}
		target = null;
		Object.Destroy(popup.gameObject);
	}
}
