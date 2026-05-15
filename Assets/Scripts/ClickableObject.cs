using UnityEngine;
using TMPro;
using System.Collections;

public class UIButtonClick : MonoBehaviour
{
    [Header("Counter")]
    public TMP_Text texteCounter;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonClique;

    [Header("Animation")]
    public RectTransform button;
    public float SizeCliqer = 0.9f;
    public float durationAnimation = 0.1f;

    private int compteur = 0;
    private Vector3 SizeOriginale;

    void Start()
    {
        SizeOriginale = button.localScale;
    }

    public void Cliquer()
    {
        compteur++;
        texteCounter.text = compteur.ToString();

        audioSource.PlayOneShot(sonClique);

        StopAllCoroutines();
        StartCoroutine(AnimationButton());
    }

    IEnumerator AnimationButton()
    {
        button.localScale = SizeOriginale * SizeCliqer;

        yield return new WaitForSeconds(durationAnimation);

        button.localScale = SizeOriginale;
    }
}
