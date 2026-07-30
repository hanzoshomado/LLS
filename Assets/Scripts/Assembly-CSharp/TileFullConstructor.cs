using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TileFullConstructor : MonoBehaviour
{
	private Mesh tileMesh;

	private int maxRows;

	private int maxColumns;

	private int maxColumnsX;

	private int maxColumnsZ;

	private List<Vector3> VertexList;

	private List<Vector2> UV_List;

	private List<int> TriangleList;

	private float _uMargin = 0.21881837f;

	private float _vMargin = 0.21881837f;

	private float _margin = 1.4004376f;

	private float _xBase = 6.4f;

	public float _yBase = 6.4f;

	private float _zBase = 6.4f;

	public float _collapseThreshold = 3f;

	public float _marginSetting = 4.57f;

	private float _lastMarginSetting;

	private Vector3 _lossyScale;

	private Vector3 _lastLossyScale;

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
		_lossyScale = base.transform.lossyScale;
		if (_lastLossyScale != _lossyScale || tileMesh.vertices.Length == 0)
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
		tileMesh = GetComponent<MeshFilter>().sharedMesh;
		if ((bool)tileMesh)
		{
			tileMesh.Clear();
		}
		tileMesh = new Mesh();
		GetComponent<MeshFilter>().mesh = tileMesh;
	}

	private void CheckSize()
	{
		maxRows = 4;
		maxColumnsX = 4;
		maxColumnsZ = 4;
		if (_yBase < 2f / _collapseThreshold / _lossyScale.y * 6.4f)
		{
			maxRows = 3;
			if (_yBase < 2f / _marginSetting / _lossyScale.y * 6.4f)
			{
				maxRows = 2;
			}
		}
		if (_xBase < 2f / _collapseThreshold / _lossyScale.x * 6.4f)
		{
			maxColumnsX = 3;
			if (_xBase < 2f / _marginSetting / _lossyScale.x * 6.4f)
			{
				maxColumnsX = 2;
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
		CalculateVertex();
		CalculateUV();
		CalculateTriangle();
		tileMesh.vertices = VertexList.ToArray();
		tileMesh.uv = UV_List.ToArray();
		tileMesh.triangles = TriangleList.ToArray();
		tileMesh.RecalculateNormals();
		tileMesh.RecalculateBounds();
	}

	private void CalculateVertex()
	{
		for (int i = 0; i < 6; i++)
		{
			float x = 0f;
			float y = 0f;
			float z = 0f;
			switch (i)
			{
			case 1:
				y = _yBase;
				break;
			case 3:
				z = _zBase;
				break;
			case 5:
				x = _xBase;
				break;
			}
			for (int j = 0; (i >= 2) ? (j < maxRows) : (j < maxColumnsZ); j++)
			{
				float num;
				float num2;
				if (i < 2)
				{
					num = _zBase;
					num2 = _lossyScale.z;
				}
				else
				{
					num = _yBase;
					num2 = _lossyScale.y;
				}
				float num3 = num;
				switch (j)
				{
				case 1:
					num3 = (((i >= 2 || maxColumnsZ != 3) && (i <= 1 || maxRows != 3)) ? (num - _margin / num2) : (num * 0.5f));
					break;
				case 2:
					num3 = _margin / num2;
					break;
				}
				if ((i >= 2) ? (j == maxRows - 1) : (j == maxColumnsZ - 1))
				{
					num3 = 0f;
				}
				if (i < 2)
				{
					z = num3;
				}
				else
				{
					y = num3;
				}
				float num4 = 0f;
				float num5 = 1f;
				float num6;
				if (i < 4)
				{
					maxColumns = maxColumnsX;
					num6 = _xBase;
					num5 = _lossyScale.x;
				}
				else
				{
					maxColumns = maxColumnsZ;
					num6 = _zBase;
					num5 = _lossyScale.z;
				}
				for (int k = 0; k < maxColumns; k++)
				{
					if (i == 3 || i == 4)
					{
						switch (k)
						{
						case 0:
							num4 = num6;
							break;
						case 1:
							num4 = ((maxColumns != 3) ? (num6 - _margin / num5) : (num6 * 0.5f));
							break;
						case 2:
							num4 = _margin / num5;
							break;
						}
						if (k == maxColumns - 1)
						{
							num4 = 0f;
						}
					}
					else
					{
						switch (k)
						{
						case 1:
							num4 = ((maxColumns != 3) ? (_margin / num5) : (num6 * 0.5f));
							break;
						case 2:
							num4 = num6 - _margin / num5;
							break;
						}
						if (k == maxColumns - 1)
						{
							num4 = num6;
						}
					}
					if (i < 4)
					{
						x = num4;
					}
					else
					{
						z = num4;
					}
					VertexList.Add(new Vector3(x, y, z));
				}
			}
		}
	}

	private void CalculateUV()
	{
		for (int i = 0; i < 6; i++)
		{
			float x = 1f;
			float y = 1f;
			if (i < 4)
			{
				maxColumns = maxColumnsX;
			}
			else
			{
				maxColumns = maxColumnsZ;
			}
			float x2 = _lossyScale.x;
			float z = _lossyScale.z;
			float num = _lossyScale.y * (_yBase / _margin) * _vMargin;
			float num2 = _vMargin + _vMargin * (1f - z % 1f) / z;
			float num3 = _vMargin + _vMargin * (1f - num % 1f) / num;
			float num4 = _uMargin + _uMargin * (1f - z % 1f) / z;
			float num5 = _uMargin + _uMargin * (1f - x2 % 1f) / x2;
			for (int j = 0; (i >= 2) ? (j < maxRows) : (j < maxColumnsZ); j++)
			{
				if (i < 2)
				{
					switch (j)
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
					if (j == maxColumnsZ - 1)
					{
						y = 0f;
					}
				}
				else
				{
					switch (j)
					{
					case 0:
						y = calculateCeiling(num);
						break;
					case 1:
						y = ((maxRows != 3) ? (calculateCeiling(num) - num3) : 0.5f);
						break;
					case 2:
						y = num3;
						break;
					}
					if (j == maxRows - 1)
					{
						y = 0f;
						if (maxRows == 2)
						{
							y = Mathf.Ceil(num) - _vMargin;
						}
					}
				}
				for (int k = 0; k < maxColumns; k++)
				{
					if (i < 4)
					{
						switch (k)
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
						if (k == maxColumns - 1)
						{
							x = calculateCeiling(x2);
							if (maxColumns < 3)
							{
								x = _uMargin - 0.05f;
							}
						}
					}
					else
					{
						switch (k)
						{
						case 0:
							x = 0f;
							break;
						case 1:
							x = ((maxColumns != 3) ? num4 : 0.5f);
							break;
						case 2:
							x = calculateCeiling(z) - num4;
							break;
						}
						if (k == maxColumns - 1)
						{
							x = calculateCeiling(z);
							if (maxColumns < 3)
							{
								x = _uMargin - 0.05f;
							}
						}
					}
					UV_List.Add(new Vector2(x, y));
				}
			}
		}
	}

	private void CalculateTriangle()
	{
		int num = 0;
		for (int i = 0; i < 6; i++)
		{
			for (int j = 0; (i >= 2) ? (j < maxRows - 1) : (j < maxColumnsZ - 1); j++)
			{
				if (i < 4)
				{
					maxColumns = maxColumnsX;
				}
				else
				{
					maxColumns = maxColumnsZ;
				}
				for (int k = 0; k < maxColumns - 1; k++)
				{
					int num2 = num + j * maxColumns + k;
					int item = num2;
					int item2 = num2 + maxColumns + 1;
					int item3 = num2 + maxColumns;
					int item4 = num2;
					int item5 = num2 + 1;
					int item6 = num2 + maxColumns + 1;
					if (i < 1)
					{
						TriangleList.Add(item);
						TriangleList.Add(item3);
						TriangleList.Add(item2);
						TriangleList.Add(item4);
						TriangleList.Add(item6);
						TriangleList.Add(item5);
					}
					else
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
			num = ((i >= 2) ? (num + maxRows * maxColumns) : (num + maxColumnsZ * maxColumns));
		}
	}
}
