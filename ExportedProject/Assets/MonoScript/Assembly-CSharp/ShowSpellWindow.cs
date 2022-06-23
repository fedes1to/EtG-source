using System.Collections;
using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Actionbar/Show Spell Window")]
public class ShowSpellWindow : MonoBehaviour
{
	private bool busy;

	private bool isVisible;

	private void OnEnable()
	{
		dfControl component = GameObject.Find("Spell Window").GetComponent<dfControl>();
		component.IsVisible = false;
	}

	private void OnClick()
	{
		if (!busy)
		{
			StopAllCoroutines();
			dfControl component = GameObject.Find("Spell Window").GetComponent<dfControl>();
			if (!isVisible)
			{
				StartCoroutine(showWindow(component));
			}
			else
			{
				StartCoroutine(hideWindow(component));
			}
		}
	}

	private IEnumerator hideWindow(dfControl window)
	{
		busy = true;
		isVisible = false;
		window.IsVisible = true;
		window.GetManager().BringToFront(window);
		dfAnimatedFloat opacity = new dfAnimatedFloat(1f, 0f, 0.33f);
		while ((float)opacity > 0.05f)
		{
			window.Opacity = opacity;
			yield return null;
		}
		window.Opacity = 0f;
		busy = false;
	}

	private IEnumerator showWindow(dfControl window)
	{
		isVisible = true;
		busy = true;
		window.IsVisible = true;
		window.GetManager().BringToFront(window);
		dfAnimatedFloat opacity = new dfAnimatedFloat(0f, 1f, 0.33f);
		while ((float)opacity < 0.95f)
		{
			window.Opacity = opacity;
			yield return null;
		}
		window.Opacity = 1f;
		busy = false;
		isVisible = true;
	}
}
