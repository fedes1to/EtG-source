using System;
using UnityEngine;

[Serializable]
[ExecuteInEditMode]
[AddComponentMenu("Daikon Forge/User Interface/Resize Handle")]
public class dfResizeHandle : dfControl
{
	[Flags]
	public enum ResizeEdge
	{
		None = 0,
		Left = 1,
		Right = 2,
		Top = 4,
		Bottom = 8
	}

	[SerializeField]
	protected dfAtlas atlas;

	[SerializeField]
	protected string backgroundSprite = string.Empty;

	[SerializeField]
	protected ResizeEdge edges = ResizeEdge.Right | ResizeEdge.Bottom;

	private Vector3 mouseAnchorPos;

	private Vector3 startPosition;

	private Vector2 startSize;

	private Vector2 minEdgePos;

	private Vector2 maxEdgePos;

	public dfAtlas Atlas
	{
		get
		{
			if (atlas == null)
			{
				dfGUIManager manager = GetManager();
				if (manager != null)
				{
					return atlas = manager.DefaultAtlas;
				}
			}
			return atlas;
		}
		set
		{
			if (!dfAtlas.Equals(value, atlas))
			{
				atlas = value;
				Invalidate();
			}
		}
	}

	public string BackgroundSprite
	{
		get
		{
			return backgroundSprite;
		}
		set
		{
			if (value != backgroundSprite)
			{
				backgroundSprite = value;
				Invalidate();
			}
		}
	}

	public ResizeEdge Edges
	{
		get
		{
			return edges;
		}
		set
		{
			edges = value;
		}
	}

	public override void Start()
	{
		base.Start();
		if (base.Size.magnitude <= float.Epsilon)
		{
			base.Size = new Vector2(25f, 25f);
			if (base.Parent != null)
			{
				base.RelativePosition = base.Parent.Size - base.Size;
				base.Anchor = dfAnchorStyle.Bottom | dfAnchorStyle.Right;
			}
		}
	}

	protected override void OnRebuildRenderData()
	{
		if (Atlas == null || string.IsNullOrEmpty(backgroundSprite))
		{
			return;
		}
		dfAtlas.ItemInfo itemInfo = Atlas[backgroundSprite];
		if (!(itemInfo == null))
		{
			renderData.Material = Atlas.Material;
			Color32 color = ApplyOpacity((!base.IsEnabled) ? disabledColor : base.color);
			dfSprite.RenderOptions renderOptions = default(dfSprite.RenderOptions);
			renderOptions.atlas = atlas;
			renderOptions.color = color;
			renderOptions.fillAmount = 1f;
			renderOptions.flip = dfSpriteFlip.None;
			renderOptions.offset = pivot.TransformToUpperLeft(base.Size);
			renderOptions.pixelsToUnits = PixelsToUnits();
			renderOptions.size = base.Size;
			renderOptions.spriteInfo = itemInfo;
			dfSprite.RenderOptions options = renderOptions;
			if (itemInfo.border.horizontal == 0 && itemInfo.border.vertical == 0)
			{
				dfSprite.renderSprite(renderData, options);
			}
			else
			{
				dfSlicedSprite.renderSprite(renderData, options);
			}
		}
	}

	protected internal override void OnMouseDown(dfMouseEventArgs args)
	{
		args.Use();
		Plane plane = new Plane(parent.transform.TransformDirection(Vector3.back), parent.transform.position);
		Ray ray = args.Ray;
		float enter = 0f;
		plane.Raycast(args.Ray, out enter);
		mouseAnchorPos = ray.origin + ray.direction * enter;
		startSize = parent.Size;
		startPosition = parent.RelativePosition;
		minEdgePos = startPosition;
		maxEdgePos = (Vector2)startPosition + startSize;
		Vector2 vector = parent.CalculateMinimumSize();
		Vector2 vector2 = parent.MaximumSize;
		if (vector2.magnitude <= float.Epsilon)
		{
			vector2 = Vector2.one * 2048f;
		}
		if ((Edges & ResizeEdge.Left) == ResizeEdge.Left)
		{
			minEdgePos.x = maxEdgePos.x - vector2.x;
			maxEdgePos.x -= vector.x;
		}
		else if ((Edges & ResizeEdge.Right) == ResizeEdge.Right)
		{
			minEdgePos.x = startPosition.x + vector.x;
			maxEdgePos.x = startPosition.x + vector2.x;
		}
		if ((Edges & ResizeEdge.Top) == ResizeEdge.Top)
		{
			minEdgePos.y = maxEdgePos.y - vector2.y;
			maxEdgePos.y -= vector.y;
		}
		else if ((Edges & ResizeEdge.Bottom) == ResizeEdge.Bottom)
		{
			minEdgePos.y = startPosition.y + vector.y;
			maxEdgePos.y = startPosition.y + vector2.y;
		}
		base.OnMouseDown(args);
	}

	protected internal override void OnMouseMove(dfMouseEventArgs args)
	{
		if (args.Buttons.IsSet(dfMouseButtons.Left) && Edges != 0)
		{
			args.Use();
			Ray ray = args.Ray;
			float enter = 0f;
			Vector3 inNormal = GetCamera().transform.TransformDirection(Vector3.back);
			new Plane(inNormal, mouseAnchorPos).Raycast(ray, out enter);
			float num = PixelsToUnits();
			Vector3 vector = ray.origin + ray.direction * enter;
			Vector3 vector2 = (vector - mouseAnchorPos) / num;
			vector2.y *= -1f;
			float num2 = startPosition.x;
			float num3 = startPosition.y;
			float num4 = num2 + startSize.x;
			float num5 = num3 + startSize.y;
			if ((Edges & ResizeEdge.Left) == ResizeEdge.Left)
			{
				num2 = Mathf.Min(maxEdgePos.x, Mathf.Max(minEdgePos.x, num2 + vector2.x));
			}
			else if ((Edges & ResizeEdge.Right) == ResizeEdge.Right)
			{
				num4 = Mathf.Min(maxEdgePos.x, Mathf.Max(minEdgePos.x, num4 + vector2.x));
			}
			if ((Edges & ResizeEdge.Top) == ResizeEdge.Top)
			{
				num3 = Mathf.Min(maxEdgePos.y, Mathf.Max(minEdgePos.y, num3 + vector2.y));
			}
			else if ((Edges & ResizeEdge.Bottom) == ResizeEdge.Bottom)
			{
				num5 = Mathf.Min(maxEdgePos.y, Mathf.Max(minEdgePos.y, num5 + vector2.y));
			}
			parent.Size = new Vector2(num4 - num2, num5 - num3);
			parent.RelativePosition = new Vector3(num2, num3, 0f);
			if (parent.GetManager().PixelPerfectMode)
			{
				parent.MakePixelPerfect();
			}
		}
	}

	protected internal override void OnMouseUp(dfMouseEventArgs args)
	{
		base.Parent.MakePixelPerfect();
		args.Use();
		base.OnMouseUp(args);
	}
}
