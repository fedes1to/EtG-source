using System;
using DaikonForge.Tween;
using UnityEngine;

public static class TweenComponentExtensions
{
	public static Tween<float> TweenAlpha(this Component component)
	{
		if (component is TextMesh)
		{
			return ((TextMesh)component).TweenAlpha();
		}
		if (component is GUIText)
		{
			return ((GUIText)component).material.TweenAlpha();
		}
		if (component.GetComponent<Renderer>() is SpriteRenderer)
		{
			return ((SpriteRenderer)component.GetComponent<Renderer>()).TweenAlpha();
		}
		if (component.GetComponent<Renderer>() == null)
		{
			throw new NullReferenceException("Component does not have a Renderer assigned");
		}
		Material material = component.GetComponent<Renderer>().material;
		if (material == null)
		{
			throw new NullReferenceException("Component does not have a Material assigned");
		}
		return material.TweenAlpha();
	}

	public static Tween<Color> TweenColor(this Component component)
	{
		if (component is TextMesh)
		{
			return ((TextMesh)component).TweenColor();
		}
		if (component is GUIText)
		{
			return ((GUIText)component).material.TweenColor();
		}
		if (component.GetComponent<Renderer>() is SpriteRenderer)
		{
			return ((SpriteRenderer)component.GetComponent<Renderer>()).TweenColor();
		}
		if (component.GetComponent<Renderer>() == null)
		{
			throw new NullReferenceException("Component does not have a Renderer assigned");
		}
		Material material = component.GetComponent<Renderer>().material;
		if (material == null)
		{
			throw new NullReferenceException("Component does not have a Material assigned");
		}
		return material.TweenColor();
	}

	public static Tween<float> TweenPath(this Component component, IPathIterator path)
	{
		return component.TweenPath(path, true);
	}

	public static Tween<float> TweenPath(this Component component, IPathIterator path, bool orientToPath)
	{
		if (component == null)
		{
			throw new ArgumentNullException("component");
		}
		return component.transform.TweenPath(path);
	}

	public static TweenShake ShakePosition(this Component component)
	{
		return component.transform.ShakePosition();
	}

	public static TweenShake ShakePosition(this Component component, bool localPosition)
	{
		return component.transform.ShakePosition(localPosition);
	}

	public static Tween<Vector3> TweenScaleFrom(this Component component, Vector3 startScale)
	{
		return component.TweenScale().SetStartValue(startScale);
	}

	public static Tween<Vector3> TweenScaleTo(this Component component, Vector3 endScale)
	{
		return component.transform.TweenScale().SetEndValue(endScale);
	}

	public static Tween<Vector3> TweenScale(this Component component)
	{
		return component.transform.TweenScale();
	}

	public static Tween<Vector3> TweenRotateFrom(this Component component, Vector3 startRotation)
	{
		return component.TweenRotateFrom(startRotation, true, false);
	}

	public static Tween<Vector3> TweenRotateFrom(this Component component, Vector3 startRotation, bool useShortestPath)
	{
		return component.TweenRotateFrom(startRotation, useShortestPath, false);
	}

	public static Tween<Vector3> TweenRotateFrom(this Component component, Vector3 startRotation, bool useShortestPath, bool useLocalRotation)
	{
		return TweenRotation(component.transform, useShortestPath, useLocalRotation).SetStartValue(startRotation);
	}

	public static Tween<Vector3> TweenRotateTo(this Component component, Vector3 endRotation)
	{
		return component.TweenRotateTo(endRotation, true, false);
	}

	public static Tween<Vector3> TweenRotateTo(this Component component, Vector3 endRotation, bool useShortestPath)
	{
		return component.TweenRotateTo(endRotation, useShortestPath, false);
	}

	public static Tween<Vector3> TweenRotateTo(this Component component, Vector3 endRotation, bool useShortestPath, bool useLocalRotation)
	{
		return TweenRotation(component.transform, useShortestPath, useLocalRotation).SetEndValue(endRotation);
	}

	public static Tween<Vector3> TweenRotation(this Component component)
	{
		return component.transform.TweenRotation(true, false);
	}

	public static Tween<Vector3> TweenRotation(this Component component, bool useShortestPath)
	{
		return component.transform.TweenRotation(useShortestPath, false);
	}

	public static Tween<Vector3> TweenRotation(this Component component, bool useShortestPath, bool useLocalRotation)
	{
		return component.transform.TweenRotation(useShortestPath, useLocalRotation);
	}

	public static Tween<Vector3> TweenMoveFrom(this Component component, Vector3 startPosition)
	{
		return component.TweenMoveFrom(startPosition, false);
	}

	public static Tween<Vector3> TweenMoveFrom(this Component component, Vector3 startPosition, bool useLocalPosition)
	{
		return component.TweenPosition(useLocalPosition).SetStartValue(startPosition);
	}

	public static Tween<Vector3> TweenMoveTo(this Component component, Vector3 endPosition)
	{
		return component.TweenMoveTo(endPosition, false);
	}

	public static Tween<Vector3> TweenMoveTo(this Component component, Vector3 endPosition, bool useLocalPosition)
	{
		return component.TweenPosition(useLocalPosition).SetEndValue(endPosition);
	}

	public static Tween<Vector3> TweenPosition(this Component component)
	{
		return component.transform.TweenPosition(false);
	}

	public static Tween<Vector3> TweenPosition(this Component component, bool useLocalPosition)
	{
		return component.transform.TweenPosition(useLocalPosition);
	}
}
