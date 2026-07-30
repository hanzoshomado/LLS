using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class nSlice_Plane : MonoBehaviour
{
	private Mesh planeMesh;

	private int maxRows;

	public int maxColumns;

	private int maxColumnsX;

	public int maxColumnsZ;

	private List<Vector3> VertexList;

	private List<Vector2> UV_List;

	private List<int> TriangleList;

	private float _uMargin = 0.21881837f;

	private float _vMargin = 0.21881837f;

	private float _margin = 1.4004376f;

	private float _xBase = 6.4f;

	public float _yBase = 6.4f;

	private float _zBase = 6.4f;

	private float _xScale;

	private float _yScale;

	private float _zScale;

	public float _collapseThreshold = 3f;

	public float _marginSetting = 4.57f;

	private float _lastMarginSetting;

	private Vector3 _lossyScale;

	private Vector3 _lastLossyScale;

	private bool checkLevelEditor;

	public bool I_AmEdge;

	public bool I_AmCorner;

	public GameObject Parent;

	private bool isPlaying;

	private IEnumerator runInEditor_Coroutine;

	private void OnEnable()
	{
		_lossyScale = base.transform.lossyScale;
		_lastLossyScale = Vector3.zero;
		CreatePlatform();
		isPlaying = false;
		runInEditor_Coroutine = runInEditor_IEnumerator(0.05f);
		StartCoroutine(runInEditor_Coroutine);
	}

	private IEnumerator runInEditor_IEnumerator(float delay)
	{
		WaitForSeconds wait = new WaitForSeconds(delay);
		int counter = 0;
		while (!isPlaying)
		{
			if (counter > 3)
			{
				isPlaying = true;
			}
			counter++;
			yield return wait;
		}
	}

	private float calculateCeiling(float num)
	{
		int num2 = (int)num;
		return (float)num2 + 1f;
	}

	private void Update()
	{
		if (isPlaying)
		{
			return;
		}
		if (base.transform.parent == Parent)
		{
			_lossyScale = base.transform.parent.transform.lossyScale;
		}
		else
		{
			_lossyScale = base.transform.lossyScale;
		}
		if (_lastLossyScale != _lossyScale || planeMesh.vertices.Length == 0)
		{
			if (_lastMarginSetting != _marginSetting)
			{
				_margin = 6.4f / _marginSetting;
				_vMargin = 1f / _marginSetting;
				_uMargin = 1f / _marginSetting;
				_lastMarginSetting = _marginSetting;
			}
			CreatePlatform();
			_lastLossyScale = _lossyScale;
		}
	}

	private void ClearMesh()
	{
		VertexList = new List<Vector3>();
		UV_List = new List<Vector2>();
		TriangleList = new List<int>();
		planeMesh = GetComponent<MeshFilter>().sharedMesh;
		if ((bool)planeMesh)
		{
			planeMesh.Clear();
		}
		planeMesh = new Mesh();
		GetComponent<MeshFilter>().mesh = planeMesh;
	}

	private void CheckSize()
	{
		maxRows = 4;
		maxColumnsX = 4;
		maxColumnsZ = 4;
		if (_xBase < 2f / _collapseThreshold / _lossyScale.x * 6.4f)
		{
			maxColumnsX = 3;
			if (_xBase < 2f / _marginSetting / _lossyScale.x * 6.4f)
			{
				maxColumnsX = 2;
			}
		}
		if (_yBase < 2f / _collapseThreshold / _lossyScale.y * 6.4f)
		{
			maxRows = 3;
			if (_yBase < 2f / _marginSetting / _lossyScale.y * 6.4f)
			{
				maxRows = 2;
			}
		}
		if (_zBase < 2f / _collapseThreshold / _lossyScale.z * 6.4f)
		{
			maxColumnsZ = 3;
			if (_zBase < 2f / _marginSetting / _lossyScale.z * 6.4f)
			{
				maxColumnsZ = 2;
			}
		}
	}

	public void CreatePlatform()
	{
		ClearMesh();
		CheckSize();
		_xScale = _lossyScale.x;
		_yScale = _lossyScale.y;
		_zScale = _lossyScale.z;
		CalculateVertex();
		CalculateUV();
		CalculateTriangle();
		planeMesh.vertices = VertexList.ToArray();
		planeMesh.uv = UV_List.ToArray();
		planeMesh.triangles = TriangleList.ToArray();
		planeMesh.RecalculateNormals();
		planeMesh.RecalculateBounds();
	}

	private void CalculateMesh()
	{
		if (maxColumns == 4 && maxRows == 4)
		{
			if (I_AmEdge)
			{
			}
			if (!I_AmCorner)
			{
			}
		}
		if (maxColumns == 3)
		{
			if (I_AmEdge)
			{
			}
			if (!I_AmCorner)
			{
			}
		}
		if (maxRows == 3)
		{
			if (I_AmEdge)
			{
			}
			if (!I_AmCorner)
			{
			}
		}
		if ((maxRows != 2 && maxColumns != 2) || I_AmEdge || !I_AmCorner)
		{
		}
		planeMesh.vertices = new Vector3[4]
		{
			new Vector3(0f, 0f, _zBase),
			new Vector3(_xBase, 0f, _zBase),
			new Vector3(0f, 0f, 0f),
			new Vector3(_xBase, 0f, 0f)
		};
		planeMesh.uv = new Vector2[16]
		{
			new Vector2(0f, _zScale),
			new Vector2(_xScale, _zScale),
			new Vector2(0f, 0f),
			new Vector2(_xScale, 0f),
			new Vector2(0f, _yScale),
			new Vector2(0f, 0f),
			new Vector2(_xScale, 0f),
			new Vector2(_xScale, _zScale),
			new Vector2(0f, 0f),
			new Vector2(_xScale, 0f),
			new Vector2(0f, _yScale),
			new Vector2(_zScale, 0f),
			new Vector2(0f, 0f),
			new Vector2(0f, _xScale),
			new Vector2(0f, 0f),
			new Vector2(_zScale, 0f)
		};
		if (I_AmCorner)
		{
			planeMesh.triangles = new int[18]
			{
				0, 2, 3, 0, 3, 1, 4, 6, 5, 7,
				9, 8, 10, 12, 11, 13, 15, 14
			};
		}
		else if (I_AmEdge)
		{
			planeMesh.triangles = new int[12]
			{
				0, 2, 3, 0, 3, 1, 4, 6, 5, 10,
				12, 11
			};
		}
		else
		{
			planeMesh.triangles = new int[6] { 7, 9, 8, 13, 15, 14 };
		}
	}

	private void CalculateVertex()
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		num2 = _yBase;
		for (int i = 0; i < maxColumnsZ; i++)
		{
			float zBase = _zBase;
			float z = _lossyScale.z;
			float num4 = zBase;
			switch (i)
			{
			case 1:
				num4 = ((maxColumnsZ != 3) ? (zBase - _margin / z) : (zBase * 0.5f));
				break;
			case 2:
				num4 = _margin / z;
				break;
			}
			if (i == maxColumnsZ - 1)
			{
				num4 = 0f;
			}
			num3 = num4;
			float num5 = 0f;
			float num6 = 1f;
			maxColumns = maxColumnsX;
			float xBase = _xBase;
			num6 = _lossyScale.x;
			for (int j = 0; j < maxColumns; j++)
			{
				switch (j)
				{
				case 1:
					num5 = ((maxColumns != 3) ? (_margin / num6) : (xBase * 0.5f));
					break;
				case 2:
					num5 = xBase - _margin / num6;
					break;
				}
				if (j == maxColumns - 1)
				{
					num5 = xBase;
				}
				num = num5;
				VertexList.Add(new Vector3(num, num2, num3));
			}
		}
	}

	private void CalculateUV()
	{
		float x = 1f;
		float y = 1f;
		maxColumns = maxColumnsX;
		float x2 = _lossyScale.x;
		float z = _lossyScale.z;
		float num = _lossyScale.y * (_yBase / _margin) * _vMargin;
		float num2 = _vMargin + _vMargin * (1f - z % 1f) / z;
		float num3 = _vMargin + _vMargin * (1f - num % 1f) / num;
		float num4 = _uMargin + _uMargin * (1f - z % 1f) / z;
		float num5 = _uMargin + _uMargin * (1f - x2 % 1f) / x2;
		for (int i = 0; i < maxColumnsZ; i++)
		{
			switch (i)
			{
			case 0:
				y = calculateCeiling(z);
				if (maxColumnsZ == 2)
				{
					y = _vMargin - 0.05f;
				}
				break;
			case 1:
				y = ((maxColumnsZ != 3) ? (calculateCeiling(z) - num2) : 0.5f);
				break;
			case 2:
				y = num2;
				break;
			}
			if (i == maxColumnsZ - 1)
			{
				y = 0f;
			}
			for (int j = 0; j < maxColumns; j++)
			{
				switch (j)
				{
				case 0:
					x = 0f;
					break;
				case 1:
					x = ((maxColumns != 3) ? num5 : 0.5f);
					break;
				case 2:
					x = calculateCeiling(x2) - num5;
					break;
				}
				if (j == maxColumns - 1)
				{
					x = calculateCeiling(x2);
					if (maxColumns < 3)
					{
						x = _uMargin - 0.05f;
					}
				}
				UV_List.Add(new Vector2(x, y));
			}
		}
	}

	private void CalculateTriangle()
	{
		int num = 0;
		for (int i = 0; i < maxColumnsZ - 1; i++)
		{
			maxColumns = maxColumnsX;
			for (int j = 0; j < maxColumns - 1; j++)
			{
				int num2 = num + i * maxColumns + j;
				int item = num2;
				int item2 = num2 + maxColumns + 1;
				int item3 = num2 + maxColumns;
				int item4 = num2;
				int item5 = num2 + 1;
				int item6 = num2 + maxColumns + 1;
				if (I_AmEdge)
				{
					if (maxColumns == 4 && maxColumnsZ == 4 && ((j == 1 && i == 0) || (j == 1 && i == 2) || (j == 0 && i == 1) || (j == 2 && i == 1)))
					{
						TriangleList.Add(item);
						TriangleList.Add(item2);
						TriangleList.Add(item3);
						TriangleList.Add(item4);
						TriangleList.Add(item5);
						TriangleList.Add(item6);
					}
				}
				else if (I_AmCorner)
				{
					if (maxColumns == 4 && maxColumnsZ == 4 && ((j == 0 && i == 0) || (j == 0 && i == 2) || (j == 2 && i == 0) || (j == 2 && i == 2)))
					{
						TriangleList.Add(item);
						TriangleList.Add(item2);
						TriangleList.Add(item3);
						TriangleList.Add(item4);
						TriangleList.Add(item5);
						TriangleList.Add(item6);
					}
				}
				else if (maxColumns != 4 || maxColumnsZ != 4)
				{
					TriangleList.Add(item);
					TriangleList.Add(item2);
					TriangleList.Add(item3);
					TriangleList.Add(item4);
					TriangleList.Add(item5);
					TriangleList.Add(item6);
				}
				else if (j == 1 && i == 1)
				{
					TriangleList.Add(item);
					TriangleList.Add(item2);
					TriangleList.Add(item3);
					TriangleList.Add(item4);
					TriangleList.Add(item5);
					TriangleList.Add(item6);
				}
			}
		}
		num += maxColumnsZ * maxColumns;
	}
}
