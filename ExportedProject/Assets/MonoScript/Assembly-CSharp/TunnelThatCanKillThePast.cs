using System;
using System.Collections;
using Kvant;
using Reaktion;
using UnityEngine;

public class TunnelThatCanKillThePast : MonoBehaviour
{
	public enum CurrentTunnelPhase
	{
		STANDARD,
		PULSED,
		SHATTER
	}

	public Tunnel targetTunnel;

	public Material targetMaterial;

	public MeshRenderer ShellRenderer;

	public float MinPulse = -1f;

	public float MaxPulse = 1f;

	public float PulsePeriod = 0.5f;

	public float ShatterTime = 1f;

	public AnimationCurve ShatterCurve;

	public tk2dSprite BulletSprite;

	[NonSerialized]
	public bool shattering;

	private CurrentTunnelPhase m_currentPhase;

	private float m_timeSincePhaseTransition;

	private int m_displacementID = -1;

	private float m_standardDisplacement;

	public float BulletX = 1.3f;

	public float BulletY = -1.5f;

	public float BulletZ = 10f;

	private void Awake()
	{
		if (!BulletSprite)
		{
			return;
		}
		if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.HELLGEON)
		{
			BulletSprite.ignoresTiltworldDepth = true;
			JitterMotion componentInChildren = GetComponentInChildren<JitterMotion>();
			if ((bool)componentInChildren)
			{
				componentInChildren.InfluenceAxialX = 10f;
				componentInChildren.InfluenceAxialY = 10f;
			}
		}
		else
		{
			BulletSprite.gameObject.SetActive(false);
			BulletSprite.renderer.enabled = false;
			BulletSprite = null;
		}
	}

	private void Start()
	{
		m_displacementID = Shader.PropertyToID("_Displacement");
		ShellRenderer.material.SetFloat("_Gain", 0.1f);
		ShellRenderer.material.SetFloat("_Brightness", 0.005f);
		if (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.VERY_LOW || GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.LOW || GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.MEDIUM || GameManager.Options.LightingQuality == GameOptions.GenericHighMedLowOption.LOW)
		{
			ShellRenderer.enabled = false;
		}
		else
		{
			ShellRenderer.enabled = true;
		}
		StartCoroutine(HandleBulletPosition());
	}

	public void ChangeToPulsed()
	{
		m_timeSincePhaseTransition = 0f;
		m_currentPhase = CurrentTunnelPhase.PULSED;
	}

	public void ManuallySetShatterAmount(float amount)
	{
		m_standardDisplacement = amount;
	}

	public void Shatter()
	{
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			BraveUtility.EnableEmission(componentsInChildren[i], false);
		}
		m_timeSincePhaseTransition = 0f;
		m_currentPhase = CurrentTunnelPhase.SHATTER;
	}

	private void Update()
	{
		m_timeSincePhaseTransition += GameManager.INVARIANT_DELTA_TIME;
		switch (m_currentPhase)
		{
		case CurrentTunnelPhase.STANDARD:
			UpdateStandard();
			break;
		case CurrentTunnelPhase.PULSED:
			UpdatePulsed();
			break;
		case CurrentTunnelPhase.SHATTER:
			UpdateShatter();
			break;
		}
	}

	private IEnumerator HandleBulletPosition()
	{
		if (!BulletSprite)
		{
			yield break;
		}
		do
		{
			float curZOffset2 = BraveMathCollege.SmoothLerp(-2f, 2f, Mathf.PingPong(Time.realtimeSinceStartup / 6f, 1f));
			Vector3 targetPos2 = new Vector3(BulletX, BulletY, BulletZ + curZOffset2);
			BulletSprite.transform.localPosition = targetPos2;
			yield return null;
		}
		while (!shattering);
		if (shattering)
		{
			float ela = 0f;
			float dura = 1f;
			while (ela < dura + 3f)
			{
				ela += GameManager.INVARIANT_DELTA_TIME;
				float curZOffset = BraveMathCollege.SmoothLerp(-2f, 2f, Mathf.PingPong(Time.realtimeSinceStartup / 6f, 1f));
				float realZOffset = Mathf.Lerp(curZOffset, 100f, Mathf.Clamp01((ela - 3f) / dura));
				Vector3 targetPos = new Vector3(BulletX, BulletY, BulletZ + realZOffset);
				BulletSprite.transform.localPosition = targetPos;
				yield return null;
			}
		}
	}

	private void UpdateStandard()
	{
		targetMaterial.SetFloat(m_displacementID, m_standardDisplacement);
	}

	private void UpdatePulsed()
	{
		targetMaterial.SetFloat(m_displacementID, Mathf.Lerp(MinPulse, MaxPulse, Mathf.SmoothStep(0f, 1f, Mathf.PingPong(m_timeSincePhaseTransition, PulsePeriod) / PulsePeriod)));
	}

	private void UpdateShatter()
	{
		float value = Mathf.Lerp(m_standardDisplacement, -100f, ShatterCurve.Evaluate(m_timeSincePhaseTransition / ShatterTime));
		targetMaterial.SetFloat(m_displacementID, value);
		float value2 = Mathf.Lerp(0.005f, 0f, m_timeSincePhaseTransition / ShatterTime);
		float value3 = Mathf.Lerp(0.1f, 0f, m_timeSincePhaseTransition / ShatterTime);
		ShellRenderer.material.SetFloat("_Brightness", value2);
		ShellRenderer.material.SetFloat("_Gain", value3);
	}
}
