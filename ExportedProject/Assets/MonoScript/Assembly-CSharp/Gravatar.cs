using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/Examples/Game Menu/Gravatar")]
public class Gravatar : MonoBehaviour
{
	private static Regex validator = new Regex("^[a-zA-Z][\\w\\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\\w\\.-]*[a-zA-Z0-9]\\.[a-zA-Z][a-zA-Z\\.]*[a-zA-Z]$", RegexOptions.IgnoreCase);

	public dfWebSprite Sprite;

	[SerializeField]
	protected string email = string.Empty;

	public string EmailAddress
	{
		get
		{
			return email;
		}
		set
		{
			if (value != email)
			{
				email = value;
				updateImage();
			}
		}
	}

	private void OnEnable()
	{
		if (validator.IsMatch(email) && Sprite != null)
		{
			updateImage();
		}
	}

	private void updateImage()
	{
		if (!(Sprite == null))
		{
			if (validator.IsMatch(email))
			{
				string arg = MD5(email.Trim().ToLower());
				Sprite.URL = string.Format("http://www.gravatar.com/avatar/{0}", arg);
			}
			else
			{
				Sprite.Texture = Sprite.LoadingImage;
			}
		}
	}

	public string MD5(string strToEncrypt)
	{
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		byte[] bytes = uTF8Encoding.GetBytes(strToEncrypt);
		MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
		byte[] array = mD5CryptoServiceProvider.ComputeHash(bytes);
		string text = string.Empty;
		for (int i = 0; i < array.Length; i++)
		{
			text += Convert.ToString(array[i], 16).PadLeft(2, '0');
		}
		return text.PadLeft(32, '0');
	}
}
