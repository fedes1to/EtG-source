using System.Text;
using UnityEngine;

public class HUDGC : MonoBehaviour
{
	public static bool ShowGcData;

	public static bool SkipNextGC;

	private static float B2MB = 9.536743E-07f;

	private static float B2kB = 0.0009765625f;

	private dfLabel m_label;

	private uint m_cachedMemMin;

	private uint m_cachedMemMax;

	private uint m_lastFrameMemSize;

	private bool m_showGcBarLastFrame;

	private float m_avgDuration;

	private float m_avgFrequency;

	private float m_avgMemDelta;

	private float m_lastGcTime;

	private float m_lastGcDuration;

	private float m_lastFrameTime;

	private StringBuilder stringBuilder = new StringBuilder();

	public void Start()
	{
		m_label = GetComponent<dfLabel>();
	}

	public void Update()
	{
		if (ShowGcData)
		{
			if (!m_label.IsVisible)
			{
				m_label.IsVisible = true;
			}
			if (!m_showGcBarLastFrame)
			{
				m_cachedMemMin = ProfileUtils.GetMonoUsedHeapSize();
				m_cachedMemMax = ProfileUtils.GetMonoHeapSize();
				m_lastFrameMemSize = m_cachedMemMin;
			}
			uint monoUsedHeapSize = ProfileUtils.GetMonoUsedHeapSize();
			float num = Time.realtimeSinceStartup - m_lastFrameTime;
			if (monoUsedHeapSize < m_lastFrameMemSize)
			{
				m_cachedMemMin = monoUsedHeapSize;
				if (!SkipNextGC)
				{
					m_cachedMemMax = m_lastFrameMemSize;
				}
				float newValue = Time.realtimeSinceStartup - m_lastGcTime;
				if (!SkipNextGC)
				{
					m_lastGcDuration = num;
					m_avgDuration = BraveMathCollege.MovingAverage(m_avgDuration, m_lastGcDuration, 5);
					if (m_lastGcTime > 0f)
					{
						m_avgFrequency = BraveMathCollege.MovingAverage(m_avgFrequency, newValue, 5);
					}
				}
				m_lastGcTime = Time.realtimeSinceStartup;
				m_avgMemDelta = 0f;
			}
			if (num > 0f && monoUsedHeapSize > m_lastFrameMemSize)
			{
				float newValue2 = (float)(monoUsedHeapSize - m_lastFrameMemSize) / num;
				m_avgMemDelta = BraveMathCollege.MovingAverage(m_avgMemDelta, newValue2, 90);
			}
			float num2 = Mathf.InverseLerp(m_cachedMemMin, m_cachedMemMax, monoUsedHeapSize);
			stringBuilder.Length = 0;
			stringBuilder.AppendFormat("Memory Use: {0:00.000}%\n", num2 * 100f);
			stringBuilder.AppendFormat(" - {0:0.00} MB / {1:0.00} MB\n", (float)monoUsedHeapSize * B2MB, (float)m_cachedMemMax * B2MB);
			stringBuilder.AppendFormat(" - {0:+0.00} MB/sec\n", m_avgMemDelta * B2MB);
			stringBuilder.AppendFormat(" - {0:+0.00} kB/frame\n", m_avgMemDelta / 60f * B2kB);
			stringBuilder.AppendFormat("Last GC Duration: {0:00.00} ms\n", m_lastGcDuration * 1000f);
			stringBuilder.AppendFormat("Time Since Last GC: {0: 00.0} sec\n", Time.realtimeSinceStartup - m_lastGcTime);
			stringBuilder.AppendFormat("Avg Duration: {0:00.00} ms\n", m_avgDuration * 1000f);
			stringBuilder.AppendFormat("Avg Time Between: {0:00.0} sec\n", m_avgFrequency);
			stringBuilder.AppendFormat("Total Collections: {0}\n", ProfileUtils.GetMonoCollectionCount());
			m_label.Text = stringBuilder.ToString();
			m_lastFrameMemSize = monoUsedHeapSize;
			m_lastFrameTime = Time.realtimeSinceStartup;
			m_showGcBarLastFrame = true;
		}
		else
		{
			if (m_label.IsVisible)
			{
				m_label.IsVisible = false;
			}
			m_showGcBarLastFrame = false;
		}
		SkipNextGC = false;
	}
}
