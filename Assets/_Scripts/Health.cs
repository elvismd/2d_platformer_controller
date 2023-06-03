using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class UnityEvtTransform : UnityEvent<Transform> { }

public class Health : MonoBehaviour
{
    [SerializeField] private float initialValue = 100f;
    [SerializeField] private float value = 100f;

    [SerializeField] private bool invincible = false;

    public UnityEvent OnDie = new UnityEvent();
    public UnityEvent OnDamage = new UnityEvent();
    public UnityEvtTransform OnDamageTransformRef = new UnityEvtTransform();

    public UnityEvent OnHeal = new UnityEvent();
    public UnityEvent OnEndInvincibility = new UnityEvent();

        public bool IsInvincible => invincibleElapsed > Time.time || invincible;
    public float Value => value;

    private float invincibleElapsed = 0.0f;
    private bool prevInvincible;

    private void Start()
    {
        value = initialValue;
    }

    public void Restore()
    {
        value = initialValue;
    }

    private void Update()
    {
        if (!IsInvincible && prevInvincible)
        {
            OnEndInvincibility.Invoke();
        }

        prevInvincible = IsInvincible;
    }

    public void Change(float mod, Transform origin = null)
    {
        if (IsInvincible && mod < 0) return;

        if (((value <= 0) && (value + mod <= 0)) || (value >= initialValue && (value + mod >= initialValue))) return;

        value += mod;
        if (mod < 0)
        {
            OnDamage.Invoke();
            OnDamageTransformRef.Invoke(origin);
        }
        else if (mod > 0)
            OnHeal.Invoke();

        if (value > initialValue)
            value = initialValue;

        if (value <= 0)
        {
            value = 0;
            OnDie.Invoke();
        }
    }

    public void InvincibleFor(float seconds)
    {
        invincibleElapsed = Time.time + seconds;
    }
}
