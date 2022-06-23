using System;
using UnityEngine;

public class HUDFPS : MonoBehaviour
{
	[NonSerialized]
	public float updateInterval = 10f;

	private dfLabel m_label;

	private float accum;

	private int frames;

	private float timeleft;

	private void Start()
	{
		updateInterval = 0.5f;
		m_label = GetComponent<dfLabel>();
		if (!m_label)
		{
			Debug.Log("FramesPerSecond needs a dfLabel component!");
			base.enabled = false;
		}
		else
		{
			timeleft = updateInterval;
		}
	}

	private void Update()
	{
		if (!m_label.IsVisible)
		{
			return;
		}
		timeleft -= GameManager.INVARIANT_DELTA_TIME;
		accum += GameManager.INVARIANT_DELTA_TIME;
		frames++;
		if ((double)timeleft <= 0.0)
		{
			float num = (float)frames / accum;
			string text = string.Format("{0:F2} FPS", num);
			m_label.Text = text;
			if (num < 30f)
			{
				m_label.Color = Color.yellow;
			}
			else if (num < 10f)
			{
				m_label.Color = Color.red;
			}
			else
			{
				m_label.Color = Color.green;
			}
			timeleft = updateInterval;
			accum = 0f;
			frames = 0;
		}
	}
}
