using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour {
    public Transform cameraToLookAt;

    void Update () {
        transform.forward = cameraToLookAt.forward;
    }
}
