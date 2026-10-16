using UnityEngine;
using UnityEngine.InputSystem;
namespace Aurelia
{
    // Gameplay parameters for the authored Blender clips; the Animator performs deformation.
    public class KnightV3Motion:MonoBehaviour
    {
        Animator animator;CharacterController body;float cooldown;
        void Awake(){animator=GetComponent<Animator>();body=GetComponentInParent<CharacterController>();}
        void Update()
        {
            if(!animator)return;cooldown-=Time.deltaTime;
            animator.SetBool("Airborne",body&&!body.isGrounded&&Mathf.Abs(body.velocity.y)>1.2f);
            bool attack=Keyboard.current!=null&&Keyboard.current.eKey.wasPressedThisFrame||Mouse.current!=null&&Cursor.lockState==CursorLockMode.Locked&&Mouse.current.leftButton.wasPressedThisFrame;
            if(attack&&cooldown<=0){animator.SetTrigger("Attack");cooldown=1.05f;}
        }
    }
}
