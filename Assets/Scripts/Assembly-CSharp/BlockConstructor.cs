using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class BlockConstructor : MonoBehaviour
{
	private Mesh blockMesh;

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

	public bool I_AmSide;

	public bool I_AmSideTop;

	public GameObject Parent;

	public bool isPlaying;

	private IEnumerator runInEditor_Coroutine;

	private void Start()
	{
		_lastLossyScale = Vector3.zero;
		_lossyScale = Vector3.one;
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

	private void Update()
	{
		if (isPlaying)
		{
			return;
		}
		_lossyScale = base.transform.lossyScale;
		if (base.transform.parent != null && (base.transform.parent == Parent || base.transform.parent.gameObject.layer == 8))
		{
			_lossyScale = base.transform.parent.transform.lossyScale;
		}
		if (_lastLossyScale != _lossyScale || blockMesh.vertices.Length == 0)
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
		blockMesh = GetComponent<MeshFilter>().sharedMesh;
		if ((bool)blockMesh)
		{
			blockMesh.Clear();
		}
		blockMesh = new Mesh();
		GetComponent<MeshFilter>().mesh = blockMesh;
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
		blockMesh.vertices = VertexList.ToArray();
		blockMesh.uv = UV_List.ToArray();
		blockMesh.triangles = TriangleList.ToArray();
		blockMesh.RecalculateNormals();
		blockMesh.RecalculateBounds();
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
			float f = _lossyScale.y * (_yBase / _margin) * _vMargin;
			for (int j = 0; (i >= 2) ? (j < maxRows) : (j < maxColumnsZ); j++)
			{
				if (i < 2)
				{
					switch (j)
					{
					case 0:
						y = Mathf.Ceil(z);
						if (maxColumnsZ == 2)
						{
							y = _vMargin - 0.05f;
						}
						break;
					case 1:
						y = ((maxColumnsZ != 3) ? (Mathf.Ceil(z) - _vMargin) : 0.5f);
						break;
					case 2:
						y = _vMargin;
						break;
					}
					if (j == maxColumnsZ - 1)
					{
						y = 0f;
					}
				}
				else
				{
					if (I_AmSide)
					{
						switch (j)
						{
						case 0:
							y = Mathf.Ceil(f);
							break;
						case 1:
							y = ((maxRows != 3) ? (Mathf.Ceil(f) - _vMargin) : 0.5f);
							break;
						case 2:
							y = _vMargin;
							break;
						}
						if (j == maxRows - 1)
						{
							y = 0f;
							if (maxRows == 2)
							{
								y = Mathf.Ceil(f) - _vMargin;
							}
						}
					}
					if (I_AmSideTop)
					{
						switch (j)
						{
						case 0:
							y = 0.99f;
							break;
						case 1:
							y = ((maxRows != 3) ? (1f - _vMargin) : 0.5f);
							break;
						case 2:
							y = _vMargin;
							break;
						}
						if (j == maxRows - 1)
						{
							y = 0f;
							if (maxRows == 2)
							{
								y = 1f - _vMargin;
							}
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
							x = ((maxColumns != 3) ? _uMargin : 0.5f);
							break;
						case 2:
							x = Mathf.Ceil(x2) - _uMargin;
							break;
						}
						if (k == maxColumns - 1)
						{
							x = Mathf.Ceil(x2);
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
							x = ((maxColumns != 3) ? _uMargin : 0.5f);
							break;
						case 2:
							x = Mathf.Ceil(z) - _uMargin;
							break;
						}
						if (k == maxColumns - 1)
						{
							x = Mathf.Ceil(z);
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
					if (i != 1)
					{
						if (I_AmSide)
						{
							if (i < 1)
							{
								TriangleList.Add(item);
								TriangleList.Add(item3);
								TriangleList.Add(item2);
								TriangleList.Add(item4);
								TriangleList.Add(item6);
								TriangleList.Add(item5);
							}
							else if (j > 0)
							{
								TriangleList.Add(item);
								TriangleList.Add(item2);
								TriangleList.Add(item3);
								TriangleList.Add(item4);
								TriangleList.Add(item5);
								TriangleList.Add(item6);
							}
						}
						else if (I_AmSideTop && i > 1 && j == 0)
						{
							TriangleList.Add(item);
							TriangleList.Add(item2);
							TriangleList.Add(item3);
							TriangleList.Add(item4);
							TriangleList.Add(item5);
							TriangleList.Add(item6);
						}
					}
					else if (!I_AmSide && !I_AmSideTop)
					{
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
			}
			num = ((i >= 2) ? (num + maxRows * maxColumns) : (num + maxColumnsZ * maxColumns));
		}
	}
}
