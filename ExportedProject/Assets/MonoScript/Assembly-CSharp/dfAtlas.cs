using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/User Interface/Texture Atlas")]
[ExecuteInEditMode]
public class dfAtlas : MonoBehaviour
{
	public enum TextureAtlasGenerator
	{
		Internal,
		TexturePacker
	}

	[Serializable]
	public class ItemInfo : IComparable<ItemInfo>, IEquatable<ItemInfo>
	{
		public string name;

		public Rect region;

		public RectOffset border = new RectOffset();

		public bool rotated;

		public Vector2 sizeInPixels = Vector2.zero;

		[SerializeField]
		public string textureGUID = string.Empty;

		public bool deleted;

		[SerializeField]
		public Texture2D texture;

		public int CompareTo(ItemInfo other)
		{
			return name.CompareTo(other.name);
		}

		public override int GetHashCode()
		{
			return name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ItemInfo))
			{
				return false;
			}
			return name.Equals(((ItemInfo)obj).name);
		}

		public bool Equals(ItemInfo other)
		{
			return name.Equals(other.name);
		}

		public static bool operator ==(ItemInfo lhs, ItemInfo rhs)
		{
			if (object.ReferenceEquals(lhs, rhs))
			{
				return true;
			}
			if ((object)lhs == null || (object)rhs == null)
			{
				return false;
			}
			return lhs.name.Equals(rhs.name);
		}

		public static bool operator !=(ItemInfo lhs, ItemInfo rhs)
		{
			return !(lhs == rhs);
		}
	}

	[SerializeField]
	protected Material material;

	[SerializeField]
	protected List<ItemInfo> items = new List<ItemInfo>();

	public TextureAtlasGenerator generator;

	public string imageFileGUID;

	public string dataFileGUID;

	private Dictionary<string, ItemInfo> map = new Dictionary<string, ItemInfo>();

	private dfAtlas replacementAtlas;

	public Texture2D Texture
	{
		get
		{
			return (!(replacementAtlas != null)) ? (material.mainTexture as Texture2D) : replacementAtlas.Texture;
		}
	}

	public int Count
	{
		get
		{
			return (!(replacementAtlas != null)) ? items.Count : replacementAtlas.Count;
		}
	}

	public List<ItemInfo> Items
	{
		get
		{
			return (!(replacementAtlas != null)) ? items : replacementAtlas.Items;
		}
	}

	public Material Material
	{
		get
		{
			return (!(replacementAtlas != null)) ? material : replacementAtlas.Material;
		}
		set
		{
			if (replacementAtlas != null)
			{
				replacementAtlas.Material = value;
			}
			else
			{
				material = value;
			}
		}
	}

	public dfAtlas Replacement
	{
		get
		{
			return replacementAtlas;
		}
		set
		{
			replacementAtlas = value;
		}
	}

	public ItemInfo this[string key]
	{
		get
		{
			if (replacementAtlas != null)
			{
				return replacementAtlas[key];
			}
			if (string.IsNullOrEmpty(key))
			{
				return null;
			}
			if (map.Count == 0)
			{
				RebuildIndexes();
			}
			ItemInfo value = null;
			if (map.TryGetValue(key, out value))
			{
				return value;
			}
			return null;
		}
	}

	internal static bool Equals(dfAtlas lhs, dfAtlas rhs)
	{
		if (object.ReferenceEquals(lhs, rhs))
		{
			return true;
		}
		if (lhs == null || rhs == null)
		{
			return false;
		}
		return lhs.material == rhs.material;
	}

	public void AddItem(ItemInfo item)
	{
		items.Add(item);
		RebuildIndexes();
	}

	public void AddItems(IEnumerable<ItemInfo> list)
	{
		items.AddRange(list);
		RebuildIndexes();
	}

	public void Remove(string name)
	{
		for (int num = items.Count - 1; num >= 0; num--)
		{
			if (items[num].name == name)
			{
				items.RemoveAt(num);
			}
		}
		RebuildIndexes();
	}

	public void RebuildIndexes()
	{
		if (map == null)
		{
			map = new Dictionary<string, ItemInfo>();
		}
		else
		{
			map.Clear();
		}
		for (int i = 0; i < items.Count; i++)
		{
			ItemInfo itemInfo = items[i];
			map[itemInfo.name] = itemInfo;
		}
	}
}
