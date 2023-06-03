using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformTracker : MonoBehaviour
{
    [SerializeField] private bool drawLines = true;
    [SerializeField] private float recordThreshold = 0.1f;
    [SerializeField] private Color lineColor = Color.red;
    [SerializeField] private float lineDuration = 1f;
    [SerializeField, Range(0.1f, 1f)] private float lineHeight = 1f;

    Vector3 _prevPos;

    void Start()
    {
        _prevPos = transform.position;
    }

    void Update()
    {
        if (drawLines)
        {
            if ((_prevPos - transform.position).magnitude > recordThreshold)
            {
                Debug.DrawLine(transform.position + Vector3.down * lineHeight * .5f, transform.position + Vector3.up * lineHeight * .5f, Color.red, lineDuration);

                _prevPos = transform.position;
            }
        }
    }
}
