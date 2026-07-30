using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class addIcicles : MonoBehaviour
{
	public GameObject Icicles;

	private GameObject iciclesInstance;

	private bool isPlaying;

	private IEnumerator runInEditor_Coroutine;

	public bool isTree;

	public bool Add_Icicles;

	private void Start()
	{
		if ((bool)base.transform.Find("Icicles(Clone)"))
		{
			iciclesInstance = base.transform.Find("Icicles(Clone)").gameObject;
		}
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
		if (Add_Icicles && iciclesInstance == null)
		{
			iciclesInstance = Object.Instantiate(Icicles, base.transform, true);
			if (isTree)
			{
				iciclesInstance.transform.localPosition = new Vector3(-3.1f, -3.3f, -3.1f);
				iciclesInstance.transform.localScale = new Vector3(0.97f, 0.83f, 0.97f);
			}
			else
			{
				iciclesInstance.transform.localPosition = new Vector3(0f, -6.4f, 0f);
				iciclesInstance.transform.localScale = Vector3.one;
			}
			iciclesInstance.transform.localRotation = Quaternion.identity;
			iciclesInstance.GetComponent<BlockConstructor>().CreatePlatform();
		}
		if (!Add_Icicles && iciclesInstance != null)
		{
			Object.DestroyImmediate(iciclesInstance);
		}
	}
}
