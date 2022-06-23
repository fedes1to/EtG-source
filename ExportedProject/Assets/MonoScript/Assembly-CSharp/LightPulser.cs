using System.Collections;
using UnityEngine;

public class LightPulser : MonoBehaviour
{
	public bool flicker;

	public float pulseSpeed = 40f;

	public float waitTime = 0.05f;

	public float normalRange = 3.33f;

	public float flickerRange = 0.5f;

	private ShadowSystem m_sl;

	private void Start()
	{
		if (flicker)
		{
			StartCoroutine("Flicker");
		}
	}

	public void AssignShadowSystem(ShadowSystem ss)
	{
		m_sl = ss;
	}

	private IEnumerator Flicker()
	{
		while (true)
		{
			if (m_sl != null)
			{
				if (m_sl.uLightRange == normalRange)
				{
					m_sl.uLightRange = flickerRange;
				}
				else
				{
					m_sl.uLightRange = normalRange;
				}
			}
			else if (GetComponent<Light>().range == normalRange)
			{
				GetComponent<Light>().range = flickerRange;
			}
			else
			{
				GetComponent<Light>().range = normalRange;
			}
			yield return new WaitForSeconds(waitTime);
		}
	}

	private void Update()
	{
		if (!flicker)
		{
			if (m_sl != null)
			{
				m_sl.uLightRange = flickerRange + Mathf.PingPong(Time.time * pulseSpeed, normalRange - flickerRange);
			}
			else
			{
				GetComponent<Light>().range = flickerRange + Mathf.PingPong(Time.time * pulseSpeed, normalRange - flickerRange);
			}
		}
	}
}
