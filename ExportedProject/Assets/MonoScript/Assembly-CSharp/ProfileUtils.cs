using System.Runtime.InteropServices;

public static class ProfileUtils
{
	public static int GetMonoCollectionCount()
	{
		return mono_gc_collection_count(0);
	}

	public static uint GetMonoHeapSize()
	{
		return (uint)mono_gc_get_heap_size();
	}

	public static uint GetMonoUsedHeapSize()
	{
		return (uint)mono_gc_get_used_size();
	}

	[DllImport("mono")]
	private static extern int mono_gc_collection_count(int generation);

	[DllImport("mono")]
	private static extern long mono_gc_get_heap_size();

	[DllImport("mono")]
	private static extern long mono_gc_get_used_size();
}
