using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WedgeConstructor : MonoBehaviour
{
	private Mesh wedgeMesh;

	private bool MonochromaticSides;

	private float _uMargin = 1f / 3f;

	private float _vMargin = 1f / 3f;

	private float _margin = 2.1333334f;

	private float _xBase = 6.4f;

	private float _yBase = 6.4f;

	private float _zBase = 9.6f;

	private float _xScale;

	private float _yScale;

	private float _zScale;

	private float _marginDividedXscale;

	private float _marginDividedYscale;

	private float _marginDividedZscale;

	private float _marginDividedYscale_HALF;

	private float _marginDividedYscale_INVERTED;

	private float _slope;

	private float _halfslope;

	private Vector3 _lossyScale;

	private Vector3 _lastLossyScale;

	public bool I_AmCollider;

	public bool I_AmSide;

	public bool I_AmSideTop;

	public GameObject Parent;

	public MeshCollider meshCollider;

	public bool isPlaying;

	private IEnumerator runInEditor_Coroutine;

	private void Start()
	{
		_slope = _yBase / _zBase;
		_halfslope = _slope / 2f;
		_lastLossyScale = Vector3.zero;
		CreateWedge();
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
		if (!isPlaying)
		{
			if (base.transform.parent == Parent)
			{
				_lossyScale = base.transform.parent.transform.lossyScale;
			}
			else
			{
				_lossyScale = base.transform.lossyScale;
			}
			if (_lossyScale != _lastLossyScale || wedgeMesh.vertices.Length == 0)
			{
				CreateWedge();
				_lastLossyScale = _lossyScale;
			}
		}
	}

	private void ClearMesh()
	{
		if (wedgeMesh == null)
		{
			wedgeMesh = new Mesh();
		}
		else
		{
			wedgeMesh.Clear();
		}
		GetComponent<MeshFilter>().mesh = wedgeMesh;
	}

	private void CheckSize()
	{
		MonochromaticSides = false;
		if (_zBase <= _marginDividedYscale / _halfslope / _zScale || _yBase <= _marginDividedYscale * 2f)
		{
			MonochromaticSides = true;
		}
	}

	private void CreateWedge()
	{
		_xScale = _lossyScale.x;
		_yScale = _lossyScale.y;
		_zScale = _lossyScale.z;
		_marginDividedXscale = _margin / _xScale;
		_marginDividedYscale = _margin / _yScale;
		_marginDividedZscale = _margin / _zScale;
		_marginDividedYscale_HALF = _marginDividedYscale / _halfslope;
		_marginDividedYscale_INVERTED = _yBase - _marginDividedYscale;
		ClearMesh();
		CheckSize();
		Constructor();
		wedgeMesh.RecalculateNormals();
		wedgeMesh.RecalculateBounds();
		if (meshCollider != null)
		{
			meshCollider.sharedMesh = null;
			meshCollider.sharedMesh = wedgeMesh;
			meshCollider.convex = true;
			base.transform.gameObject.GetComponent<MeshRenderer>().enabled = false;
		}
	}

	private void Constructor()
	{
		float num = _zBase / _marginDividedZscale * _vMargin;
		float x = _marginDividedYscale / _halfslope / _marginDividedZscale * _vMargin;
		if (!MonochromaticSides)
		{
			wedgeMesh.vertices = new Vector3[38]
			{
				new Vector3(0f, _yBase, _zBase),
				new Vector3(_xBase, _yBase, _zBase),
				new Vector3(0f, 0f, 0f),
				new Vector3(_xBase, 0f, 0f),
				new Vector3(0f, 0f, _zBase),
				new Vector3(_xBase, 0f, _zBase),
				new Vector3(0f, 0f, _marginDividedZscale),
				new Vector3(_xBase, 0f, _marginDividedZscale),
				new Vector3(0f, 0f, 0f),
				new Vector3(_xBase, 0f, 0f),
				new Vector3(0f, _yBase, _zBase),
				new Vector3(_xBase, _yBase, _zBase),
				new Vector3(0f, _yBase - _marginDividedYscale, _zBase),
				new Vector3(_xBase, _yBase - _marginDividedYscale, _zBase),
				new Vector3(0f, 0f, _zBase),
				new Vector3(_xBase, 0f, _zBase),
				new Vector3(0f, 0f, 0f),
				new Vector3(0f, _yBase, _zBase),
				new Vector3(0f, _marginDividedYscale, _marginDividedYscale_HALF),
				new Vector3(0f, _yBase - _marginDividedYscale, _zBase),
				new Vector3(0f, _marginDividedYscale, _marginDividedYscale_HALF),
				new Vector3(0f, _marginDividedYscale_INVERTED, _zBase),
				new Vector3(0f, _marginDividedYscale, _zBase),
				new Vector3(0f, _marginDividedYscale, _marginDividedYscale_HALF),
				new Vector3(0f, _marginDividedYscale, _zBase),
				new Vector3(0f, 0f, 0f),
				new Vector3(0f, 0f, _zBase),
				new Vector3(_xBase, 0f, 0f),
				new Vector3(_xBase, _yBase, _zBase),
				new Vector3(_xBase, _marginDividedYscale, _marginDividedYscale_HALF),
				new Vector3(_xBase, _yBase - _marginDividedYscale, _zBase),
				new Vector3(_xBase, _marginDividedYscale, _marginDividedYscale_HALF),
				new Vector3(_xBase, _marginDividedYscale_INVERTED, _zBase),
				new Vector3(_xBase, _marginDividedYscale, _zBase),
				new Vector3(_xBase, _marginDividedYscale, _marginDividedYscale_HALF),
				new Vector3(_xBase, _marginDividedYscale, _zBase),
				new Vector3(_xBase, 0f, 0f),
				new Vector3(_xBase, 0f, _zBase)
			};
			Vector2[] array = new Vector2[4]
			{
				new Vector2(0f, num),
				new Vector2(_xScale, num),
				new Vector2(0f, 0f),
				new Vector2(_xScale, 0f)
			};
			Vector2[] array2 = ((!I_AmSideTop) ? new Vector2[12]
			{
				new Vector2(0f, 0f),
				new Vector2(_xScale, 0f),
				new Vector2(0f, _vMargin),
				new Vector2(_xScale, _vMargin),
				new Vector2(0f, _zScale),
				new Vector2(_xScale, _zScale),
				new Vector2(0f, _yScale),
				new Vector2(_xScale, _yScale),
				new Vector2(0f, _yScale - _vMargin),
				new Vector2(_xScale, _yScale - _vMargin),
				new Vector2(0f, 0f),
				new Vector2(_xScale, 0f)
			} : new Vector2[12]
			{
				new Vector2(0f, 0f),
				new Vector2(_xScale, 0f),
				new Vector2(0f, 1f - _vMargin),
				new Vector2(_xScale, 1f - _vMargin),
				new Vector2(0f, 1f),
				new Vector2(_xScale, 1f),
				new Vector2(0f, 0.99f),
				new Vector2(_xScale, 0.99f),
				new Vector2(0f, 1f - _vMargin),
				new Vector2(_xScale, 1f - _vMargin),
				new Vector2(0f, 0f),
				new Vector2(_xScale, 0f)
			});
			Vector2[] array3 = new Vector2[22]
			{
				new Vector2(0f, 0.99f),
				new Vector2(num, 0.99f),
				new Vector2(x, 1f - _vMargin),
				new Vector2(num, 1f - _vMargin),
				new Vector2(x, _vMargin),
				new Vector2(num, _yScale - _vMargin),
				new Vector2(num, _vMargin),
				new Vector2(x, _vMargin),
				new Vector2(num, _vMargin),
				new Vector2(0f, 0f),
				new Vector2(num, 0f),
				new Vector2(0f, 0.99f),
				new Vector2(num, 0.99f),
				new Vector2(x, 1f - _vMargin),
				new Vector2(num, 1f - _vMargin),
				new Vector2(x, _vMargin),
				new Vector2(num, _yScale - _vMargin),
				new Vector2(num, _vMargin),
				new Vector2(x, _vMargin),
				new Vector2(num, _vMargin),
				new Vector2(0f, 0f),
				new Vector2(num, 0f)
			};
			Vector2[] array4 = new Vector2[array.Length + array2.Length + array3.Length];
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			for (num2 = 0; num2 < array.Length; num2++)
			{
				array4[num2] = array[num2];
			}
			for (num3 = 0; num3 < array2.Length; num3++)
			{
				array4[num2 + num3] = array2[num3];
			}
			for (num4 = 0; num4 < array3.Length; num4++)
			{
				array4[num2 + num3 + num4] = array3[num4];
			}
			wedgeMesh.uv = array4;
			if (I_AmCollider)
			{
				wedgeMesh.triangles = new int[60]
				{
					0, 3, 2, 0, 1, 3, 4, 6, 7, 4,
					7, 5, 6, 8, 9, 6, 9, 7, 10, 12,
					13, 10, 13, 11, 12, 14, 15, 12, 15, 13,
					16, 18, 17, 18, 19, 17, 20, 22, 21, 23,
					25, 26, 23, 26, 24, 27, 28, 29, 29, 28,
					30, 31, 32, 33, 34, 37, 36, 34, 35, 37
				};
			}
			else if (I_AmSide)
			{
				wedgeMesh.triangles = new int[30]
				{
					4, 6, 7, 4, 7, 5, 12, 14, 15, 12,
					15, 13, 20, 22, 21, 23, 25, 26, 23, 26,
					24, 31, 32, 33, 34, 37, 36, 34, 35, 37
				};
			}
			else if (I_AmSideTop)
			{
				wedgeMesh.triangles = new int[24]
				{
					6, 8, 9, 6, 9, 7, 10, 12, 13, 10,
					13, 11, 16, 18, 17, 18, 19, 17, 27, 28,
					29, 29, 28, 30
				};
			}
			else
			{
				wedgeMesh.triangles = new int[6] { 0, 3, 2, 0, 1, 3 };
			}
		}
		if (MonochromaticSides && I_AmSide)
		{
			wedgeMesh.vertices = new Vector3[18]
			{
				new Vector3(0f, _yBase, _zBase),
				new Vector3(_xBase, _yBase, _zBase),
				new Vector3(0f, 0f, 0f),
				new Vector3(_xBase, 0f, 0f),
				new Vector3(0f, 0f, _zBase),
				new Vector3(_xBase, 0f, _zBase),
				new Vector3(0f, 0f, 0f),
				new Vector3(_xBase, 0f, 0f),
				new Vector3(0f, _yBase, _zBase),
				new Vector3(_xBase, _yBase, _zBase),
				new Vector3(0f, 0f, _zBase),
				new Vector3(_xBase, 0f, _zBase),
				new Vector3(0f, 0f, 0f),
				new Vector3(0f, _yBase, _zBase),
				new Vector3(0f, 0f, _zBase),
				new Vector3(_xBase, 0f, 0f),
				new Vector3(_xBase, _yBase, _zBase),
				new Vector3(_xBase, 0f, _zBase)
			};
			wedgeMesh.uv = new Vector2[18]
			{
				new Vector2(0f, num),
				new Vector2(_xScale, num),
				new Vector2(0f, 0f),
				new Vector2(_xScale, 0f),
				new Vector2(0f, num),
				new Vector2(_xScale, num),
				new Vector2(0f, 0f),
				new Vector2(_xScale, 0f),
				new Vector2(0f, num),
				new Vector2(_xScale, num),
				new Vector2(0f, 0f),
				new Vector2(_xScale, 0f),
				new Vector2(_xScale, num),
				new Vector2(0f, 0f),
				new Vector2(_xScale, 0f),
				new Vector2(_xScale, num),
				new Vector2(0f, 0f),
				new Vector2(_xScale, 0f)
			};
			wedgeMesh.triangles = new int[24]
			{
				0, 3, 2, 0, 1, 3, 4, 6, 7, 4,
				7, 5, 8, 10, 11, 8, 11, 9, 12, 14,
				13, 15, 16, 17
			};
		}
	}
}
