using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        PlayerController.PlayerWalk += ToggleWalk;
        PlayerController.PlayerShoot += TriggerShoot;
        PlayerController.PlayerJump += TriggerJump;
    }

    private void OnDestroy()
    {
        PlayerController.PlayerWalk -= ToggleWalk;
        PlayerController.PlayerShoot -= TriggerShoot;
        PlayerController.PlayerJump -= TriggerJump;
    }

    void TriggerShoot()
    {
        anim.SetTrigger("Shoot");
    }

    void TriggerJump()
    {
        anim.SetTrigger("Jump");
    }

    void ToggleWalk(bool boolean)
    {
        if (boolean) anim.SetBool("isWalking", true);
        else anim.SetBool("isWalking", false);
    }
}
