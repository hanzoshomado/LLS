using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Randomize_Mesh : MonoBehaviour
{
	public bool NextMesh;

	public int RoundRobin;

	public Mesh[] Meshes;

	private MeshFilter meshFilter;

	private void Start()
	{
		meshFilter = base.transform.GetComponent<MeshFilter>();
		if (meshFilter.sharedMesh == null)
		{
			int num = Random.Range(0, Meshes.Length);
			meshFilter.mesh = Meshes[num];
		}
	}

	private void Update()
	{
		if (NextMesh)
		{
			if (RoundRobin < Meshes.Length)
			{
				meshFilter.mesh = Meshes[RoundRobin];
				RoundRobin++;
				NextMesh = false;
			}
			else
			{
				RoundRobin = 0;
			}
		}
	}
}
