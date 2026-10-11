using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AlwaysGripHand : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        animator.SetFloat("Grip", 1f);
        animator.SetFloat("Pinch", 1f);
    }
}