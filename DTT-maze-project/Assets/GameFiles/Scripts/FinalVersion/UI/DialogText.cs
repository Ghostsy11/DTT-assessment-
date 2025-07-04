using System.Collections;
using TMPro;
using UnityEngine;

public class DialogText : MonoBehaviour
{

    [Header("Dialog Settings")]
    [TextArea]
    [SerializeField] string fullText;         // The full dialog text
    [SerializeField] float duration = 5f;     // Total time to display the full text

    // [Header("Audio Settings")]
    // [SerializeField] AudioClip textAudio;

    [Header("UI Reference")]
    [SerializeField] TextMeshProUGUI dialogUIText;       // UI Text reference

    private Coroutine typingCoroutine;

    public void PlayOnStart()
    {
        PlayDialog(fullText, duration);

    }
    // Call this to start showing dialog
    public void PlayDialog(string textToShow, float displayDuration)
    {
        fullText = textToShow;
        duration = displayDuration;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText());
        //PlayAudioClip();
    }

    // Show the dialog letter by letter
    private IEnumerator TypeText()
    {
        dialogUIText.text = "";
        float delay = duration / fullText.Length;

        foreach (char c in fullText)
        {
            dialogUIText.text += c;
            yield return new WaitForSeconds(delay);
        }
        yield return new WaitForSeconds(15f);
        gameObject.SetActive(false);
    }

    //private void PlayAudioClip()
    //{
    //    if (textAudio == null) return;

    //    GameObject tempAudio = new GameObject("TempDialogAudio");
    //    AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
    //    audioSource.clip = textAudio;
    //    audioSource.Play();

    //    Destroy(tempAudio, textAudio.length);
    //}
}

