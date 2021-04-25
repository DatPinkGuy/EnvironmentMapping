using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;

public static class SaveSystem
{
    public static void SaveMesh(byte[] _meshBytes)
    {
        var formatter = new BinaryFormatter();
        var path = Application.persistentDataPath + "mesh.txt";
        var fileStream = new FileStream(path, FileMode.Create);
        var data = new SaveCloud(_meshBytes);
        formatter.Serialize(fileStream, data);
        fileStream.Close();
    }

    public static void SaveCloud(ulong[] key, float[] x, float[] y,float[] z) //Point Cloud data saving to a file
    {
        var formatter = new BinaryFormatter();
        var path = Application.persistentDataPath + "/cloud.txt";
        var fileStream = new FileStream(path, FileMode.Create);
        var data = new SaveCloud(key, x, y, z);
        formatter.Serialize(fileStream,data);
        fileStream.Close();
    }
    
#if (UNITY_EDITOR) || (UNITY_STANDALONE)    
    public static SaveCloud LoadCloud()
    {
        var path = Application.persistentDataPath + "mesh.txt";
        Debug.Log(path);
        if (File.Exists(path))
        {
            var formatter = new BinaryFormatter();
            var fileStream = new FileStream(path, FileMode.Open);
            if (fileStream.Length == 0)
            {
                fileStream.Dispose();
                return null;
            }
            var data = (SaveCloud) formatter.Deserialize(fileStream);
            fileStream.Close();
            return data;
        }
        else
        {
            Debug.Log("Save file not found");
            return null;
        }
    }
    
    public static SaveCloud LoadCloudPoints()
    {
        var path = Application.persistentDataPath + "/cloud.txt";
        Debug.Log(path);
        if (File.Exists(path))
        {
            var formatter = new BinaryFormatter();
            var fileStream = new FileStream(path, FileMode.Open);
            if (fileStream.Length == 0)
            {
                fileStream.Dispose();
                return null;
            }
            var data = (SaveCloud) formatter.Deserialize(fileStream);
            fileStream.Close();
            return data;
        }
        else
        {
            Debug.Log("Save file not found");
            return null;
        }
    }

    public static SaveCloud FileExplorerLoad(string path) //File loading based on file path provided
    {
        if (File.Exists(path))
        {
            var formatter = new BinaryFormatter();
            var fileStream = new FileStream(path, FileMode.Open);
            if (fileStream.Length == 0)
            {
                fileStream.Dispose();
                return null;
            }
            var data = (SaveCloud) formatter.Deserialize(fileStream);
            fileStream.Close();
            return data;
        }
        else
        {
            Debug.Log("Save file not found");
            return null;
        }
    }
#endif
#if (UNITY_EDITOR)
    
    public static SaveCloud TempLoad(Object obj) //Doesnt work when trying to build due to AssetDatabase
    {
        var path = AssetDatabase.GetAssetPath(obj); //Object needs to be in Assets and referenced
        if (File.Exists(path))
        {
            var formatter = new BinaryFormatter();
            var fileStream = new FileStream(path, FileMode.Open);
            if (fileStream.Length == 0)
            {
                fileStream.Dispose();
                return null;
            }
            var data = (SaveCloud) formatter.Deserialize(fileStream);
            fileStream.Close();
            return data;
        }
        else
        {
            Debug.Log("Save file not found");
            return null;
        }
    }
    
#endif
}
