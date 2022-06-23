using UnityEngine;

public class SimpleSparksDoer : MonoBehaviour
{
	public Vector3 localMin;

	public Vector3 localMax;

	public GlobalSparksDoer.SparksType sparksType;

	public Vector3 baseDirection = Vector3.up;

	public float magnitudeVariance = 0.5f;

	public float angleVariance = 45f;

	public float LifespanMin = 0.5f;

	public float LifespanMax = 1f;

	public int SparksPerSecond = 60;

	public bool DefineColor;

	public Color Color1;

	public Color Color2;

	private Transform m_transform;

	private float m_particlesToSpawn;

	private void Start()
	{
		m_transform = base.gameObject.transform;
	}

	private void Update()
	{
		if (GameManager.Options.ShaderQuality != 0 && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW)
		{
			Color? startColor = null;
			if (DefineColor)
			{
				startColor = new Color(Random.Range(Color1.r, Color2.r), Random.Range(Color1.g, Color2.g), Random.Range(Color1.b, Color2.b), Random.Range(Color1.a, Color2.a));
			}
			m_particlesToSpawn += (float)SparksPerSecond * BraveTime.DeltaTime;
			GlobalSparksDoer.DoRandomParticleBurst((int)m_particlesToSpawn, m_transform.position + localMin, m_transform.position + localMax, baseDirection, angleVariance, magnitudeVariance, null, Random.Range(LifespanMin, LifespanMax), startColor, sparksType);
			m_particlesToSpawn %= 1f;
		}
	}
}
