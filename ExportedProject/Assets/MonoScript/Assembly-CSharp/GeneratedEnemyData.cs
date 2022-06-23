public struct GeneratedEnemyData
{
	public string enemyGuid;

	public float percentOfEnemies;

	public bool isSignatureEnemy;

	public GeneratedEnemyData(string id, float percent, bool isSig)
	{
		enemyGuid = id;
		percentOfEnemies = percent;
		isSignatureEnemy = isSig;
	}
}
