using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WedgeCornerConstructor : MonoBehaviour
{
	private Mesh wedgeCornerMesh;

	private bool Monochromatic;

	private float _uMargin = 1f / 6f;

	private float _vMargin = 1f / 3f;

	private float _margin = 2.1333334f;

	private float _xBase = 9.6f;

	private float _yBase = 6.4f;

	private float _zBase = 9.6f;

	private float _xScale;

	private float _yScale;

	private float _zScale;

	private float _marginDividedXscale;

	private float _marginDividedYscale;

	private float _marginDividedZscale;

	private float Ymargin_adjustedBy_X;

	private float Ymargin_adjustedBy_Z;

	private float _marginpoint_along_slope_Xvector;

	private float _marginpoint_along_slope_Zvector;

	private float _slope;

	private Vector3 _lossyScale;

	private Vector3 _lastLossyScale;

	public bool I_AmCollider;

	public bool I_AmSide;

	public GameObject Parent;

	public MeshCollider meshCollider;

	private bool isPlaying;

	private IEnumerator runInEditor_Coroutine;

	private void Start()
	{
		_slope = _yBase / _zBase;
		_lastLossyScale = Vector3.zero;
		CreateWedgeCorner();
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
			if (base.transform.parent == Parent && Parent != null)
			{
				_lossyScale = base.transform.parent.transform.lossyScale;
			}
			else
			{
				_lossyScale = base.transform.lossyScale;
			}
			if (_lossyScale != _lastLossyScale || wedgeCornerMesh.vertices.Length == 0)
			{
				CreateWedgeCorner();
				_lastLossyScale = _lossyScale;
			}
		}
	}

	private void CheckSize()
	{
		Monochromatic = false;
		if (_lossyScale.x <= 0.25f || _lossyScale.y <= 0.25f || _lossyScale.z <= 0.25f)
		{
			Monochromatic = true;
		}
	}

	private void ClearMesh()
	{
		if (wedgeCornerMesh == null)
		{
			wedgeCornerMesh = new Mesh();
		}
		else
		{
			wedgeCornerMesh.Clear();
		}
		GetComponent<MeshFilter>().mesh = wedgeCornerMesh;
	}

	private void CreateWedgeCorner()
	{
		_xScale = _lossyScale.x;
		_yScale = _lossyScale.y;
		_zScale = _lossyScale.z;
		_marginDividedXscale = _margin / _xScale;
		_marginDividedYscale = _margin / _yScale;
		_marginDividedZscale = _margin / _zScale;
		Ymargin_adjustedBy_Z = _marginDividedYscale / (_zScale * _marginDividedZscale);
		Ymargin_adjustedBy_X = _marginDividedYscale / (_xScale * _marginDividedXscale);
		_marginpoint_along_slope_Xvector = _xBase * (Ymargin_adjustedBy_Z / _yBase);
		ClearMesh();
		CheckSize();
		Constructor();
		wedgeCornerMesh.RecalculateNormals();
		wedgeCornerMesh.RecalculateBounds();
		if (meshCollider != null)
		{
			meshCollider.sharedMesh = null;
			meshCollider.sharedMesh = wedgeCornerMesh;
			meshCollider.convex = true;
			base.transform.gameObject.GetComponent<MeshRenderer>().enabled = false;
		}
	}

	private void Constructor()
	{
		float num = Mathf.Sqrt(Mathf.Pow(_xBase * _xScale, 2f) + Mathf.Pow(_yBase * _yScale, 2f) + Mathf.Pow(_zBase * _zScale, 2f)) / (_margin * (_xScale + _yScale + _zScale)) * _vMargin;
		float num2 = Mathf.Sqrt(Mathf.Pow((_xBase - _marginDividedXscale) * _xScale, 2f) + Mathf.Pow((_yBase - Ymargin_adjustedBy_Z) * _yScale, 2f) + Mathf.Pow((_marginpoint_along_slope_Zvector - _zBase) * _zScale, 2f)) / (_margin * (_xScale + _yScale + _zScale)) * _vMargin;
		float num3 = Mathf.Sqrt(Mathf.Pow((_marginpoint_along_slope_Xvector - _xBase) * _xScale, 2f) + Mathf.Pow((_yBase - Ymargin_adjustedBy_X) * _yScale, 2f) + Mathf.Pow((_zBase - _marginDividedZscale) * _zScale, 2f)) / (_margin * (_xScale + _yScale + _zScale)) * _vMargin;
		wedgeCornerMesh.vertices = new Vector3[16]
		{
			new Vector3(0f, 0f, _zBase),
			new Vector3(_xBase, 0f, _zBase),
			new Vector3(0f, 0f, 0f),
			new Vector3(_xBase, 0f, 0f),
			new Vector3(0f, _yBase, 0f),
			new Vector3(0f, 0f, 0f),
			new Vector3(_xBase, 0f, 0f),
			new Vector3(0f, _yBase, 0f),
			new Vector3(_xBase, 0f, _zBase),
			new Vector3(0f, 0f, _zBase),
			new Vector3(0f, _yBase, 0f),
			new Vector3(0f, 0f, _zBase),
			new Vector3(0f, 0f, 0f),
			new Vector3(0f, _yBase, 0f),
			new Vector3(_xBase, 0f, 0f),
			new Vector3(_xBase, 0f, _zBase)
		};
		wedgeCornerMesh.uv = new Vector2[16]
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
		if (I_AmCollider)
		{
			wedgeCornerMesh.triangles = new int[18]
			{
				0, 2, 3, 0, 3, 1, 4, 6, 5, 7,
				9, 8, 10, 12, 11, 13, 15, 14
			};
		}
		else if (I_AmSide)
		{
			wedgeCornerMesh.triangles = new int[12]
			{
				0, 2, 3, 0, 3, 1, 4, 6, 5, 10,
				12, 11
			};
		}
		else
		{
			wedgeCornerMesh.triangles = new int[6] { 7, 9, 8, 13, 15, 14 };
		}
	}
}
