using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class ShadowSystem : BraveBehaviour
{
	private static List<ShadowSystem> m_allLights = new List<ShadowSystem>();

	public static bool DisabledLightsRequireBoost = false;

	[NonSerialized]
	public bool IsDirty;

	public bool IsDynamic;

	public float lightRadius = 10f;

	public bool ignoreUnityLight;

	public Color uLightColor;

	public float uLightIntensity;

	public float uLightRange;

	public Texture2D uLightCookie;

	public float uLightCookieAngle;

	public bool ignoreCustomFloorLight;

	[SerializeField]
	private float minLuminance = 0.01f;

	[SerializeField]
	private float shadowBias = 0.001f;

	[SerializeField]
	private Camera shadowCamera;

	[SerializeField]
	private bool highQuality;

	[SerializeField]
	private Shader lightDistanceShader;

	[SerializeField]
	private Shader transparentShader;

	[SerializeField]
	private Shader casterShader;

	[SerializeField]
	private int shadowMapSize = 512;

	[SerializeField]
	public bool CoronalLight;

	[SerializeField]
	public List<Renderer> PersonalCookies = new List<Renderer>();

	private RenderTexture _texTarget;

	private Dictionary<Shader, Material> _shaderMap = new Dictionary<Shader, Material>();

	private List<RenderTexture> _tempRenderTextures = new List<RenderTexture>();

	private bool m_initialized;

	private Vector3 m_cachedPosition;

	private bool m_locallyDisabled;

	private static int m_numberLightsUpdatedThisFrame = 0;

	public static List<ShadowSystem> AllLights
	{
		get
		{
			return m_allLights;
		}
	}

	private int ModifiedShadowMapSize
	{
		get
		{
			return shadowMapSize;
		}
	}

	private RenderTextureFormat IdealFormat
	{
		get
		{
			return (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf)) ? RenderTextureFormat.Default : RenderTextureFormat.ARGBHalf;
		}
	}

	public static void ForceAllLightsUpdate()
	{
		for (int i = 0; i < m_allLights.Count; i++)
		{
			m_allLights[i].IsDirty = true;
			m_allLights[i].renderer.enabled = true;
		}
	}

	public static void ForceRoomLightsUpdate(RoomHandler room, float duration)
	{
		for (int i = 0; i < m_allLights.Count; i++)
		{
			IntVector2 pos = m_allLights[i].transform.position.IntXY(VectorConversions.Floor);
			if (GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(pos) == room)
			{
				m_allLights[i].TriggerTemporalUpdate(duration);
			}
		}
	}

	public static void ClearPerLevelData()
	{
		m_allLights.Clear();
	}

	private Material GetMaterial(Shader shader)
	{
		Material value;
		if (_shaderMap.TryGetValue(shader, out value))
		{
			return value;
		}
		value = new Material(shader);
		_shaderMap.Add(shader, value);
		return value;
	}

	private void PreprocessAttachedUnityLight()
	{
		Light componentInChildren = base.transform.parent.GetComponentInChildren<Light>();
		if (componentInChildren != null)
		{
			uLightColor = componentInChildren.color;
			uLightIntensity = componentInChildren.intensity;
			uLightRange = componentInChildren.range;
			LightPulser component = componentInChildren.GetComponent<LightPulser>();
			if (component != null)
			{
				component.AssignShadowSystem(this);
			}
			UnityEngine.Object.Destroy(componentInChildren);
		}
	}

	private void Awake()
	{
		base.renderer.enabled = false;
		if (!m_allLights.Contains(this))
		{
			m_allLights.Add(this);
		}
	}

	private void Start()
	{
		if (!ignoreUnityLight)
		{
			PreprocessAttachedUnityLight();
		}
		Material material = base.renderer.material;
		SceneLightManager component = GetComponent<SceneLightManager>();
		if (component != null)
		{
			Color value = component.validColors[UnityEngine.Random.Range(0, component.validColors.Length)];
			material.SetColor("_TintColor", value);
		}
		else
		{
			material.SetColor("_TintColor", Color.white);
		}
	}

	private void CleanupLightsForLowLighting()
	{
		DisabledLightsRequireBoost = true;
		if (_texTarget != null)
		{
			UnityEngine.Object.Destroy(_texTarget);
		}
		ReleaseAllRenderTextures();
		base.renderer.enabled = false;
		base.transform.parent.gameObject.SetActive(false);
	}

	private void ReturnFromDead()
	{
		DisabledLightsRequireBoost = false;
		shadowMapSize = Mathf.NextPowerOfTwo(shadowMapSize);
		shadowMapSize = Mathf.Clamp(shadowMapSize, 8, 2048);
		if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
		{
			_texTarget = new RenderTexture(ModifiedShadowMapSize, ModifiedShadowMapSize, 0, RenderTextureFormat.ARGBHalf);
		}
		else
		{
			_texTarget = new RenderTexture(ModifiedShadowMapSize, ModifiedShadowMapSize, 0, RenderTextureFormat.Default);
		}
		_texTarget.useMipMap = false;
		_texTarget.autoGenerateMips = false;
		shadowCamera.rect = new Rect(0f, 0f, 1f, 1f);
		base.transform.localScale = Vector3.one * shadowCamera.orthographicSize / 5f;
		base.transform.localScale = base.transform.localScale.WithZ(base.transform.localScale.z * 1.414f);
		base.renderer.material.mainTexture = _texTarget;
		IsDirty = true;
		base.renderer.enabled = true;
	}

	private void OnEnable()
	{
		if (GameManager.Options.LightingQuality == GameOptions.GenericHighMedLowOption.LOW || ignoreCustomFloorLight || m_locallyDisabled)
		{
			if (ignoreCustomFloorLight)
			{
				PreprocessAttachedUnityLight();
			}
			CleanupLightsForLowLighting();
		}
		else
		{
			ReturnFromDead();
		}
	}

	protected override void OnDestroy()
	{
		if (m_allLights != null && m_allLights.Contains(this))
		{
			m_allLights.Remove(this);
		}
		foreach (KeyValuePair<Shader, Material> item in _shaderMap)
		{
			UnityEngine.Object.Destroy(item.Value);
		}
		_shaderMap.Clear();
		if (_texTarget != null)
		{
			UnityEngine.Object.Destroy(_texTarget);
		}
		ReleaseAllRenderTextures();
	}

	private void TriggerTemporalUpdate(float duration)
	{
		StartCoroutine(HandleTemporalUpdate(duration));
	}

	private IEnumerator HandleTemporalUpdate(float duration)
	{
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			RenderFullShadowMap();
			yield return null;
		}
	}

	private bool RequiresCasterDepthBuffer()
	{
		if (StaticReferenceManager.AllShadowSystemDepthHavers.Count > 0)
		{
			Vector2 vector = base.transform.PositionVector2();
			float num = lightRadius * lightRadius;
			for (int i = 0; i < StaticReferenceManager.AllShadowSystemDepthHavers.Count; i++)
			{
				Transform t = StaticReferenceManager.AllShadowSystemDepthHavers[i];
				float sqrMagnitude = (vector - t.PositionVector2()).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void RenderFullShadowMap()
	{
		if (GameManager.Options.LightingQuality == GameOptions.GenericHighMedLowOption.LOW || ignoreCustomFloorLight)
		{
			if (ignoreCustomFloorLight)
			{
				PreprocessAttachedUnityLight();
			}
			CleanupLightsForLowLighting();
			return;
		}
		if (!base.transform.parent.gameObject.activeSelf)
		{
			base.transform.parent.gameObject.SetActive(true);
			DisabledLightsRequireBoost = false;
			ReturnFromDead();
		}
		for (int i = 0; i < PersonalCookies.Count; i++)
		{
			PersonalCookies[i].enabled = true;
		}
		base.transform.position = base.transform.position.WithZ(base.transform.position.y - 2.5f);
		tk2dBaseSprite tk2dBaseSprite2 = null;
		int layer = -1;
		int layer2 = -1;
		if (GameManager.Instance.IsFoyer && GameManager.Instance.PrimaryPlayer != null)
		{
			tk2dBaseSprite2 = GameManager.Instance.PrimaryPlayer.sprite;
			layer = tk2dBaseSprite2.gameObject.layer;
			tk2dBaseSprite2.gameObject.SetLayerRecursively(LayerMask.NameToLayer("PlayerAndProjectiles"));
			if (GameManager.Instance.SecondaryPlayer != null)
			{
				layer2 = GameManager.Instance.SecondaryPlayer.sprite.gameObject.layer;
				GameManager.Instance.SecondaryPlayer.sprite.gameObject.SetLayerRecursively(LayerMask.NameToLayer("PlayerAndProjectiles"));
			}
		}
		int depth = (RequiresCasterDepthBuffer() ? 16 : 0);
		RenderTexture renderTexture = PushRenderTexture(ModifiedShadowMapSize, ModifiedShadowMapSize, depth, IdealFormat);
		renderTexture.filterMode = FilterMode.Point;
		renderTexture.wrapMode = TextureWrapMode.Clamp;
		shadowCamera.targetTexture = renderTexture;
		if (casterShader != null)
		{
			shadowCamera.RenderWithShader(casterShader, string.Empty);
		}
		else
		{
			shadowCamera.Render();
		}
		if (GameManager.Instance.IsFoyer && GameManager.Instance.PrimaryPlayer != null)
		{
			tk2dBaseSprite2.gameObject.SetLayerRecursively(layer);
			if (GameManager.Instance.SecondaryPlayer != null)
			{
				GameManager.Instance.SecondaryPlayer.sprite.gameObject.SetLayerRecursively(layer2);
			}
		}
		Material material = GetMaterial(lightDistanceShader);
		material.SetFloat("_MinLuminance", minLuminance);
		material.SetFloat("_ShadowOffset", shadowBias);
		material.SetFloat("_Resolution", ModifiedShadowMapSize);
		material.SetFloat("_LightRadius", lightRadius);
		RenderTexture renderTexture2 = PushRenderTexture(ModifiedShadowMapSize, 1, 0, IdealFormat);
		Graphics.Blit(renderTexture, renderTexture2, material, 0);
		Graphics.Blit(renderTexture2, _texTarget, material, (!highQuality) ? 1 : 2);
		ReleaseAllRenderTextures();
		m_initialized = true;
		m_cachedPosition = base.transform.position;
		for (int j = 0; j < PersonalCookies.Count; j++)
		{
			PersonalCookies[j].enabled = false;
		}
		if (!base.renderer.enabled)
		{
			base.renderer.enabled = true;
		}
	}

	private void Update()
	{
		m_numberLightsUpdatedThisFrame = 0;
	}

	private void LateUpdate()
	{
		if (!base.renderer.isVisible && 1 == 0)
		{
			return;
		}
		bool flag = !_texTarget.IsCreated();
		bool flag2 = base.renderer.isVisible && IsDynamic;
		if (m_initialized && !IsDirty && !(base.transform.position != m_cachedPosition) && !flag2 && !flag)
		{
			return;
		}
		if (!flag2 && !flag)
		{
			if (m_numberLightsUpdatedThisFrame < 3)
			{
				IsDirty = false;
				m_numberLightsUpdatedThisFrame++;
				RenderFullShadowMap();
			}
		}
		else
		{
			IsDirty = false;
			RenderFullShadowMap();
		}
	}

	private RenderTexture PushRenderTexture(int width, int height, int depth = 0, RenderTextureFormat format = RenderTextureFormat.ARGBHalf)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, depth, format);
		temporary.filterMode = FilterMode.Point;
		temporary.wrapMode = TextureWrapMode.Clamp;
		_tempRenderTextures.Add(temporary);
		return temporary;
	}

	private void ReleaseAllRenderTextures()
	{
		if (_tempRenderTextures == null || _tempRenderTextures.Count == 0)
		{
			return;
		}
		foreach (RenderTexture tempRenderTexture in _tempRenderTextures)
		{
			RenderTexture.ReleaseTemporary(tempRenderTexture);
		}
		_tempRenderTextures.Clear();
	}
}
