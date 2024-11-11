using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnimation : MonoBehaviour
{
    Animator ani;
    void Start()
    {
        ani = transform.GetComponentInChildren<Animator>();
    }

    public void PlayAniNext() 
    {
        if (ani != null)
        {
            ani.SetTrigger("Next");
        }
        else 
        {
            ShowMessageManager.Instance.ShowMessage("无动画!");
        }
    }
    public void PlayAniLast()
    {
        if (ani != null)
        {
            ani.SetTrigger("Last");
        }
        else
        {
            ShowMessageManager.Instance.ShowMessage("无动画!");
        }
    }
}
