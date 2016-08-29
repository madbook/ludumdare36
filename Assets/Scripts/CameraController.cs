using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
    void Update () {
        if (Input.GetKeyDown (KeyCode.LeftArrow)) {
            transform.RotateAround (Vector3.zero, Vector3.up, -45f);
        } else if (Input.GetKeyDown (KeyCode.RightArrow)) {
            transform.RotateAround (Vector3.zero, Vector3.up, 45f);
        } else if (Input.GetKeyDown (KeyCode.UpArrow)) {
            transform.RotateAround (Vector3.zero, transform.right, 45);
        } else if (Input.GetKeyDown (KeyCode.DownArrow)) {
            transform.RotateAround (Vector3.zero, transform.right, -45);
        }
    }
}
