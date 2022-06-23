using UnityEngine;

public class TarnisherController : BraveBehaviour
{
	public void Awake()
	{
		base.healthHaver.OnPreDeath += OnPreDeath;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		base.healthHaver.OnPreDeath -= OnPreDeath;
	}

	private void OnPreDeath(Vector2 vector2)
	{
		base.aiAnimator.OtherAnimations.Find((AIAnimator.NamedDirectionalAnimation a) => a.name == "pitfall").anim.Prefix = "pitfall_dead";
	}
}
