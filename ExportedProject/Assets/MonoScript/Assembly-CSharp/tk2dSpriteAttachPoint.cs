using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("2D Toolkit/Sprite/tk2dSpriteAttachPoint")]
[ExecuteInEditMode]
public class tk2dSpriteAttachPoint : MonoBehaviour
{
	private tk2dBaseSprite sprite;

	public List<Transform> attachPoints = new List<Transform>();

	private static bool[] attachPointUpdated = new bool[32];

	public bool deactivateUnusedAttachPoints;

	public bool disableEmissionOnUnusedParticleSystems;

	public bool ignorePosition;

	public bool ignoreScale;

	public bool ignoreRotation;

	public bool centerUnusedAttachPoints;

	private List<string> attachPointNames = new List<string>();

	private static tk2dSpriteDefinition.AttachPoint[] emptyAttachPointArray = null;

	public Transform GetAttachPointByName(string name)
	{
		if (attachPoints.Count != attachPointNames.Count)
		{
			ReinitAttachPointNames();
		}
		for (int i = 0; i < attachPoints.Count; i++)
		{
			if (attachPoints[i].name.ToLowerInvariant() == name.ToLowerInvariant())
			{
				return attachPoints[i];
			}
		}
		return null;
	}

	private void ReinitAttachPointNames()
	{
		attachPointNames.Clear();
		for (int i = 0; i < attachPoints.Count; i++)
		{
			attachPointNames.Add((!attachPoints[i]) ? null : attachPoints[i].name);
		}
	}

	private void Awake()
	{
		if (sprite == null)
		{
			sprite = GetComponent<tk2dBaseSprite>();
			if (sprite != null)
			{
				HandleSpriteChanged(sprite);
			}
		}
	}

	private void OnEnable()
	{
		if (sprite != null)
		{
			sprite.SpriteChanged += HandleSpriteChanged;
		}
	}

	private void OnDisable()
	{
		if (sprite != null)
		{
			sprite.SpriteChanged -= HandleSpriteChanged;
		}
	}

	private void UpdateAttachPointTransform(tk2dSpriteDefinition.AttachPoint attachPoint, Transform t)
	{
		if (!ignorePosition)
		{
			t.localPosition = Vector3.Scale(attachPoint.position, sprite.scale);
		}
		if (!ignoreScale)
		{
			t.localScale = sprite.scale;
		}
		if (!ignoreRotation)
		{
			float num = Mathf.Sign(sprite.scale.x) * Mathf.Sign(sprite.scale.y);
			t.localEulerAngles = new Vector3(0f, 0f, attachPoint.angle * num);
		}
		if (disableEmissionOnUnusedParticleSystems)
		{
			ParticleSystem component = t.GetComponent<ParticleSystem>();
			if ((bool)component)
			{
				BraveUtility.EnableEmission(component, true);
			}
		}
	}

	public void ForceAddAttachPoint(string apname)
	{
		GameObject gameObject = new GameObject(apname);
		Transform transform = gameObject.transform;
		transform.parent = base.transform;
		if (deactivateUnusedAttachPoints)
		{
			gameObject.SetActive(false);
		}
		attachPoints.Add(transform);
	}

	private void HandleSpriteChanged(tk2dBaseSprite spr)
	{
		tk2dSpriteDefinition.AttachPoint[] array = spr.Collection.GetAttachPoints(spr.spriteId);
		if (emptyAttachPointArray == null)
		{
			emptyAttachPointArray = new tk2dSpriteDefinition.AttachPoint[0];
		}
		if (array == null)
		{
			array = emptyAttachPointArray;
		}
		int num = Mathf.Max(array.Length, attachPoints.Count);
		if (num > attachPointUpdated.Length)
		{
			attachPointUpdated = new bool[num];
		}
		if (attachPoints.Count != attachPointNames.Count)
		{
			ReinitAttachPointNames();
		}
		for (int i = 0; i < attachPointUpdated.Length; i++)
		{
			attachPointUpdated[i] = false;
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (attachPoints.Count != attachPointNames.Count)
			{
				ReinitAttachPointNames();
			}
			tk2dSpriteDefinition.AttachPoint attachPoint = array[j];
			bool flag = false;
			for (int k = 0; k < attachPoints.Count; k++)
			{
				Transform transform = attachPoints[k];
				int num2 = attachPoints.IndexOf(transform);
				if (transform != null && attachPointNames[k] == attachPoint.name)
				{
					if (deactivateUnusedAttachPoints || centerUnusedAttachPoints || disableEmissionOnUnusedParticleSystems)
					{
						attachPointUpdated[num2] = true;
					}
					UpdateAttachPointTransform(attachPoint, transform);
					flag = true;
				}
			}
			if (!flag)
			{
				GameObject gameObject = new GameObject(attachPoint.name);
				Transform transform2 = gameObject.transform;
				transform2.parent = base.transform;
				UpdateAttachPointTransform(attachPoint, transform2);
				attachPoints.Add(transform2);
			}
		}
		if (centerUnusedAttachPoints)
		{
			for (int l = 0; l < attachPointUpdated.Length; l++)
			{
				if (l < attachPoints.Count && attachPoints[l] != null)
				{
					GameObject gameObject2 = attachPoints[l].gameObject;
					if (!attachPointUpdated[l] && gameObject2.activeSelf)
					{
						gameObject2.transform.position = spr.WorldCenter.ToVector3ZUp(gameObject2.transform.position.z);
					}
				}
			}
		}
		if (disableEmissionOnUnusedParticleSystems)
		{
			for (int m = 0; m < attachPointUpdated.Length; m++)
			{
				if (m < attachPoints.Count && !attachPointUpdated[m] && attachPoints[m] != null && (bool)attachPoints[m].gameObject)
				{
					ParticleSystem component = attachPoints[m].gameObject.GetComponent<ParticleSystem>();
					if ((bool)component)
					{
						BraveUtility.EnableEmission(component, false);
					}
				}
			}
		}
		if (!deactivateUnusedAttachPoints)
		{
			return;
		}
		for (int n = 0; n < attachPoints.Count; n++)
		{
			if (attachPoints[n] != null)
			{
				GameObject gameObject3 = attachPoints[n].gameObject;
				if (attachPointUpdated[n] && !gameObject3.activeSelf)
				{
					gameObject3.SetActive(true);
				}
				else if (!attachPointUpdated[n] && gameObject3.activeSelf)
				{
					gameObject3.SetActive(false);
				}
			}
			attachPointUpdated[n] = false;
		}
	}
}
