using System;
using UnityEngine;

public class PostShootProjectileModifier : MonoBehaviour
{
	public int NumberBouncesToSet;

	private void Start()
	{
		Gun component = GetComponent<Gun>();
		component.PostProcessProjectile = (Action<Projectile>)Delegate.Combine(component.PostProcessProjectile, new Action<Projectile>(PostProcessProjectile));
	}

	private void PostProcessProjectile(Projectile obj)
	{
		BounceProjModifier component = obj.GetComponent<BounceProjModifier>();
		if ((bool)component)
		{
			component.numberOfBounces = NumberBouncesToSet;
		}
	}
}
