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
    public Vector3 lookRot;
    Vector3 aimPos;
    Vector3 aimRot;
    float weight = 0;
    float recoilValue = 20;
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
            aimRot = Quaternion.LookRotation(hitInfo.point - playerFire.firePos.transform.position).eulerAngles + new Vector3(23 - recoilValue, 40, 0);
        }
        else
        {
            aimRot = Quaternion.LookRotation(Camera.main.transform.forward).eulerAngles + new Vector3(23 - recoilValue, 40, 0);
        }
        aimPos = Quaternion.Euler(aimRot) * Vector3.forward * 10;
        // 반동 회복
        recoilValue -= recoilValue * Time.deltaTime * 2;
    }
    public void RecoilSet(float value)
    {
        recoilValue = value;
    }

private void OnAnimatorIK(int layerIndex)
    {
        anim.SetLookAtWeight(weight, weight, weight);
        if (playerStatus.aimingState)
        {
            if (weight < 1) weight += Time.deltaTime * 1;
            float valueY;
            float ratio = 30;
            if (cam.isZoomChanging)
            {
                ratio = 15;
                if (Mathf.Abs(Mathf.DeltaAngle(aimRot.y, playerMove.bodyTransform.eulerAngles.y)) > 90)
                {
                    valueY = playerMove.turnSpeed * Time.deltaTime;
                }
                else
                {
                    valueY = Mathf.DeltaAngle(lookRot.y, aimRot.y);
                }
            }
            else valueY = Mathf.DeltaAngle(lookRot.y, aimRot.y);
            lookRot += new Vector3(Mathf.DeltaAngle(lookRot.x, aimRot.x), valueY, Mathf.DeltaAngle(lookRot.z, aimRot.z)) * ratio * Time.deltaTime;
            lookPos = Quaternion.Euler(lookRot) * Vector3.forward * 10;
        }
        else //if (cam.isZoomChanging)
        {
            if (weight > 0) weight -= Time.deltaTime * 0.3f;
            float ratio = 15;
            lookRot += new Vector3(Mathf.DeltaAngle(lookRot.x, 0), Mathf.DeltaAngle(lookRot.y, playerMove.bodyTransform.eulerAngles.y), Mathf.DeltaAngle(lookRot.z, 0)) * ratio * Time.deltaTime;
            lookPos = Quaternion.Euler(lookRot) * Vector3.forward * 10;
        }
        anim.SetLookAtPosition(lookPos + playerMove.bodyTransform.position + Vector3.up * 0.63f);
    }
}
