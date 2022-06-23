using System;

[Serializable]
public class BulletScriptSettings
{
	public bool overrideMotion;

	public bool preventPooling;

	public bool surviveRigidbodyCollisions;

	public bool surviveTileCollisions;

	public BulletScriptSettings()
	{
	}

	public BulletScriptSettings(BulletScriptSettings other)
	{
		SetAll(other);
	}

	public void SetAll(BulletScriptSettings other)
	{
		overrideMotion = other.overrideMotion;
		preventPooling = other.preventPooling;
		surviveRigidbodyCollisions = other.surviveRigidbodyCollisions;
		surviveTileCollisions = other.surviveTileCollisions;
	}
}
