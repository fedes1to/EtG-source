using System.Collections.Generic;
using UnityEngine;

public class MinorBreakableGroupManager : BraveBehaviour
{
	public enum MinorBreakableGroupBehavior
	{
		TRIGGERS_DEBRIS,
		TRIGGERS_BREAK,
		NONE
	}

	public MinorBreakableGroupBehavior behavior;

	public bool autodetectDimensions = true;

	public IntVector2 overridePixelDimensions;

	private List<MinorBreakable> registeredBreakables = new List<MinorBreakable>();

	private List<DebrisObject> registeredDebris = new List<DebrisObject>();

	public void Initialize()
	{
		MinorBreakable[] componentsInChildren = GetComponentsInChildren<MinorBreakable>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			RegisterMinorBreakable(componentsInChildren[i]);
		}
		DebrisObject[] componentsInChildren2 = GetComponentsInChildren<DebrisObject>(true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			MinorBreakable component = componentsInChildren2[j].GetComponent<MinorBreakable>();
			if (component == null)
			{
				RegisterDebris(componentsInChildren2[j]);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public Vector2 GetDimensions()
	{
		if (!autodetectDimensions)
		{
			return PhysicsEngine.PixelToUnit(overridePixelDimensions);
		}
		float a = float.MaxValue;
		float a2 = float.MaxValue;
		float num = base.transform.position.x;
		float num2 = base.transform.position.y;
		tk2dSprite[] componentsInChildren = GetComponentsInChildren<tk2dSprite>(true);
		foreach (tk2dSprite tk2dSprite2 in componentsInChildren)
		{
			Transform transform = tk2dSprite2.transform;
			Bounds bounds = tk2dSprite2.GetBounds();
			float x = bounds.size.x;
			float y = bounds.size.y;
			a = Mathf.Min(a, transform.position.x);
			a2 = Mathf.Min(a2, transform.position.y);
			num = Mathf.Max(num, transform.position.x + x);
			num2 = Mathf.Max(num2, transform.position.y + y);
		}
		return new Vector2(num - base.transform.position.x, num2 - base.transform.position.y);
	}

	public void Destabilize(Vector3 force, float height)
	{
		for (int i = 0; i < registeredBreakables.Count; i++)
		{
			MinorBreakable minorBreakable = registeredBreakables[i];
			if (minorBreakable != null && (bool)minorBreakable)
			{
				if ((bool)minorBreakable.sprite && minorBreakable.sprite.attachParent != null)
				{
					minorBreakable.sprite.attachParent.DetachRenderer(minorBreakable.sprite);
					minorBreakable.sprite.attachParent = null;
				}
				DebrisObject component = minorBreakable.GetComponent<DebrisObject>();
				if (component != null)
				{
					component.Trigger(Quaternion.Euler(0f, 0f, Mathf.Lerp(-30f, 30f, Random.value)) * force, height);
				}
				else
				{
					minorBreakable.Break(force.XY());
				}
			}
		}
		for (int j = 0; j < registeredDebris.Count; j++)
		{
			DebrisObject debrisObject = registeredDebris[j];
			if ((bool)debrisObject && (bool)debrisObject.sprite)
			{
				if (debrisObject.sprite.attachParent != null)
				{
					debrisObject.sprite.attachParent.DetachRenderer(debrisObject.sprite);
					debrisObject.sprite.attachParent = null;
				}
				debrisObject.Trigger(Quaternion.Euler(0f, 0f, Mathf.Lerp(-30f, 30f, Random.value)) * force, height);
				j--;
			}
		}
		registeredBreakables.Clear();
		registeredDebris.Clear();
	}

	public void InformBroken(MinorBreakable mb, Vector2 breakForce, float breakHeight)
	{
		DeregisterMinorBreakable(mb);
		for (int i = 0; i < registeredBreakables.Count; i++)
		{
			MinorBreakable minorBreakable = registeredBreakables[i];
			if (!minorBreakable)
			{
				continue;
			}
			switch (behavior)
			{
			case MinorBreakableGroupBehavior.TRIGGERS_DEBRIS:
			{
				DebrisObject component = minorBreakable.GetComponent<DebrisObject>();
				Vector3 zero = Vector3.zero;
				zero = ((!(breakForce == Vector2.zero)) ? (Quaternion.Euler(0f, 0f, Mathf.Lerp(-30f, 30f, Random.value)) * breakForce.ToVector3ZUp(0.5f)) : Random.insideUnitCircle.normalized.ToVector3ZUp(0.5f));
				component.Trigger(zero, breakHeight);
				break;
			}
			case MinorBreakableGroupBehavior.TRIGGERS_BREAK:
				if (breakForce == Vector2.zero)
				{
					minorBreakable.Break();
				}
				else
				{
					minorBreakable.Break(breakForce);
				}
				break;
			}
		}
		for (int j = 0; j < registeredDebris.Count; j++)
		{
			DebrisObject debrisObject = registeredDebris[j];
			switch (behavior)
			{
			case MinorBreakableGroupBehavior.TRIGGERS_DEBRIS:
				if (breakForce == Vector2.zero)
				{
					breakForce = Random.insideUnitCircle.normalized;
				}
				debrisObject.Trigger(breakForce.ToVector3ZUp(0.5f), breakHeight);
				j--;
				break;
			}
		}
		if (behavior != MinorBreakableGroupBehavior.NONE)
		{
			registeredBreakables.Clear();
		}
	}

	public void RegisterDebris(DebrisObject d)
	{
		if (!registeredDebris.Contains(d))
		{
			registeredDebris.Add(d);
		}
		d.groupManager = this;
	}

	public void DeregisterDebris(DebrisObject d)
	{
		d.groupManager = null;
		registeredDebris.Remove(d);
	}

	public void RegisterMinorBreakable(MinorBreakable mb)
	{
		if (!registeredBreakables.Contains(mb))
		{
			registeredBreakables.Add(mb);
		}
		mb.GroupManager = this;
	}

	public void DeregisterMinorBreakable(MinorBreakable mb)
	{
		mb.GroupManager = null;
		registeredBreakables.Remove(mb);
	}
}
