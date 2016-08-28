using UnityEngine;
using System.Collections;

public static class MeshGenerator {
    public static MeshData GenerateMeshData (float[,] heightMap) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        MeshData meshData = MeshData.CreateGridMesh (width, height);

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Vector3 position = new Vector3 (x - width/2 + .5f, heightMap[x, y], y - height/2 + .5f);
                Vector2 uv = new Vector2 (x/(float)width, y/(float)height);
                meshData.AddVertex (position, uv);
            }
        }

        for (int y = 0; y < height - 1; y++) {
            for (int x = 0; x < width - 1; x++) {
                int a = x + (width*y);
                int b = a + 1;
                int c = a + width;
                int d = c + 1;
                // Debug.Log ("at (" + x + "," + y + ") we have " + a + "," + b + "," + c + "," + d);
                meshData.AddTriangle (a, b, c);
                meshData.AddTriangle (b, d, c);
            }
        }

        return meshData;
    }

    public static MeshData GenerateBottomMeshData (float[,] heightMap) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        int numBorderVerticies = 2 * (width + height - 2);
        MeshData meshData = new MeshData (numBorderVerticies + 1, numBorderVerticies);

        for (int x = 0; x < width - 1; x++) {
            int y = 0;
            Vector3 position = new Vector3 (x - width/2 + .5f, heightMap[x, y], y - height/2 + .5f);
            Vector2 uv = new Vector2 (x/(float)width, y/(float)height);
            meshData.AddVertex (position, uv);
        }

        for (int y = 0; y < height - 1; y++) {
            int x = width - 1;
            Vector3 position = new Vector3 (x - width/2 + .5f, heightMap[x, y], y - height/2 + .5f);
            Vector2 uv = new Vector2 (x/(float)width, y/(float)height);
            meshData.AddVertex (position, uv);
        }

        for (int x = width - 1; x > 0; x--) {
            int y = height - 1;
            Vector3 position = new Vector3 (x - width/2 + .5f, heightMap[x, y], y - height/2 + .5f);
            Vector2 uv = new Vector2 (x/(float)width, y/(float)height);
            meshData.AddVertex (position, uv);
        }

        for (int y = height - 1; y > 0; y--) {
            int x = 0;
            Vector3 position = new Vector3 (x - width/2 + .5f, heightMap[x, y], y - height/2 + .5f);
            Vector2 uv = new Vector2 (x/(float)width, y/(float)height);
            meshData.AddVertex (position, uv);
        }

        Vector2 bottomCenterUV = new Vector2 (.5f, .5f);
        Vector3 bottomCenterPosition = new Vector3 (0, -5, 0);
        meshData.AddVertex (bottomCenterPosition, bottomCenterUV);
        int bottomVerticiesLength = meshData.verticies.Length;
        int lastVertexIndex = bottomVerticiesLength - 1;
        for (int i = 0; i < bottomVerticiesLength - 2; i++) {
            meshData.AddTriangle (i, i+1, lastVertexIndex);
        }
        // Complete the loop.
        meshData.AddTriangle (bottomVerticiesLength - 2, 0, lastVertexIndex);
        Debug.Log ((bottomVerticiesLength - 2) + "," + 0 + "," + lastVertexIndex);

        return meshData;
    }
}

public class MeshData {
    public Vector3[] verticies;
    Vector2[] uvs;
    int[] triangles;
    int triangleIndex;
    int vertexIndex;

    public MeshData (int numVerticies, int numTriangles) {
        verticies = new Vector3 [numVerticies];
        uvs = new Vector2 [numVerticies];
        triangles = new int [numTriangles * 3];
        triangleIndex = 0;
        vertexIndex = 0;
    }

    public static MeshData CreateGridMesh (int width, int height) {
        int numVerticies = width * height;
        int numTriangles = (width - 1) * (height * 1) * 2;
        return new MeshData (numVerticies, numTriangles);
    }

    public void AddVertex (Vector3 vertex, Vector2 uv) {
        verticies[vertexIndex] = vertex;
        uvs[vertexIndex] = uv;
        vertexIndex += 1;
    }

    public void AddTriangle (int indexA, int indexB, int indexC) {
        triangles[triangleIndex] = indexA;
        triangles[triangleIndex+1] = indexB;
        triangles[triangleIndex+2] = indexC;
        triangleIndex += 3;
    }

    public Mesh GenerateMesh () {
        Mesh mesh = new Mesh ();
		mesh.vertices = verticies;
		mesh.triangles = triangles;
        mesh.uv = uvs;
		mesh.RecalculateNormals ();
		return mesh;
    }
}
