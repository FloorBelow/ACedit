using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AssetPaths", menuName = "ScriptableObjects/AssetPathObject", order = 1)]
public class AssetPathObject : ScriptableObject
{
    public List<uint> ids;
    public List<string> paths;
}
