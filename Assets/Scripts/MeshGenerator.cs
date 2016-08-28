using UnityEngine;
using System.Collections;

public static class MeshGenerator {
    public static MeshData GenerateMeshData (float[,] heightMap) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        MeshData meshData = new MeshData (width, height);
 
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Vector3 position = new Vector3 (x - width/2, heightMap[x, y], y - height/2);
                meshData.AddVertex (position);
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
}

public class MeshData {
    Vector3[] verticies;
    int[] triangles;
    int triangleIndex;
    int vertexIndex;

    public MeshData (int width, int height) {
        verticies = new Vector3 [width * height];
        triangles = new int [(width - 1) * (height - 1) * 6];
        triangleIndex = 0;
        vertexIndex = 0;
    }

    public void AddVertex (Vector3 vertex) {
        verticies[vertexIndex] = vertex;
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
		mesh.RecalculateNormals ();
		return mesh;
    }
}
