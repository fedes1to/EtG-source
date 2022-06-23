using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Sonic Ether/SE Natural Bloom and Dirty Lens")]
public class SENaturalBloomAndDirtyLens : MonoBehaviour
{
	[Range(0f, 0.4f)]
	public float bloomIntensity = 0.05f;

	public Shader shader;

	private Material material;

	public Texture2D lensDirtTexture;

	[Range(0f, 0.95f)]
	public float lensDirtIntensity = 0.05f;

	private bool isSupported;

	private float blurSize = 4f;

	public bool inputIsHDR;

	[HideInInspector]
	public bool overrideDisable;

	protected int IterationCount
	{
		get
		{
			if (!Application.isPlaying)
			{
				return 1;
			}
			if (GameManager.Options != null && GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.HIGH)
			{
				return 2;
			}
			return 1;
		}
	}

	private void Start()
	{
		isSupported = true;
		if (!material)
		{
			material = new Material(shader);
		}
		if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures || !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
		{
			isSupported = false;
		}
	}

	private void OnDisable()
	{
		if ((bool)material)
		{
			Object.DestroyImmediate(material);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (overrideDisable)
		{
			return;
		}
		if (!isSupported)
		{
			Graphics.Blit(source, destination);
			return;
		}
		if (!material)
		{
			material = new Material(shader);
		}
		material.hideFlags = HideFlags.HideAndDontSave;
		material.SetFloat("_BloomIntensity", Mathf.Exp(bloomIntensity) - 1f);
		material.SetFloat("_LensDirtIntensity", Mathf.Exp(lensDirtIntensity) - 1f);
		source.filterMode = FilterMode.Bilinear;
		int num = source.width / 2;
		int num2 = source.height / 2;
		RenderTexture source2 = source;
		float num3 = 1f;
		int iterationCount = IterationCount;
		for (int i = 0; i < 6; i++)
		{
			RenderTexture renderTexture = RenderTexture.GetTemporary(num, num2, 0, source.format);
			renderTexture.filterMode = FilterMode.Bilinear;
			Graphics.Blit(source2, renderTexture, material, 1);
			source2 = renderTexture;
			num3 = ((i <= 1) ? 0.5f : 1f);
			if (i == 2)
			{
				num3 = 0.75f;
			}
			for (int j = 0; j < iterationCount; j++)
			{
				material.SetFloat("_BlurSize", (blurSize * 0.5f + (float)j) * num3);
				RenderTexture temporary = RenderTexture.GetTemporary(num, num2, 0, source.format);
				temporary.filterMode = FilterMode.Bilinear;
				Graphics.Blit(renderTexture, temporary, material, 2);
				RenderTexture.ReleaseTemporary(renderTexture);
				renderTexture = temporary;
				temporary = RenderTexture.GetTemporary(num, num2, 0, source.format);
				temporary.filterMode = FilterMode.Bilinear;
				Graphics.Blit(renderTexture, temporary, material, 3);
				RenderTexture.ReleaseTemporary(renderTexture);
				renderTexture = temporary;
			}
			switch (i)
			{
			case 0:
				material.SetTexture("_Bloom0", renderTexture);
				break;
			case 1:
				material.SetTexture("_Bloom1", renderTexture);
				break;
			case 2:
				material.SetTexture("_Bloom2", renderTexture);
				break;
			case 3:
				material.SetTexture("_Bloom3", renderTexture);
				break;
			case 4:
				material.SetTexture("_Bloom4", renderTexture);
				break;
			case 5:
				material.SetTexture("_Bloom5", renderTexture);
				break;
			}
			RenderTexture.ReleaseTemporary(renderTexture);
			num /= 2;
			num2 /= 2;
		}
		material.SetTexture("_LensDirt", lensDirtTexture);
		Graphics.Blit(source, destination, material, 0);
	}
}
