using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(Mesh))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class MeshDrawing : MonoBehaviour
{
    private ARPointCloud m_arPointCloud;
    private bool m_pointCloudFound;
    public Mesh mesh { get; private set; }
    static List<Vector3> s_Vertices = new List<Vector3>();
    private const string k_newLine = "\n";
    [SerializeField] private Text m_textArea;
    
    private void Awake()
    {
        mesh = new Mesh();
    }
    
    void Update()
    {
        if(!m_pointCloudFound) TryFindCloud();
    }

    private void TryFindCloud()
    {
        m_arPointCloud = FindObjectOfType<ARPointCloud>();
    }

    public void GenerateMesh()
    {
        if (!m_arPointCloud) m_textArea.text += "No ARPointCloud" + k_newLine;
        try
        {
            m_textArea.text += "Started" + k_newLine;
            s_Vertices.Clear();
            m_textArea.text += "Vertices cleared" + k_newLine;
            if (m_arPointCloud.positions.HasValue)
            {
                foreach (var point in m_arPointCloud.positions) s_Vertices.Add(point);
            }
            m_textArea.text += "Looped through" + k_newLine;
            mesh.Clear();
            m_textArea.text += "Mesh cleared" + k_newLine;
            mesh.SetVertices(s_Vertices);
            m_textArea.text += "Vertices set" + k_newLine;

            var indices = new int[s_Vertices.Count];
            for (int i = 0; i < s_Vertices.Count; ++i)
            {
                indices[i] = i;
            }

            mesh.SetIndices(indices, MeshTopology.Points, 0);

            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null) meshFilter.sharedMesh = mesh;
        }
        catch (Exception e)
        {
            m_textArea.text += e;
            throw;
        }
    }
}
