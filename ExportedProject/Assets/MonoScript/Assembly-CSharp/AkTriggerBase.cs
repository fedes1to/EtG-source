using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AkTriggerBase : MonoBehaviour
{
	public delegate void Trigger(GameObject in_gameObject);

	public Trigger triggerDelegate;

	public static Dictionary<uint, string> GetAllDerivedTypes()
	{
		Dictionary<uint, string> dictionary = new Dictionary<uint, string>();
		Type typeFromHandle = typeof(AkTriggerBase);
		Type[] types = typeFromHandle.Assembly.GetTypes();
		for (int i = 0; i < types.Length; i++)
		{
			if (types[i].IsClass && (types[i].IsSubclassOf(typeFromHandle) || (typeFromHandle.IsAssignableFrom(types[i]) && typeFromHandle != types[i])))
			{
				string text = types[i].Name;
				dictionary.Add(AkUtilities.ShortIDGenerator.Compute(text), text);
			}
		}
		dictionary.Add(AkUtilities.ShortIDGenerator.Compute("Awake"), "Awake");
		dictionary.Add(AkUtilities.ShortIDGenerator.Compute("Start"), "Start");
		dictionary.Add(AkUtilities.ShortIDGenerator.Compute("Destroy"), "Destroy");
		return dictionary;
	}
}
