using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaistAngle : MonoBehaviour
{
    public Animator anim;
    public CamMove cam;
    public PlayerMove playerMove;
    public PlayerFire playerFire;
    public PlayerStatus playerStatus;

    Vector3 lookPos;
    Vector3 lookRot;
    Vector3 aimPos;
    Vector3 aimRot;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hitInfo = new RaycastHit();
        if (Physics.Raycast(ray, out hitInfo))
        {
            aimRot = Quaternion.LookRotation(hitInfo.point - playerFire.firePos.transform.position).eulerAngles + new Vector3(18, 15, 0);
        }
        else
        {
            aimRot = Quaternion.LookRotation(Camera.main.transform.forward).eulerAngles + new Vector3(18, 15, 0);
        }
        aimPos = Quaternion.Euler(aimRot) * Vector3.forward * 10;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        anim.SetLookAtWeight(1, 1, 1);
        if (playerStatus.aimingState)
        {
            float ratio = 30;
            if (cam.isZoomChanging) ratio = 15;
            lookRot += new Vector3(Mathf.DeltaAngle(lookRot.x, aimRot.x), Mathf.DeltaAngle(lookRot.y, aimRot.y), Mathf.DeltaAngle(lookRot.z, aimRot.z)) * ratio * Time.deltaTime;
            lookPos = Quaternion.Euler(lookRot) * Vector3.forward * 10;
        }
        else if (cam.isZoomChanging)
        {
            float ratio = 15;
            lookRot += new Vector3(Mathf.DeltaAngle(lookRot.x, 0), Mathf.DeltaAngle(lookRot.y, playerMove.bodyTransform.eulerAngles.y), Mathf.DeltaAngle(lookRot.z, 0)) * ratio * Time.deltaTime;
            lookPos = Quaternion.Euler(lookRot) * Vector3.forward * 10;
        }
        else
        {
            lookRot = new Vector3(0, playerMove.bodyTransform.eulerAngles.y, 0);
            lookPos = playerMove.bodyTransform.forward * 10;
        }
        anim.SetLookAtPosition(lookPos + playerMove.bodyTransform.position + Vector3.up * 0.63f + playerMove.bodyTransform.forward);
    }
}
