using UnityEngine;

public abstract class ProjectileAndBeamMotionModule : ProjectileMotionModule
{
	public abstract Vector2 GetBoneOffset(BasicBeamController.BeamBone bone, BeamController sourceBeam, bool inverted);
}
