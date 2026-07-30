using UnityEngine;

public class ReindeerPlayerAnimator : MonoBehaviour
{
	public Animator ReindeerAnimator;

	public SantaCharacterController Owner;

	public void SetAnimatorBool(string animatorProperty, bool value)
	{
		ReindeerAnimator.SetBool(animatorProperty, value);
	}

	public void PlayTrotStep()
	{
		Owner.PlayReindeerStep();
	}

	public void PlayRunStep()
	{
		Owner.PlayReindeerRunStep();
	}
}
