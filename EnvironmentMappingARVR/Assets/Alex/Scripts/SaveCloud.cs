using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveCloud
{
    public byte[] meshBytes;
    public ulong[] dictionaryKey;
    public float[] pointX;
    public float[] pointY;
    public float[] pointZ;

    public SaveCloud(byte[] _meshBytes)//Saves mesh
    {
        meshBytes = _meshBytes;
    }

    public SaveCloud(ulong[] key, float[] x, float[] y, float[] z)
    {
        dictionaryKey = key;
        pointX = x;
        pointY = y;
        pointZ = z;
    }
}
