using System.Collections;
using System.Collections.Generic;
using HullDelaunayVoronoi.Hull;
using HullDelaunayVoronoi.Primitives;
using UnityEngine;

public class ConvexTest : MonoBehaviour
{
     public int NumberOfVertices = 1000;

        public float size = 5;

        public int seed = 0;

        private Material lineMaterial;

        private Mesh mesh;

        private Matrix4x4 rotation = Matrix4x4.identity;

        private float theta;

        private ConvexHull3 hull;
        
        //Added
        [SerializeField] private GameObject meshObject;

        private void Start()
        {
            lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));

            mesh = new Mesh();
            Vertex3[] vertices = new Vertex3[NumberOfVertices];

            Vector3[] meshVerts = new Vector3[NumberOfVertices];
            int[] meshIndices = new int[NumberOfVertices];

            Random.InitState(seed);
            for (int i = 0; i < NumberOfVertices; i++)
            {
                float x = size * Random.Range(-1.0f, 1.0f);
                float y = size * Random.Range(-1.0f, 1.0f);
                float z = size * Random.Range(-1.0f, 1.0f);

                vertices[i] = new Vertex3(x, y, z);

                meshVerts[i] = new Vector3(x, y, z);
                meshIndices[i] = i;
            }

            mesh.vertices = meshVerts;
            mesh.SetIndices(meshIndices, MeshTopology.Points, 0);

            hull = new ConvexHull3();
            hull.Generate(vertices);
            GenerateMesh();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.KeypadMinus))
            {
                theta += (Input.GetKey(KeyCode.KeypadPlus)) ? 0.005f : -0.005f;

                rotation[0, 0] = Mathf.Cos(theta);
                rotation[0, 2] = Mathf.Sin(theta);
                rotation[2, 0] = -Mathf.Sin(theta);
                rotation[2, 2] = Mathf.Cos(theta);
            }

            Graphics.DrawMesh(mesh, rotation, lineMaterial, 0, Camera.main);
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(20, 20, Screen.width, Screen.height), "Numpad +/- to Rotate");
        }

        private void OnPostRender()
        {

            if (hull == null || hull.Simplexs.Count == 0 || hull.Vertices.Count == 0) return;

            GL.PushMatrix();

            GL.LoadIdentity();
            GL.MultMatrix(GetComponent<Camera>().worldToCameraMatrix * rotation);
            GL.LoadProjectionMatrix(GetComponent<Camera>().projectionMatrix);

            lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(Color.red);

            foreach (Simplex<Vertex3> f in hull.Simplexs)
            {
                DrawSimplex(f);
            }

            GL.End();

            GL.PopMatrix();
        }

        private void DrawSimplex(Simplex<Vertex3> f)
        {
            GL.Vertex3(f.Vertices[0].X, f.Vertices[0].Y, f.Vertices[0].Z);
            GL.Vertex3(f.Vertices[1].X, f.Vertices[1].Y, f.Vertices[1].Z);

            GL.Vertex3(f.Vertices[0].X, f.Vertices[0].Y, f.Vertices[0].Z);
            GL.Vertex3(f.Vertices[2].X, f.Vertices[2].Y, f.Vertices[2].Z);

            GL.Vertex3(f.Vertices[1].X, f.Vertices[1].Y, f.Vertices[1].Z);
            GL.Vertex3(f.Vertices[2].X, f.Vertices[2].Y, f.Vertices[2].Z);
        }

        private void GenerateMesh()
        {
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();

            for (int i = 0; i < hull.Simplexs.Count; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Vector3 v = new Vector3();
                    v.x = hull.Simplexs[i].Vertices[j].X;
                    v.y = hull.Simplexs[i].Vertices[j].Y;
                    v.z = hull.Simplexs[i].Vertices[j].Z;

                    positions.Add(v);
                }

                Vector3 n = new Vector3();
                n.x = hull.Simplexs[i].Normal[0];
                n.y = hull.Simplexs[i].Normal[1];
                n.z = hull.Simplexs[i].Normal[2];

                // if (hull.Simplexs[i].IsNormalFlipped)
                // {
                //     indices.Add(i * 3 + 2);
                //     indices.Add(i * 3 + 1);
                //     indices.Add(i * 3 + 0);
                // }
                // else
                // {
                //     indices.Add(i * 3 + 0);
                //     indices.Add(i * 3 + 1);
                //     indices.Add(i * 3 + 2);
                // }
                
                if (hull.Simplexs[i].IsNormalFlipped)
                {
                    indices.Add(i * 3 + 0);
                    indices.Add(i * 3 + 1);
                    indices.Add(i * 3 + 2);
                }
                else
                {
                    indices.Add(i * 3 + 2);
                    indices.Add(i * 3 + 1);
                    indices.Add(i * 3 + 0);
                }

                normals.Add(n);
                normals.Add(n);
                normals.Add(n);
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(positions);
            mesh.SetNormals(normals);
            mesh.SetTriangles(indices, 0);

            mesh.RecalculateBounds();
            
            meshObject.GetComponent<MeshFilter>().mesh = mesh;
        }
}
