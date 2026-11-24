using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuiPlayer : MonoBehaviour
{
    CameraControleurMovement mainCameraFollow;

    private void Start()
    {
        mainCameraFollow = Camera.main.gameObject.GetComponent<CameraControleurMovement>();
    }
    private void LateUpdate()
    {
        if (mainCameraFollow.getIsLock())
        {
            transform.LookAt(Camera.main.transform.position);
        }

    }
}
