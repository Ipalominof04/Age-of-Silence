using System;
using UnityEngine;

[Serializable]
public struct ScenePart
{
    public string id;
    public bool active;
    public Vector3 pos;
    public Quaternion rot;
    public bool persisteAlMorir;
}