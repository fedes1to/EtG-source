using UnityEngine;

public class MagicCircleHelper : MonoBehaviour
{
	public ParticleSystem CircleParticles;

	public float CircleStartVal = 0.75f;

	public float EmissiveColorPower = 7f;

	public float minBrightness = 0.5f;

	public float maxBrightness = 1f;

	public float minEmissivePower = 50f;

	public float maxEmissivePower = 100f;

	public float pulsePeriod = 1f;

	public float fadeInTime = 1f;

	private float elapsed;

	private Material m_materialInst;

	private MeshFilter m_mf;

	private static bool indicesInitialized;

	private static int powerIndex;

	private static int colorPowerIndex;

	private static int circlefadeIndex;

	private static int uvRangeIndex;

	private static int brightnessIndex;

	private void Start()
	{
		if (!indicesInitialized)
		{
			indicesInitialized = true;
			powerIndex = Shader.PropertyToID("_EmissivePower");
			colorPowerIndex = Shader.PropertyToID("_EmissiveColorPower");
			circlefadeIndex = Shader.PropertyToID("_RadialFade");
			uvRangeIndex = Shader.PropertyToID("_UVMinMax");
			brightnessIndex = Shader.PropertyToID("_Brightness");
		}
		tk2dBaseSprite component = GetComponent<tk2dBaseSprite>();
		if (component != null)
		{
			component.usesOverrideMaterial = true;
		}
		m_mf = GetComponent<MeshFilter>();
		m_materialInst = GetComponent<Renderer>().material;
		m_materialInst.SetFloat(powerIndex, minEmissivePower);
		m_materialInst.SetFloat(colorPowerIndex, EmissiveColorPower);
	}

	public void OnSpawned()
	{
		elapsed = 0f;
	}

	private Vector4 GetMinMaxUVs()
	{
		Vector2 rhs = new Vector2(float.MaxValue, float.MaxValue);
		Vector2 rhs2 = new Vector2(float.MinValue, float.MinValue);
		for (int i = 0; i < m_mf.sharedMesh.uv.Length; i++)
		{
			rhs = Vector2.Min(m_mf.sharedMesh.uv[i], rhs);
			rhs2 = Vector2.Max(m_mf.sharedMesh.uv[i], rhs2);
		}
		return new Vector4(rhs.x, rhs.y, rhs2.x, rhs2.y);
	}

	private void LateUpdate()
	{
		m_materialInst.SetVector(uvRangeIndex, GetMinMaxUVs());
		elapsed += BraveTime.DeltaTime;
		m_materialInst.SetFloat(circlefadeIndex, Mathf.Lerp(1f, 0f, elapsed / fadeInTime));
		float t = Mathf.PingPong(elapsed, pulsePeriod) / pulsePeriod;
		m_materialInst.SetFloat(brightnessIndex, Mathf.Lerp(minBrightness, maxBrightness, t) * Mathf.Clamp01(elapsed / fadeInTime));
		m_materialInst.SetFloat(powerIndex, Mathf.Lerp(minEmissivePower, maxEmissivePower, t) * Mathf.Clamp01(elapsed / fadeInTime));
		if (CircleParticles != null)
		{
			BraveUtility.EnableEmission(CircleParticles, elapsed / fadeInTime >= CircleStartVal);
		}
	}
}
