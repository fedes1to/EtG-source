using UnityEngine;

public class EmissionSettings : MonoBehaviour
{
	public float EmissivePower;

	public float EmissiveColorPower = 7f;

	private static bool indicesInitialized;

	private static int powerIndex;

	private static int colorPowerIndex;

	private void Start()
	{
		if (!indicesInitialized)
		{
			indicesInitialized = true;
			powerIndex = Shader.PropertyToID("_EmissivePower");
			colorPowerIndex = Shader.PropertyToID("_EmissiveColorPower");
		}
		tk2dBaseSprite component = GetComponent<tk2dBaseSprite>();
		if (component != null)
		{
			component.usesOverrideMaterial = true;
		}
		GetComponent<Renderer>().material.SetFloat(powerIndex, EmissivePower);
		GetComponent<Renderer>().material.SetFloat(colorPowerIndex, EmissiveColorPower);
	}
}
