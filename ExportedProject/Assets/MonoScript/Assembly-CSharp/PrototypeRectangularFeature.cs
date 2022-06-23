using System;

[Serializable]
public struct PrototypeRectangularFeature
{
	public IntVector2 basePosition;

	public IntVector2 dimensions;

	public static PrototypeRectangularFeature CreateMirror(PrototypeRectangularFeature source, IntVector2 roomDimensions)
	{
		PrototypeRectangularFeature result = default(PrototypeRectangularFeature);
		result.dimensions = source.dimensions;
		result.basePosition = source.basePosition;
		result.basePosition.x = roomDimensions.x - (result.basePosition.x + result.dimensions.x);
		return result;
	}
}
