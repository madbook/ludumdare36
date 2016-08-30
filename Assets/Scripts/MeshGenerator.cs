using UnityEngine;
using System.Collections;

public static class MeshGenerator {
    public static MeshData GenerateMeshData (float[,] heightMap) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        MeshData meshData = MeshData.CreateGridMesh (width, height);
 
        float uvOffsetX = .5f/width;
        float uvOffsetY = .5f/height;

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Vector3 position = new Vector3 (x - width/2 + .5f, heightMap[x, y], y - height/2 + .5f);
                Vector2 uv = new Vector2 (x/(float)width + uvOffsetX, y/(float)height + uvOffsetY);
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
        int numVerticies = numBorderVerticies + 2*width + 2*height + 1;
        int numTriangles = numBorderVerticies * 3 + 8;
        MeshData meshData = new MeshData (numVerticies, numTriangles);

        int[,] vertexIndicies = new int[width+2,height+2];
        int vertexIndex = 0;
        for (int x = -1; x <= width; x++) {
            for (int y = -1; y <= height; y++) {
                int edgeCount = 0;
                if (y == -1) edgeCount += 1;
                if (x == -1) edgeCount += 1;
                if (y == height) edgeCount += 1;
                if (x == width) edgeCount += 1;
                if (edgeCount > 1) {
                    vertexIndicies[x+1,y+1] = -1;
                } else if (x <= 0 || y <= 0 || x >= width - 1 || y >= height - 1) {
                    int altX = Mathf.Clamp (x, 0, width - 1);
                    int altY = Mathf.Clamp (y, 0, height - 1);
                    float altitude = heightMap[altX, altY];
                    if (edgeCount > 0) {
                        // altitude -= 2f;
                        altitude = -1;
                    }
                    meshData.AddVertex (new Vector3 (x - width/2 + .5f, altitude, y - height/2 + .5f));
                    vertexIndicies[x+1,y+1] = vertexIndex;
                    vertexIndex++;
                } else {
                    vertexIndicies[x+1,y+1] = -1;
                }
            }
        }
        meshData.AddVertex (new Vector3 (0, -5, 0));

        for (int x = 0; x < width - 1; x++) {
            int y = 0;
            int indexA = vertexIndicies[x+1,y+1];
            int indexB = vertexIndicies[x+2,y+1];
            int indexC = vertexIndicies[x+1,y];
            int indexD = vertexIndicies[x+2,y];
            // Debug.Log (x + "," + y + ": " + indexA + "," + indexB + "," + indexC + "," + indexD);
            meshData.AddTriangle (indexA, indexB, indexC);
            meshData.AddTriangle (indexB, indexD, indexC);
            meshData.AddTriangle (indexC, indexD, vertexIndex);

            y = height - 1;
            indexA = vertexIndicies[x+1,y+1];
            indexB = vertexIndicies[x+2,y+1];
            indexC = vertexIndicies[x+1,y+2];
            indexD = vertexIndicies[x+2,y+2];
            // Debug.Log (x + "," + y + ": " + indexA + "," + indexB + "," + indexC + "," + indexD);
            meshData.AddTriangle (indexA, indexC, indexB);
            meshData.AddTriangle (indexB, indexC, indexD);
            meshData.AddTriangle (indexD, indexC, vertexIndex);
        }

        for (int y = 0; y < height - 1; y++) {
            int x = 0;
            int indexA = vertexIndicies[x+1,y+1];
            int indexB = vertexIndicies[x+1,y+2];
            int indexC = vertexIndicies[x,y+1];
            int indexD = vertexIndicies[x,y+2];
            // Debug.Log (x + "," + y + ": " + indexA + "," + indexB + "," + indexC + "," + indexD);
            meshData.AddTriangle (indexA, indexC, indexB);
            meshData.AddTriangle (indexB, indexC, indexD);
            meshData.AddTriangle (indexD, indexC, vertexIndex);

            x = width - 1;
            indexA = vertexIndicies[x+1,y+1];
            indexB = vertexIndicies[x+1,y+2];
            indexC = vertexIndicies[x+2,y+1];
            indexD = vertexIndicies[x+2,y+2];
            // Debug.Log (x + "," + y + ": " + indexA + "," + indexB + "," + indexC + "," + indexD);
            meshData.AddTriangle (indexA, indexB, indexC);
            meshData.AddTriangle (indexB, indexD, indexC);
            meshData.AddTriangle (indexC, indexD, vertexIndex);
        }

        // The corners.
        meshData.AddTriangle (vertexIndicies[1,1], vertexIndicies[1,0], vertexIndicies[0,1]);
        meshData.AddTriangle (vertexIndicies[0,1], vertexIndicies[1,0], vertexIndex);
        meshData.AddTriangle (vertexIndicies[width,1], vertexIndicies[width+1,1], vertexIndicies[width,0]);
        meshData.AddTriangle (vertexIndicies[width,0], vertexIndicies[width+1,1], vertexIndex);
        meshData.AddTriangle (vertexIndicies[1,height], vertexIndicies[0,height], vertexIndicies[1,height+1]);
        meshData.AddTriangle (vertexIndicies[1,height+1], vertexIndicies[0,height], vertexIndex);
        meshData.AddTriangle (vertexIndicies[width,height], vertexIndicies[width,height+1], vertexIndicies[width+1,height]);
        meshData.AddTriangle (vertexIndicies[width+1,height], vertexIndicies[width,height+1], vertexIndex);

        return meshData;
    }

    public static MeshData GeneratePyramid (float height, float baseSize) {
        MeshData meshData = new MeshData (4, 3);

        float baseHeight = baseSize * 1.5f;
        float baseRemain = baseHeight - baseSize;
        float rightOffset = Mathf.Sqrt(Mathf.Pow(baseRemain, 2) + Mathf.Pow(baseHeight/2f, 2));

        meshData.AddVertex (new Vector3 (0, height/2f, 0));
        meshData.AddVertex (new Vector3 (0, -height/2f, -baseSize));
        meshData.AddVertex (new Vector3 (rightOffset, -height/2f, baseRemain));
        meshData.AddVertex (new Vector3 (-rightOffset, -height/2f, baseRemain));

        meshData.AddTriangle (2, 1, 0);
        meshData.AddTriangle (3, 2, 0);
        meshData.AddTriangle (1, 3, 0);

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
        mesh.uv = uvs;
		mesh.RecalculateNormals ();
		return mesh;
    }
}
