using System;
using System.Collections;
using UnityEngine;

[Serializable]
[dfTooltip("Downloads an image from a web URL and displays it on-screen like a sprite")]
[ExecuteInEditMode]
[dfCategory("Basic Controls")]
[dfHelp("http://www.daikonforge.com/docs/df-gui/classdf_web_sprite.html")]
[AddComponentMenu("Daikon Forge/User Interface/Sprite/Web")]
public class dfWebSprite : dfTextureSprite
{
	public PropertyChangedEventHandler<Texture> DownloadComplete;

	public PropertyChangedEventHandler<string> DownloadError;

	[SerializeField]
	protected string url = string.Empty;

	[SerializeField]
	protected Texture2D loadingImage;

	[SerializeField]
	protected Texture2D errorImage;

	[SerializeField]
	protected bool autoDownload = true;

	public string URL
	{
		get
		{
			return url;
		}
		set
		{
			if (value != url)
			{
				url = value;
				if (Application.isPlaying && AutoDownload)
				{
					LoadImage();
				}
			}
		}
	}

	public Texture2D LoadingImage
	{
		get
		{
			return loadingImage;
		}
		set
		{
			loadingImage = value;
		}
	}

	public Texture2D ErrorImage
	{
		get
		{
			return errorImage;
		}
		set
		{
			errorImage = value;
		}
	}

	public bool AutoDownload
	{
		get
		{
			return autoDownload;
		}
		set
		{
			autoDownload = value;
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (base.Texture == null)
		{
			base.Texture = LoadingImage;
		}
		if (base.Texture == null && AutoDownload && Application.isPlaying)
		{
			LoadImage();
		}
	}

	public void LoadImage()
	{
		StopAllCoroutines();
		StartCoroutine(downloadTexture());
	}

	private IEnumerator downloadTexture()
	{
		base.Texture = loadingImage;
		if (string.IsNullOrEmpty(url))
		{
			yield break;
		}
		using (WWW request = new WWW(url))
		{
			yield return request;
			if (!string.IsNullOrEmpty(request.error))
			{
				base.Texture = errorImage ?? loadingImage;
				if (DownloadError != null)
				{
					DownloadError(this, request.error);
				}
				Signal("OnDownloadError", this, request.error);
			}
			else
			{
				base.Texture = request.texture;
				if (DownloadComplete != null)
				{
					DownloadComplete(this, base.Texture);
				}
				Signal("OnDownloadComplete", this, base.Texture);
			}
		}
	}
}
