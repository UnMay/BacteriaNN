using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PanelOpen : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Animator animator;

    public void OpenPanel()
    {
        bool isOpen = animator.GetBool("isOpen");

        if (isOpen)
            text.text = "<";
        else
            text.text = ">";

        animator.SetBool("isOpen", !isOpen);
    }
}
