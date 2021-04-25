using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HullDelaunayVoronoi;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class GetPointPos : MonoBehaviour
{
    private ARPointCloud m_arPointCloud;
    private ARPointCloudManager m_arPointManager;
    private int m_lastArraySize;
    private List<ARPointCloud> points;
    private int lastKnowListSize;
    private MeshDrawing m_meshDrawing;
    private const string k_newLine = "\n";
    private bool _eventAdded;
    Dictionary<ulong, Vector3> m_Points = new Dictionary<ulong, Vector3>();
    public Dictionary<ulong, Vector3> Points => m_Points;
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private ConvexArTest _convexArTest;
    [SerializeField] private DelaunayTest _delaunayTest;
    [SerializeField] private Text m_textArea;
    [SerializeField] private Text m_textArea2;
    

    void Start()
    {
        m_arPointManager = FindObjectOfType<ARPointCloudManager>();
        if (!m_arPointManager) m_textArea2.text += "Didn't find ARPointCloudManager\n";
        m_meshDrawing = FindObjectOfType<MeshDrawing>();
    }
    
    void Update()
    {
        if(!m_arPointCloud) m_arPointCloud = FindObjectOfType<ARPointCloud>();
        if (m_arPointCloud && !_eventAdded)
        {
            m_arPointCloud.updated += GetPoints;
            _eventAdded = true;
        }
        if (!m_arPointCloud || m_arPointCloud.enabled == false) return;
        if (m_textArea.text.Length > 10000) m_textArea.text = "";
    }

    public void StopARDrawMesh()
    {
        m_textArea.text = "";
        try
        {
            m_textArea2.text += "There is " + m_Points.Count + " points in dictionary" + k_newLine;
            m_textArea2.text += "Started" + k_newLine;
            if(_convexArTest) _convexArTest.CreateConvexAndMesh();
            if(_delaunayTest) _delaunayTest.CalculateDelaunay(m_textArea2);
            m_textArea2.text += "Generated" + k_newLine;
            m_arPointManager.enabled = false;
            m_arPointCloud.enabled = false;
        }
        catch (Exception e)
        {
            m_textArea2.text += e;
            throw;
        }
    }
    
    public void RelocateCamera()
    {
        var startingRot = Quaternion.Euler(0,0,0);
        m_arPointManager.transform.rotation = startingRot;
    }

    private void GetPoints(ARPointCloudUpdatedEventArgs args)
    {
        if (!m_arPointCloud.positions.HasValue)
            return;

        var positions = m_arPointCloud.positions.Value;

        // Store all the positions over time associated with their unique identifiers
        if (m_arPointCloud.identifiers.HasValue)
        {
            var identifiers = m_arPointCloud.identifiers.Value;
            for (int i = 0; i < positions.Length; ++i)
            {
                m_Points[identifiers[i]] = positions[i];
            }
        }
    }

    public void Save()
    {
        try
        {
            var bytes = MeshSerialization.WriteMesh(_meshFilter.mesh, true);
            SaveSystem.SaveMesh(bytes);
        }
        catch (Exception e)
        {
            m_textArea2.text += e + k_newLine;
            throw;
        }
    }

    public void SaveCloud() //Separate Point Cloud into pieces for saving
    {
        try
        {
            var currentArray = 0;
            var tempVector = new Vector3();
            var dictionaryKey = new ulong[m_Points.Count];
            var pointX = new float[m_Points.Count];
            var pointY = new float[m_Points.Count];
            var pointZ = new float[m_Points.Count];
            foreach (var point in m_Points)
            {
                dictionaryKey[currentArray] = point.Key;
                tempVector = point.Value;
                pointX[currentArray] = tempVector.x;
                pointY[currentArray] = tempVector.y;
                pointZ[currentArray] = tempVector.z;
                currentArray++;
            }
            SaveSystem.SaveCloud(dictionaryKey, pointX, pointY, pointZ);
        }
        catch (Exception e)
        {
            m_textArea2.text += e + k_newLine;
            throw;
        }
    }
}
