using UnityEngine;

public class MysteryBoxController : MonoBehaviour
{
    public Animator animator;

    private bool isOpen = false;

    public void Spin()
    {
        if (isOpen) return;

        animator.SetTrigger("OpenTrigger");
        isOpen = true;
    }

    public void CloseBox()
    {
        if (!isOpen) return;

        animator.SetTrigger("CloseTrigger");
        isOpen = false;
    }
}