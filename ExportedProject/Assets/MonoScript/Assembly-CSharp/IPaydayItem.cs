public interface IPaydayItem
{
	void StoreData(string id1, string id2, string id3);

	string GetID(int placement);

	bool HasCachedData();
}
