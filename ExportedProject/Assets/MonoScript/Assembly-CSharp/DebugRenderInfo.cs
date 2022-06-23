using System;
using UnityEngine;
using UnityEngine.Profiling;

[AddComponentMenu("Daikon Forge/Examples/General/Debug Render Info")]
public class DebugRenderInfo : MonoBehaviour
{
	public float interval = 0.5f;

	private dfLabel info;

	private dfGUIManager view;

	private float lastUpdate;

	private int frameCount;

	private void Start()
	{
		info = GetComponent<dfLabel>();
		if (info == null)
		{
			base.enabled = false;
			throw new Exception("No Label component found");
		}
		info.Text = string.Empty;
	}

	private void Update()
	{
		if (view == null)
		{
			view = info.GetManager();
		}
		frameCount++;
		float num = Time.realtimeSinceStartup - lastUpdate;
		if (!(num < interval))
		{
			lastUpdate = Time.realtimeSinceStartup;
			float num2 = 1f / (num / (float)frameCount);
			Vector2 vector = new Vector2(Screen.width, Screen.height);
			string text = string.Format("{0}x{1}", (int)vector.x, (int)vector.y);
			string format = "Screen : {0}, DrawCalls: {1}, Triangles: {2}, Mem: {3:F0}MB, FPS: {4:F0}";
			float num3 = ((!Profiler.supported) ? ((float)GC.GetTotalMemory(false) / 1048576f) : ((float)Profiler.GetMonoUsedSize() / 1048576f));
			string text2 = string.Format(format, text, view.TotalDrawCalls, view.TotalTriangles, num3, num2);
			info.Text = text2.Trim();
			frameCount = 0;
		}
	}
}
