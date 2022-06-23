using System;
using UnityEngine;

public class TowerBossEmitterController : MonoBehaviour
{
	public float currentAngle;

	public string eastSpriteName;

	public string westSpriteName;

	public string southSpriteName;

	private tk2dSprite m_sprite;

	private void Start()
	{
		m_sprite = GetComponent<tk2dSprite>();
	}

	public void UpdateAngle(float newAngle)
	{
		currentAngle = newAngle;
		float num = currentAngle / (float)Math.PI;
		if (num > 0.05f && num < 0.95f)
		{
			m_sprite.renderer.enabled = false;
		}
		else if (num > 1.75f || num <= 0.05f)
		{
			m_sprite.renderer.enabled = true;
			m_sprite.SetSprite(eastSpriteName);
		}
		else if (num > 1.25f)
		{
			m_sprite.renderer.enabled = true;
			m_sprite.SetSprite(southSpriteName);
		}
		else
		{
			m_sprite.renderer.enabled = true;
			m_sprite.SetSprite(westSpriteName);
		}
	}
}
