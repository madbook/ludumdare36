using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour {
    public Transform camera;

    void Update () {
        transform.forward = camera.forward;
    }
}
