using Bolt;
using UnityEngine;

public class SantaCharacterNameTagManager : GlobalEventListener
{
	public PooledPrefab CharacterNameTagPool;

	private static SantaCharacterNameTagManager _instance;

	public static SantaCharacterNameTagManager Instance
	{
		get
		{
			return _instance;
		}
	}

	public void Awake()
	{
		_instance = this;
	}

	public void CreateNameTagFor(SantaCharacterController character)
	{
		Transform transform = CharacterNameTagPool.InstantiateNewObject();
		EnemyNameTagUI component = transform.GetComponent<EnemyNameTagUI>();
		component.SetCharacterAndFollow(character);
		transform.SetParent(Singleton<UIRoot>.Instance.transform, false);
		transform.SetAsFirstSibling();
	}

	public override void EntityDetached(BoltEntity entity)
	{
		SantaCharacterController component = entity.GetComponent<SantaCharacterController>();
		if (!(component != null))
		{
		}
	}
}
