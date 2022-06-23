using System;
using UnityEngine;

public class DwarfTestFlowBootstrapper : MonoBehaviour
{
	public static bool IsBootstrapping;

	public static bool ShouldConvertToCoopMode;

	public bool ConvertToCoopMode;

	private void Start()
	{
		GameManager[] array = UnityEngine.Object.FindObjectsOfType<GameManager>();
		foreach (GameManager obj in array)
		{
			UnityEngine.Object.Destroy(obj);
		}
		if (ConvertToCoopMode)
		{
			ShouldConvertToCoopMode = true;
		}
		UnityEngine.Random.InitState(new System.Random().Next(1, 1000));
	}
}
