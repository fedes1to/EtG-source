using System.Collections;
using UnityEngine;

public class DEMO_PictureSelector : MonoBehaviour
{
	public dfTextureSprite DisplayImage;

	protected dfTextureSprite myImage;

	public void OnEnable()
	{
		myImage = GetComponent<dfTextureSprite>();
	}

	public IEnumerator OnDoubleTapGesture()
	{
		if (DisplayImage == null)
		{
			Debug.LogWarning("The DisplayImage property is not configured, cannot select the image");
			yield break;
		}
		dfTextureSprite photo = Object.Instantiate(DisplayImage.gameObject).GetComponent<dfTextureSprite>();
		myImage.GetManager().AddControl(photo);
		photo.Texture = myImage.Texture;
		photo.Size = myImage.Size;
		photo.RelativePosition = myImage.GetAbsolutePosition();
		photo.transform.rotation = Quaternion.identity;
		photo.BringToFront();
		photo.Opacity = 1f;
		photo.IsVisible = true;
		Vector2 screenSize = myImage.GetManager().GetScreenSize();
		Vector2 fullSize = new Vector2(photo.Texture.width, photo.Texture.height);
		Vector2 displaySize = fitImage(screenSize.x * 0.75f, screenSize.y * 0.75f, fullSize.x, fullSize.y);
		dfAnimatedVector3 animatedPosition = new dfAnimatedVector3(EndValue: new Vector3((screenSize.x - displaySize.x) * 0.5f, (screenSize.y - displaySize.y) * 0.5f), StartValue: myImage.GetAbsolutePosition(), Time: 0.2f);
		dfAnimatedVector2 animatedSize = new dfAnimatedVector2(myImage.Size, displaySize, 0.2f);
		while (!animatedPosition.IsDone || !animatedSize.IsDone)
		{
			photo.Size = animatedSize;
			photo.RelativePosition = animatedPosition;
			yield return null;
		}
	}

	private static Vector2 fitImage(float maxWidth, float maxHeight, float imageWidth, float imageHeight)
	{
		float a = maxWidth / imageWidth;
		float b = maxHeight / imageHeight;
		float num = Mathf.Min(a, b);
		return new Vector2(Mathf.Floor(imageWidth * num), Mathf.Ceil(imageHeight * num));
	}
}
