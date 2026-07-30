using System.Collections.Generic;
using Bolt;
using UnityEngine;

public class CharacterTracker : GlobalEventListener
{
	private List<SantaCharacterController> _characters;

	private static CharacterTracker _instance;

	public static CharacterTracker Instance
	{
		get
		{
			return _instance;
		}
	}

	public void Awake()
	{
		_instance = this;
		_characters = new List<SantaCharacterController>();
	}

	public override void EntityAttached(BoltEntity entity)
	{
		SantaCharacterController component = entity.GetComponent<SantaCharacterController>();
		if (component != null)
		{
			_characters.Add(component);
		}
	}

	public override void EntityDetached(BoltEntity entity)
	{
		SantaCharacterController component = entity.GetComponent<SantaCharacterController>();
		if (component != null)
		{
			_characters.Remove(component);
		}
	}

	public Vector3 GetOverlappingCharacters(SantaCharacterController santaCharacterController, Vector3 potentialNewPosition)
	{
		Vector3 result = potentialNewPosition;
		for (int i = 0; i < _characters.Count; i++)
		{
			SantaCharacterController santaCharacterController2 = _characters[i];
			if (santaCharacterController2 != null && !santaCharacterController2.IsDetached() && santaCharacterController2 != santaCharacterController && santaCharacterController2.IsAlive())
			{
				Vector3 vector = santaCharacterController2.transform.position - potentialNewPosition;
				float magnitude = vector.magnitude;
				if (magnitude <= santaCharacterController2.CharacterCollisionRadius + santaCharacterController.CharacterCollisionRadius)
				{
					float num = santaCharacterController.CharacterCollisionRadius + santaCharacterController2.CharacterCollisionRadius - magnitude;
					result -= num * vector.normalized;
				}
			}
		}
		return result;
	}

	public SantaCharacterController GetCharacterWithConnection(BoltConnection connection)
	{
		for (int i = 0; i < _characters.Count; i++)
		{
			if (_characters[i].IsControlledBy(connection))
			{
				return _characters[i];
			}
		}
		return null;
	}

	public SantaCharacterController GetAliveCharacterWithConnection(BoltConnection connection)
	{
		for (int i = 0; i < _characters.Count; i++)
		{
			if (_characters[i].IsAlive() && _characters[i].IsControlledBy(connection))
			{
				return _characters[i];
			}
		}
		return null;
	}

	public int GetNumberOfLivingCharacters()
	{
		int num = 0;
		for (int i = 0; i < _characters.Count; i++)
		{
			if (_characters[i].IsAlive())
			{
				num++;
			}
		}
		return num;
	}

	public List<SantaCharacterController> GetAllLivingCharacters()
	{
		List<SantaCharacterController> list = new List<SantaCharacterController>();
		for (int i = 0; i < _characters.Count; i++)
		{
			if (_characters[i].IsAlive())
			{
				list.Add(_characters[i]);
			}
		}
		return list;
	}

	public SantaCharacterController GetDeadCharacterWithConnection(BoltConnection connection)
	{
		for (int i = 0; i < _characters.Count; i++)
		{
			if (!_characters[i].IsAlive() && _characters[i].IsControlledBy(connection))
			{
				return _characters[i];
			}
		}
		return null;
	}

	public bool OwnAnyCharacter(BoltConnection connection)
	{
		for (int i = 0; i < _characters.Count; i++)
		{
			if (_characters[i].IsOwnedBy(connection))
			{
				return true;
			}
		}
		return false;
	}

	public SantaCharacterController GetSantaWithControl()
	{
		for (int i = 0; i < _characters.Count; i++)
		{
			if (_characters[i].IsAlive() && _characters[i].HasBeenUnderLocalControl())
			{
				return _characters[i];
			}
		}
		return null;
	}
}
