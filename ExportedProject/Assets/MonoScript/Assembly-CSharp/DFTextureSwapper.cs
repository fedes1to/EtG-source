using UnityEngine;

public class DFTextureSwapper : MonoBehaviour
{
	public string EnglishSpriteName;

	public string OtherSpriteName;

	private void Start()
	{
		dfControl component = GetComponent<dfControl>();
		if ((bool)component)
		{
			component.IsVisibleChanged += HandleVisibilityChanged;
			if (component.IsVisible)
			{
				HandleVisibilityChanged(component, true);
			}
		}
	}

	private void HandleVisibilityChanged(dfControl control, bool value)
	{
		if (control is dfSlicedSprite)
		{
			dfSlicedSprite dfSlicedSprite2 = control as dfSlicedSprite;
			dfSlicedSprite2.SpriteName = ((GameManager.Options.CurrentLanguage != 0) ? OtherSpriteName : EnglishSpriteName);
		}
		else if (control is dfSprite)
		{
			dfSprite dfSprite2 = control as dfSprite;
			dfSprite2.SpriteName = ((GameManager.Options.CurrentLanguage != 0) ? OtherSpriteName : EnglishSpriteName);
		}
	}
}
