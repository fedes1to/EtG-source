using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/User Interface/Animation Clip")]
public class dfAnimationClip : MonoBehaviour
{
	[SerializeField]
	private dfAtlas atlas;

	[SerializeField]
	private List<string> sprites = new List<string>();

	public dfAtlas Atlas
	{
		get
		{
			return atlas;
		}
		set
		{
			atlas = value;
		}
	}

	public List<string> Sprites
	{
		get
		{
			return sprites;
		}
	}
}
