using DaikonForge.Tween;
using UnityEngine;

public static class Tween
{
	public static Tween<Color> Color(SpriteRenderer renderer)
	{
		return renderer.TweenColor();
	}

	public static Tween<float> Alpha(SpriteRenderer renderer)
	{
		return renderer.TweenAlpha();
	}

	public static Tween<Color> Color(Material material)
	{
		return material.TweenColor();
	}

	public static Tween<float> Alpha(Material material)
	{
		return material.TweenAlpha();
	}

	public static Tween<Vector3> Position(Transform transform)
	{
		return Position(transform, false);
	}

	public static Tween<Vector3> Position(Transform transform, bool useLocalPosition)
	{
		return transform.TweenPosition(useLocalPosition);
	}

	public static Tween<Vector3> MoveTo(Transform transform, Vector3 endPosition)
	{
		return MoveTo(transform, endPosition, false);
	}

	public static Tween<Vector3> MoveTo(Transform transform, Vector3 endPosition, bool useLocalPosition)
	{
		return transform.TweenMoveTo(endPosition, useLocalPosition);
	}

	public static Tween<Vector3> MoveFrom(Transform transform, Vector3 startPosition)
	{
		return transform.TweenMoveFrom(startPosition, false);
	}

	public static Tween<Vector3> MoveFrom(Transform transform, Vector3 startPosition, bool useLocalPosition)
	{
		return transform.TweenMoveFrom(startPosition, useLocalPosition);
	}

	public static Tween<Vector3> RotateFrom(Transform transform, Vector3 startRotation)
	{
		return RotateFrom(transform, startRotation, true, false);
	}

	public static Tween<Vector3> RotateFrom(Transform transform, Vector3 startRotation, bool useShortestPath)
	{
		return RotateFrom(transform, startRotation, useShortestPath, false);
	}

	public static Tween<Vector3> RotateFrom(Transform transform, Vector3 startRotation, bool useShortestPath, bool useLocalRotation)
	{
		return transform.TweenRotateFrom(startRotation, useShortestPath, useLocalRotation);
	}

	public static Tween<Vector3> RotateTo(Transform transform, Vector3 endRotation)
	{
		return RotateTo(transform, endRotation, true, false);
	}

	public static Tween<Vector3> RotateTo(Transform transform, Vector3 endRotation, bool useShortestPath)
	{
		return RotateTo(transform, endRotation, useShortestPath, false);
	}

	public static Tween<Vector3> RotateTo(Transform transform, Vector3 endRotation, bool useShortestPath, bool useLocalRotation)
	{
		return transform.TweenRotateTo(endRotation, useShortestPath, useLocalRotation);
	}

	public static Tween<Vector3> Rotation(Transform transform)
	{
		return transform.TweenRotation();
	}

	public static Tween<Vector3> Rotation(Transform transform, bool useShortestPath)
	{
		return Rotation(transform, useShortestPath, false);
	}

	public static Tween<Vector3> Rotation(Transform transform, bool useShortestPath, bool useLocalRotation)
	{
		return transform.TweenRotation(useShortestPath, useLocalRotation);
	}

	public static Tween<Vector3> ScaleFrom(Transform transform, Vector3 startScale)
	{
		return Scale(transform).SetStartValue(startScale);
	}

	public static Tween<Vector3> ScaleTo(Transform transform, Vector3 endScale)
	{
		return Scale(transform).SetEndValue(endScale);
	}

	public static Tween<Vector3> Scale(Transform transform)
	{
		return transform.TweenScale();
	}

	public static TweenShake Shake(Transform transform)
	{
		return Shake(transform, false);
	}

	public static TweenShake Shake(Transform transform, bool localPosition)
	{
		return transform.ShakePosition(localPosition);
	}

	public static Tween<T> NamedProperty<T>(object target, string propertyName)
	{
		return TweenNamedProperty<T>.Obtain(target, propertyName);
	}
}
