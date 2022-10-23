using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityComponent : MonoBehaviour {
    public uint id;
    public uint datafileID;
    public long datafileOffset;
    public long gameStateTransformMatrixOffset;
    public bool edit;
    public float[] transformMatrix;
}
