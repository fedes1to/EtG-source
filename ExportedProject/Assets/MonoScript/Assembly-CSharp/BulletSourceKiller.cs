using UnityEngine;

public class BulletSourceKiller : BraveBehaviour
{
	public BulletScriptSource BraveSource;

	public SpeculativeRigidbody TrackRigidbody;

	public void Start()
	{
		if (!BraveSource)
		{
			BraveSource = GetComponent<BulletScriptSource>();
		}
	}

	public void Update()
	{
		if ((bool)TrackRigidbody)
		{
			base.transform.position = TrackRigidbody.GetUnitCenter(ColliderType.HitBox);
		}
		if ((bool)BraveSource && BraveSource.IsEnded)
		{
			Object.Destroy(base.gameObject);
		}
		if (!BraveSource)
		{
			Object.Destroy(base.gameObject);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
