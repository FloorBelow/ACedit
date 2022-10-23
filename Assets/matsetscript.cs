using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class matsetscript : MonoBehaviour
{
    public Material material;
    public bool activate;
    void Update()
    {
        if(activate) {
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers) renderer.sharedMaterial = material;
            activate = false;
        }
    }
}
