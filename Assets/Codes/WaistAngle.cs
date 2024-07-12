using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaistAngle : MonoBehaviour
{
    public Animator anim;
    public CamMove cam;
    public PlayerMove playerMove;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (playerMove.aimingState)
        {
            anim.SetLookAtWeight(1, 1, 1);
            anim.SetLookAtPosition(cam.lookPos);
        }
        else
        {

        }
    }
}
