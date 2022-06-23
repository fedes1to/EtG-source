using System;

[Serializable]
public enum EdgeDetectMode
{
	TriangleDepthNormals = 0,
	RobertsCrossDepthNormals = 1,
	SobelDepth = 2,
	SobelDepthThin = 3
}
