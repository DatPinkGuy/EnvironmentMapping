using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

public class MeshPointLoading : MonoBehaviour
{
    private bool switchGizmo;
    private PointOctree<Vector3> pointTree;
    private string _path;
    private Dictionary<ulong, Vector3> _pointDictionary;
    private ParticleSystem m_ParticleSystem;
    private ParticleSystem.Particle[] m_Particles;
    private int m_NumParticles;
    private float minX;
    private float maxX;
    private float minY;
    private float maxY;
    private float minZ;
    private float maxZ;
    private List<PointOctreeNode<Vector3>> _nodeList;
    private List<GameObject> _octreeCubes;
    private bool _hasFileExplorerRan;
    private SaveCloud _saveCloud;
    private CanvasSampleOpenFileText _openFile;

    [Header("Variables used for octree")] 
    [SerializeField] private bool _createOnStart;
    [Tooltip("How small can octrees go")]
    [SerializeField] private float _octreeMinSize;
    [SerializeField] private GameObject _buildingPrefab;
    [SerializeField] private GameObject _cubePrefab;
    [SerializeField] private Object _pointAsset;
    [Tooltip("Number of points in octree needed to use that octree area later in boxing out")]
    [SerializeField] private int _pointAmountToInclude;
    [Header("Variables used for meshing (currently unused)")]
    [SerializeField] private Object _meshAsset;
    [SerializeField] private MeshFilter _meshFilter;
    [Header("Player")]
    [SerializeField] private GameObject _player;


    private void Start()
    {
        _openFile = FindObjectOfType<CanvasSampleOpenFileText>();
        _octreeCubes = new List<GameObject>();
#if (UNITY_EDITOR)
        if (!_createOnStart) return;
        _saveCloud = SaveSystem.TempLoad(_pointAsset);
        GenerateCloud();
#endif
    }

    private void Update()
    {
        if(!_createOnStart && !_pointAsset && !_hasFileExplorerRan) LoadCloud();
    }

    private void LoadCloud()
    {
        if (_openFile.path.Length <= 0) return;
        _saveCloud = SaveSystem.FileExplorerLoad(_openFile.path);
        if (_saveCloud == null) return;
        _hasFileExplorerRan = true;
        GenerateCloud();
    }
    
#if (UNITY_EDITOR || UNITY_STANDALONE)
    // public SaveCloud GetBytes() //for mesh serialization
    // {
    //     var bytes = SaveSystem.TempLoad(_meshAsset);
    //     return bytes;
    // }
    //
    // public void GenerateMesh()
    // {
    //     var mesh = MeshSerialization.ReadMesh(GetBytes().meshBytes);
    //     _meshFilter.mesh = mesh;
    // }

    public void GenerateCloud() //regenerate cloud based on save data
    {
        _pointDictionary = new Dictionary<ulong, Vector3>();
        for (int i = 0; i < _saveCloud.dictionaryKey.Length; i++)
        {
            if (minX > _saveCloud.pointX[i]) minX = _saveCloud.pointX[i];
            if (maxX < _saveCloud.pointX[i]) maxX = _saveCloud.pointX[i];
            if (minY > _saveCloud.pointY[i]) minY = _saveCloud.pointY[i];
            if (maxY < _saveCloud.pointY[i]) maxY = _saveCloud.pointY[i];
            if (minZ > _saveCloud.pointZ[i]) minZ = _saveCloud.pointZ[i];
            if (maxZ < _saveCloud.pointZ[i]) maxZ = _saveCloud.pointZ[i];
            _pointDictionary.Add(_saveCloud.dictionaryKey[i], new Vector3(_saveCloud.pointX[i], _saveCloud.pointY[i], _saveCloud.pointZ[i]));
        }
        PrepareForOctree();
        RenderPoints();
    }
#endif    

    private void PrepareForOctree()
    {
        //var minYAbs = Mathf.Abs(minY);
        //var minZAbs = Mathf.Abs(minZ);
        //var minMaxYOvr = minYAbs + maxY;
        //var minMaxZOvr = minZAbs + maxZ;
        var minXAbs = Mathf.Abs(minX);
        var minMaxXOvr = minXAbs + maxX;
        var minMaxYMid = (minY + maxY) / 2;
        var minMaxXMid = (minX + maxX) / 2;
        var minMaxZMid = (minZ + maxZ) / 2;

        CreateOctree(minMaxXOvr,minMaxXMid,minMaxYMid,minMaxZMid);
    }

    private void CreateOctree(float initialSize, float minMaxX,float minMaxY,float minMaxZ)
    {
        pointTree = new PointOctree<Vector3>(initialSize,
            new Vector3(minMaxX,minMaxY,minMaxZ), _octreeMinSize);
        foreach (var point in _pointDictionary)
        {
            pointTree.Add(point.Value,point.Value);
        }
        _nodeList = new List<PointOctreeNode<Vector3>> {pointTree.rootNode};
        StartCoroutine(LocateSmallestNodes(_nodeList, _pointAmountToInclude));
    }

    private void SetParticlePosition(int index, Vector3 position)
    {
        m_Particles[index].startColor = m_ParticleSystem.main.startColor.color;
        m_Particles[index].startSize = m_ParticleSystem.main.startSize.constant;
        m_Particles[index].position = position;
        m_Particles[index].remainingLifetime = 1f;
    }

    private void RenderPoints()
    {
        m_ParticleSystem = GetComponent<ParticleSystem>();
        // Make sure we have enough particles to store all the ones we want to draw
        int numParticles = _pointDictionary.Count;
        if (m_Particles == null || m_Particles.Length < numParticles)
        {
            m_Particles = new ParticleSystem.Particle[numParticles];
        }

        // Draw all the particles
        int particleIndex = 0;
        foreach (var kvp in _pointDictionary)
        {
            SetParticlePosition(particleIndex++, kvp.Value);
        }
      
        m_ParticleSystem.SetParticles(m_Particles, Math.Max(numParticles, m_NumParticles));
        m_NumParticles = numParticles;
    }

    private void OnDrawGizmos()
    {
        if (switchGizmo) return;
        pointTree?.DrawAllBounds();
        _nodeList?.Last().DrawAllObjects();
    }

    private IEnumerator LocateSmallestNodes(IEnumerable<PointOctreeNode<Vector3>> treesWithCHildren, int pointAmount)
    {
        var loop = false;
        var currentList = new List<PointOctreeNode<Vector3>>(treesWithCHildren);
        var nodesWithChildren = new List<PointOctreeNode<Vector3>>();
        var nodesWithObjects = new List<PointOctreeNode<Vector3>>();
        while (!loop)
        {
            if (nodesWithChildren.Count != 0) currentList = new List<PointOctreeNode<Vector3>>(nodesWithChildren);
            nodesWithObjects.Clear();
            nodesWithChildren.Clear();
            foreach (var tree in currentList)
            {
                switch (tree.HasChildren)
                {
                    case true:
                    {
                        foreach (var child in tree.children)
                        {
                            nodesWithChildren.AddRange(new[] {child});
                        }
                        break;
                    }
                    case false when tree.objects.Count > pointAmount:
                        nodesWithObjects.Add(tree);
                        break;
                }
            }
            if (nodesWithChildren.Count == 0) loop = true;
            yield return null;
        }
        _nodeList = nodesWithObjects;
        CreateCubesInTree();
    }

    private void CreateCubesInTree()
    {
        var parent = new GameObject("smallCubes");
        minX = 0;
        maxX = 0;
        minY = 0;
        maxY = 0;
        minZ = 0;
        maxZ = 0;
        foreach (var tree in _nodeList)
        {
            var go = Instantiate(_cubePrefab, tree.Center, quaternion.identity);
            go.transform.parent = parent.transform;
            go.transform.localScale = new Vector3(tree.SideLength, tree.SideLength, tree.SideLength);
            _octreeCubes.Add(go);
          
            //calculate min and max on each axis based on current _nodeList
            if (minX > go.transform.position.x) minX = go.transform.position.x;
            if (maxX < go.transform.position.x) maxX = go.transform.position.x;
            if (minY > go.transform.position.y) minY = go.transform.position.y;
            if (maxY < go.transform.position.y) maxY = go.transform.position.y;
            if (minZ > go.transform.position.z) minZ = go.transform.position.z;
            if (maxZ < go.transform.position.z) maxZ = go.transform.position.z;
        }
        GenerateRoomOctreeBased(_nodeList.First().SideLength);
    }

    private void CreateMultipleCubesInTree() //heavy on performance, currently unused
    {
        //
        var parent = new GameObject("smallCubes");
        minX = 0;
        maxX = 0;
        minY = 0;
        maxY = 0;
        minZ = 0;
        maxZ = 0;
        //
        foreach (var tree in _nodeList)
        {
            foreach (var point in tree.objects)
            {
                var go = Instantiate(_buildingPrefab, point.Pos, quaternion.identity);
                //var go not being placed in precise location, float deviation
                go.transform.parent = parent.transform;
                go.transform.localScale = new Vector3(tree.SideLength/2, tree.SideLength/2, tree.SideLength/2);
                _octreeCubes.Add(go);
                
                //calculate min and max on smallest children
                if (minX > go.transform.position.x) minX = go.transform.position.x;
                if (maxX < go.transform.position.x) maxX = go.transform.position.x;
                if (minY > go.transform.position.y) minY = go.transform.position.y;
                if (maxY < go.transform.position.y) maxY = go.transform.position.y;
                if (minZ > go.transform.position.z) minZ = go.transform.position.z;
                if (maxZ < go.transform.position.z) maxZ = go.transform.position.z;
            }
        }
        GenerateRoomOctreeBased(_nodeList.First().SideLength);
    }
    
    private void GenerateRoomOctreeBased(float sideLength)
    {
        var parent = new GameObject("roomOctree");
        var minYAbs = Mathf.Abs(minY);
        var minXAbs = Mathf.Abs(minX);
        var minZAbs = Mathf.Abs(minZ);
        var minMaxYOvr = minYAbs + maxY;
        var minMaxXOvr = minXAbs + maxX;
        var minMaxZOvr = minZAbs + maxZ;
        var minMaxYMid = (minY + maxY) / 2;
        var minMaxXMid = (minX + maxX) / 2;
        var minMaxZMid = (minZ + maxZ) / 2;
        var floor = Instantiate(_buildingPrefab, new Vector3(minMaxXMid,minY,minMaxZMid),Quaternion.identity);
        floor.name = "floor2";
        floor.transform.parent = parent.transform;
        if (_player)
        {
            var transformPosition = _player.transform.position;
            transformPosition.y = floor.transform.position.y;
            _player.transform.position = transformPosition;
        }
        var roof = Instantiate(_buildingPrefab, new Vector3(minMaxXMid,maxY,minMaxZMid),Quaternion.identity);
        roof.name = "roof2";
        roof.transform.parent = parent.transform;
        var wall1 = Instantiate(_buildingPrefab, new Vector3(minX,minMaxYMid,minMaxZMid),Quaternion.identity);
        wall1.name = "wall1-2";
        wall1.transform.parent = parent.transform;
        var wall2 = Instantiate(_buildingPrefab, new Vector3(maxX,minMaxYMid,minMaxZMid),Quaternion.identity);
        wall2.name = "wall2-2";
        wall2.transform.parent = parent.transform;
        var wall3 = Instantiate(_buildingPrefab, new Vector3(minMaxXMid,minMaxYMid,minZ),Quaternion.identity);
        wall3.name = "wall3-2";
        wall3.transform.parent = parent.transform;
        var wall4 = Instantiate(_buildingPrefab, new Vector3(minMaxXMid,minMaxYMid,maxZ),Quaternion.identity);
        wall4.name = "wall4-2";
        wall4.transform.parent = parent.transform;
        floor.transform.localScale = new Vector3(minMaxXOvr, sideLength, minMaxZOvr);
        roof.transform.localScale = new Vector3(minMaxXOvr, sideLength, minMaxZOvr);
        wall1.transform.localScale = new Vector3(sideLength, minMaxYOvr, minMaxZOvr);
        wall2.transform.localScale = new Vector3(sideLength, minMaxYOvr, minMaxZOvr);
        wall3.transform.localScale = new Vector3(minMaxXOvr, minMaxYOvr, sideLength);
        wall4.transform.localScale = new Vector3(minMaxXOvr, minMaxYOvr, sideLength);
    }
    
    public void SwitchGizmo()
    {
        switchGizmo = !switchGizmo;
    }

    public void SwitchGO()
    {
        Debug.Log(_octreeCubes.Count);
        foreach (var cube in _octreeCubes)
        {
            Debug.Log(cube.activeSelf);
            cube.SetActive(!cube.activeSelf);
        }
    }

    public void CheckPositionsOfCubes() //remnant method for checking created cubes position due to potential deviation
    {
        var mainObject = GameObject.Find("smallCubes");
        var list = mainObject.GetComponentsInChildren<Transform>();
        foreach (var cube in list)
        {
            Debug.Log(cube.position);
        }
    }
}
