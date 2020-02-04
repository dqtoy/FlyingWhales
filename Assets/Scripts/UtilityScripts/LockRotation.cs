using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockRotation : MonoBehaviour {
    Quaternion rotation;

    private void Start() {
        rotation = transform.rotation;
    }
    private void LateUpdate() {
        transform.rotation = rotation;
    }
}
