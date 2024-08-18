using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudDisappear : MonoBehaviour
{
    // Start is called before the first frame update
    public float disappearTime = 2f;  
    public float fadeDuration = 1f;
    private bool isTriggered = false;
    private SpriteRenderer spriteRenderer;


    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && !isTriggered)
        {
            isTriggered = true;
            StartCoroutine(FadeOut());
            Invoke("Disappear", disappearTime);
        }
    }

    private IEnumerator FadeOut()
    {
     
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(1f, 0f, normalizedTime);
            spriteRenderer.color = color;
            yield return null;
        }
        Color finalColor = spriteRenderer.color;
        finalColor.a = 0f;
        spriteRenderer.color = finalColor;
    }
    private void Disappear()
    {
        // Deactivate the cloud tile (make it disappear)
        gameObject.SetActive(false);
    }

}
