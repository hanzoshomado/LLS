using UnityEngine;

public class SantaSnowStepAudio : MonoBehaviour
{
	public Animator LegsAnimator;

	public SantaCharacterController SantaCharacterController;

	public void PlaySnowStepSound()
	{
		SantaCharacterController.PlayStepForGroundType();
	}
}
