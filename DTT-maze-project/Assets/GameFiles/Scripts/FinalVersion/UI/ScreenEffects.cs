using UnityEngine;

public class ScreenEffects : MonoBehaviour
{
    [SerializeField] Animator loadingScreenAnimation;
    [SerializeField] AudioSource lodaingAudio;

    private void Update()
    {
        // Developer Tool
        if (Input.GetKeyDown(KeyCode.L))
        {
            loadingScreenAnimation.SetTrigger("PlayTransition");

        }
    }

    public void PlayFadeScreenEffect()
    {
        if (!lodaingAudio.isPlaying)
        {
            lodaingAudio.Play();
            loadingScreenAnimation.SetTrigger("FadeInOut");
        }

    }
}
