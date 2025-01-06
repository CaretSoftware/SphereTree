using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace StarParticles {
    public class TriangleMeshGenerator : MonoBehaviour {
        [Header("Mesh Parameters")]
        public int _triangleCount = 1000; // 16_777_216; Total number of triangles (max value for 256x256x256 RGB colors)
        public float _spacing = 0.001f;         // Spacing between triangles

        // 65K vertices Unity limit (21_666 Triangles) !!! 
        [ContextMenu("Generate Mesh")]
        public void GenerateMesh() {
            // Ensure triangle count is within limits
            _triangleCount = Mathf.Clamp(_triangleCount, 1, 21_666);// 16_777_216);

            // Initialize mesh data
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[_triangleCount * 3];
            int[] triangles = new int[_triangleCount * 3];
            Color[] colors = new Color[_triangleCount * 3];
            Vector2[] uvs = new Vector2[_triangleCount * 3]; // Add UVs
            // Generate the triangles
            int vertexIndex = 0;
            int triangleIndex = 0;
            float zOffset = -(_triangleCount / 2f * _spacing);

            for (int i = 0; i < _triangleCount; i++)
            {
                // Compute unique color for the triangle
                Color color = UniqueColor(i);

                // Define vertices of the triangle
                Vector3 v0 = new Vector3(0f,        1f,     zOffset);   // top
                Vector3 v1 = new Vector3(0.86605f,  -0.5f,  zOffset);   // right
                Vector3 v2 = new Vector3(-0.86605f, -0.5f,  zOffset);   // left

                vertices[vertexIndex]           = v0;
                vertices[vertexIndex + 1]       = v1;
                vertices[vertexIndex + 2]       = v2;

                // Assign UVs for the triangle (e.g., simple mapping for an equilateral triangle)
                uvs[vertexIndex]                = v0; // Top-center
                uvs[vertexIndex + 1]            = v1; // Bottom-left
                uvs[vertexIndex + 2]            = v2; // Bottom-right
                
                // Assign color to all vertices of the triangle
                colors[vertexIndex]             = color;
                colors[vertexIndex + 1]         = color;
                colors[vertexIndex + 2]         = color;

                // Define the triangle indices
                triangles[triangleIndex]        = vertexIndex;
                triangles[triangleIndex + 1]    = vertexIndex + 1;
                triangles[triangleIndex + 2]    = vertexIndex + 2;

                // Increment indices and zOffset
                vertexIndex += 3;
                triangleIndex += 3;
                zOffset += _spacing;
            }

            // Assign mesh data
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;
            mesh.uv = uvs; // Set UVs
            
            // Recalculate bounds
            // mesh.RecalculateBounds();
            mesh.bounds = new Bounds(Vector3.zero, 1_000 * Vector3.one);
            mesh.RecalculateNormals();

            // Save the mesh as an asset
            SaveMeshAsAsset(mesh);
        }

        private Color UniqueColor(int index) { 
            // Calculate RGB components from index
            int r = (index & 0xFF);
            int g = ((index >> 8) & 0xFF);
            int b = ((index >> 16) & 0xFF);

            // Convert to normalized [0, 1] color
            return new Color(r / 255f, g / 255f, b / 255f);
        }

        private static void SaveMeshAsAsset(Mesh mesh) {
            string path = "Assets/GeneratedMesh.asset";
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"Mesh saved as asset at: {path}");
        }
    }
}