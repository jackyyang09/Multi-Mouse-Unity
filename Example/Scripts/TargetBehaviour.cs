using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetBehaviour : MonoBehaviour, IShootable
{
    [SerializeField] float resetTime = 3;
    [SerializeField] new Collider collider;
    [SerializeField] Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        ResetTarget();
    }

    public void InvokeOnShotBehaviour()
    {
        anim.Play("Flip Down");
        collider.enabled = false;
        Invoke(nameof(ResetTarget), resetTime);
    }

    void ResetTarget()
    {
        anim.Play("Flip Up");
        collider.enabled = true;
    }
}
