using FullInspector;

[InspectorDropdownName("BulletSkeleton/DoubleShotHard1")]
public class BulletSkeletonDoubleShotHard1 : BulletSkeletonDoubleShot1
{
	protected override bool IsHard
	{
		get
		{
			return true;
		}
	}
}
