using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpriteFlicker : MonoBehaviour
{
    public UnityEvent OnStartFlick = new UnityEvent();
    public UnityEvent OnFinishFlick = new UnityEvent();

    public SpriteRenderer[] spriteRenderer;

    public float duration = 1.5f;
    public float flickDuration = 0.1f;

    float elapsedDuration = 0.0f;
    float elapsedFlickDuration = 0.0f;

    bool isFlicking = false;

    public bool IsFlicking => isFlicking;

    private void Update()
    {
        if (isFlicking)
        {
            if (elapsedDuration < Time.time)
            {
                for(int i = 0; i < spriteRenderer.Length; i++) spriteRenderer[i].enabled = true;
                isFlicking = false;
                OnFinishFlick.Invoke();
            }
            else
            {
                if (elapsedFlickDuration < Time.time)
                {
                     for(int i = 0; i < spriteRenderer.Length; i++) spriteRenderer[i].enabled = !spriteRenderer[i].enabled;
                    elapsedFlickDuration = Time.time + flickDuration;
                }
            }
        }
    }

    public void FlickIt()
    {
        elapsedDuration = Time.time + duration;
        elapsedFlickDuration = Time.time + flickDuration;

        isFlicking = true;

        OnStartFlick.Invoke();
    }
}
