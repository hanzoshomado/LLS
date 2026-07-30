using System;
using UnityEngine;

[RequireComponent(typeof(ITweenMover))]
public class TwoPositionMover : MonoBehaviour
{
	public Transform EndPosIndicator;

	public ITweenHash MoveToStartHash;

	public ITweenHash MoveToEndHash;

	private Vector3 _startPosition;

	private Vector3 _endPosition;

	private ITweenMover _iTweenMover;

	private void Awake()
	{
		_startPosition = base.transform.position;
		_endPosition = EndPosIndicator.position;
		_iTweenMover = GetComponent<ITweenMover>();
	}

	public void SetToStartPosition()
	{
		base.transform.position = _startPosition;
		_iTweenMover.StopAllTweens();
	}

	public void SetToEndPosition()
	{
		base.transform.position = _endPosition;
		_iTweenMover.StopAllTweens();
	}

	public void MoveToStartPosition(Action completeCallback = null)
	{
		_iTweenMover.MoveTo(_startPosition, MoveToStartHash, completeCallback);
	}

	public void MoveToEndPosition(Action completeCallback = null, ITweenHash overrideHash = null)
	{
		_iTweenMover.MoveTo(_endPosition, (overrideHash == null) ? MoveToEndHash : overrideHash, completeCallback);
	}
}
