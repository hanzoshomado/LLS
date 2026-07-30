using System.Collections.Generic;
using Bolt;
using UnityEngine;

[ExecuteInEditMode]
public class BoltPOI : EntityBehaviour
{
	private static Mesh mesh;

	private static Material poiMat;

	private static Material aoiDetectMat;

	private static Material aoiReleaseMat;

	private static List<BoltPOI> availablePOIs = new List<BoltPOI>();

	[SerializeField]
	public float radius = 16f;

	public static Mesh Mesh
	{
		get
		{
			if (!mesh)
			{
				mesh = (Mesh)Resources.Load("IcoSphere", typeof(Mesh));
			}
			return mesh;
		}
	}

	public static Material MaterialPOI
	{
		get
		{
			if (!poiMat)
			{
				poiMat = CreateMaterial(new Color(0f, 0.14509805f, 1f));
			}
			return poiMat;
		}
	}

	public static Material MaterialAOIDetect
	{
		get
		{
			if (!aoiDetectMat)
			{
				aoiDetectMat = CreateMaterial(new Color(0.14509805f, 0.4f, 0f));
			}
			return aoiDetectMat;
		}
	}

	public static Material MaterialAOIRelease
	{
		get
		{
			if (!aoiReleaseMat)
			{
				aoiReleaseMat = CreateMaterial(new Color(1f, 0.14509805f, 0f));
			}
			return aoiReleaseMat;
		}
	}

	private static Material CreateMaterial(Color c)
	{
		Material material = new Material(Resources.Load("BoltShaderPOI", typeof(Shader)) as Shader);
		material.hideFlags = HideFlags.HideAndDontSave;
		material.SetColor("_SpecColor", c);
		return material;
	}

	private void Update()
	{
		Graphics.DrawMesh(Mesh, Matrix4x4.TRS(base.transform.position, Quaternion.identity, new Vector3(radius, radius, radius)), MaterialPOI, 0);
	}

	private void OnDestroy()
	{
		availablePOIs.Remove(this);
	}

	private void BoltSceneObject()
	{
		if (BoltNetwork.isClient)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public override void Attached()
	{
		availablePOIs.Add(this);
	}

	public override void Detached()
	{
		availablePOIs.Remove(this);
	}

	public static void UpdateScope(BoltAOI aoi, BoltConnection connection)
	{
		Vector3 position = aoi.transform.position;
		float detectRadius = aoi.detectRadius;
		float releaseRadius = aoi.releaseRadius;
		for (int i = 0; i < availablePOIs.Count; i++)
		{
			BoltPOI boltPOI = availablePOIs[i];
			Vector3 position2 = boltPOI.transform.position;
			float bRadius = boltPOI.radius;
			if (OverlapSphere(position, position2, detectRadius, bRadius))
			{
				boltPOI.entity.SetScope(connection, true);
			}
			else if (!OverlapSphere(position, position2, releaseRadius, bRadius))
			{
				boltPOI.entity.SetScope(connection, false);
			}
		}
	}

	private static bool OverlapSphere(Vector3 a, Vector3 b, float aRadius, float bRadius)
	{
		float num = aRadius + bRadius;
		return (a - b).sqrMagnitude <= num * num;
	}
}
