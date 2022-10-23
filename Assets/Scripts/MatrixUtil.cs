using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MatrixUtil {
    public static void SetTransform(Transform t, float[] m) {
        if (m.Length != 16) return;
        t.localPosition = new Vector3(m[12], m[13], m[14]);
        Vector3 forward;
        forward.x = m[8];
        forward.y = m[9];
        forward.z = m[10];

        Vector3 upwards;
        upwards.x = m[4];
        upwards.y = m[5];
        upwards.z = m[6];

        t.localRotation = Quaternion.LookRotation(forward, upwards);
    }
}
