using System.Collections;
using UnityEngine;

public class HeatIndicatorController : MonoBehaviour
{
	public float CurrentRadius = 5f;

	public Color CurrentColor = new Color(1f, 0f, 0f, 1f);

	public bool IsFire = true;

	private Material m_materialInst;

	private int m_radiusID;

	private int m_centerID;

	private int m_colorID;

	public void Awake()
	{
		m_radiusID = Shader.PropertyToID("_Radius");
		m_centerID = Shader.PropertyToID("_WorldCenter");
		m_colorID = Shader.PropertyToID("_RingColor");
		m_materialInst = GetComponent<MeshRenderer>().material;
	}

	public void Start()
	{
		StartCoroutine(LerpColor(this, new Color(0f, 0f, 0f, 0f), CurrentColor, 0.5f));
		if (!IsFire)
		{
			ParticleSystem componentInChildren = GetComponentInChildren<ParticleSystem>();
			if ((bool)componentInChildren)
			{
				ParticleSystem.EmissionModule emission = componentInChildren.emission;
				emission.enabled = false;
				componentInChildren.gameObject.SetActive(false);
			}
		}
	}

	public void LateUpdate()
	{
		base.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
		base.transform.localScale = new Vector3(CurrentRadius * 2f, CurrentRadius * 2f * Mathf.Sqrt(2f), 1f);
		Vector3 position = base.transform.position;
		m_materialInst.SetVector(m_centerID, new Vector4(position.x, position.y, position.z, 0f));
		m_materialInst.SetFloat(m_radiusID, CurrentRadius);
		m_materialInst.SetColor(m_colorID, CurrentColor);
	}

	public void EndEffect()
	{
		StartCoroutine(LerpColor(this, CurrentColor, new Color(0f, 0f, 0f, 0f), 0.5f));
		StartCoroutine(HandleDeathDelay());
	}

	private IEnumerator HandleDeathDelay()
	{
		ParticleSystem m_particleSystem = GetComponentInChildren<ParticleSystem>();
		if ((bool)m_particleSystem)
		{
			ParticleSystem.EmissionModule emission = m_particleSystem.emission;
			emission.enabled = false;
		}
		float elapsed = 0f;
		while ((bool)m_particleSystem && m_particleSystem.particleCount > 0)
		{
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		while ((double)elapsed < 0.5)
		{
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		Object.Destroy(base.gameObject);
	}

	private IEnumerator LerpColor(HeatIndicatorController indicator, Color startColor, Color targetColor, float lerpTime)
	{
		float elapsed = 0f;
		while (elapsed < lerpTime)
		{
			elapsed += BraveTime.DeltaTime;
			float t = Mathf.Clamp01(elapsed / lerpTime);
			Color c = (indicator.CurrentColor = Color.Lerp(startColor, targetColor, t));
			yield return null;
		}
	}
}
