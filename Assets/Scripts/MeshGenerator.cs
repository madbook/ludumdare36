using UnityEngine;

public static class MeshGenerator {
    public static MeshData GenerateMeshData (float[,] heightMap, int borderSize) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        int borderedWidth = width + 2*borderSize;
        int borderedHeight = width + 2*borderSize;

        MeshData meshData = MeshData.CreateGridMesh (borderedWidth, borderedHeight);

        float uvOffsetX = .5f/width;
        float uvOffsetY = .5f/height;

        for (int bX = 0; bX < borderedWidth; bX++) {
            for (int bY = 0; bY < borderedHeight; bY++) {
                int x = Mathf.Clamp (bX - borderSize, 0, width - 1);
                int y = Mathf.Clamp (bY - borderSize, 0, height - 1);

                Vector3 position = new Vector3 (bX - borderedWidth/2 + .5f, heightMap[x, y], bY - borderedHeight/2 + .5f);
                Vector2 uv = new Vector2 ((bX-1)/(float)width + uvOffsetX, (bY-1)/(float)height + uvOffsetY);
                meshData.AddVertex (position, uv);
            }
        }

        for (int y = 0; y < borderedHeight - 1; y++) {
            for (int x = 0; x < borderedWidth - 1; x++) {
                int a = x + (borderedWidth*y);
                int b = a + 1;
                int c = a + borderedWidth;
                int d = c + 1;
                // Debug.Log ("at (" + x + "," + y + ") we have " + a + "," + b + "," + c + "," + d);
                meshData.AddTriangle (a, b, c);
                meshData.AddTriangle (b, d, c);
            }
        }

        return meshData;
    }

    public static MeshData GenerateBottomMeshData (float[,] heightMap, int borderSize) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        int borderedWidth = width + 2*borderSize;
        int borderedHeight = height + 2*borderSize;
        int numBorderVerticies = 2 * (borderedWidth + borderedHeight - 2);
        int numVerticies = numBorderVerticies + 2*borderedWidth + 2*borderedHeight + 1;
        int numTriangles = numBorderVerticies * 3 + 8;
        MeshData meshData = new MeshData (numVerticies, numTriangles);

        int[,] vertexIndicies = new int[borderedWidth+2,borderedHeight+2];
        int vertexIndex = 0;
        for (int bX = -1; bX <= borderedWidth; bX++) {
            for (int bY = -1; bY <= borderedHeight; bY++) {
                int edgeCount = 0;
                if (bY == -1) edgeCount += 1;
                if (bX == -1) edgeCount += 1;
                if (bY == borderedHeight) edgeCount += 1;
                if (bX == borderedWidth) edgeCount += 1;
                if (edgeCount > 1) {
                    vertexIndicies[bX+1,bY+1] = -1;
                } else if (bX <= 0 || bY <= 0 || bX >= borderedWidth - 1 || bY >= borderedHeight - 1) {
                    int x = Mathf.Clamp (bX-borderSize, 0, width-1);
                    int y = Mathf.Clamp (bY-borderSize, 0, height-1);
                    float altitude = heightMap[x, y];
                    if (edgeCount > 0) {
                        altitude = -1;
                    }
                    meshData.AddVertex (new Vector3 (bX - borderedWidth/2 + .5f, altitude, bY - borderedHeight/2 + .5f));
                    vertexIndicies[bX+1,bY+1] = vertexIndex;
                    vertexIndex++;
                } else {
                    vertexIndicies[bX+1,bY+1] = -1;
                }
            }
        }
        meshData.AddVertex (new Vector3 (0, -5, 0));

        for (int x = 0; x < borderedWidth - 1; x++) {
            int y = 0;
            int indexA = vertexIndicies[x+1,y+1];
            int indexB = vertexIndicies[x+2,y+1];
            int indexC = vertexIndicies[x+1,y];
            int indexD = vertexIndicies[x+2,y];
            // Debug.Log (x + "," + y + ": " + indexA + "," + indexB + "," + indexC + "," + indexD);
            meshData.AddTriangle (indexA, indexB, indexC);
            meshData.AddTriangle (indexB, indexD, indexC);
            meshData.AddTriangle (indexC, indexD, vertexIndex);

            y = borderedHeight - 1;
            indexA = vertexIndicies[x+1,y+1];
            indexB = vertexIndicies[x+2,y+1];
            indexC = vertexIndicies[x+1,y+2];
            indexD = vertexIndicies[x+2,y+2];
            // Debug.Log (x + "," + y + ": " + indexA + "," + indexB + "," + indexC + "," + indexD);
            meshData.AddTriangle (indexA, indexC, indexB);
            meshData.AddTriangle (indexB, indexC, indexD);
            meshData.AddTriangle (indexD, indexC, vertexIndex);
        }

        for (int y = 0; y < borderedHeight - 1; y++) {
            int x = 0;
            int indexA = vertexIndicies[x+1,y+1];
            int indexB = vertexIndicies[x+1,y+2];
            int indexC = vertexIndicies[x,y+1];
            int indexD = vertexIndicies[x,y+2];
            // Debug.Log (x + "," + y + ": " + indexA + "," + indexB + "," + indexC + "," + indexD);
            meshData.AddTriangle (indexA, indexC, indexB);
            meshData.AddTriangle (indexB, indexC, indexD);
            meshData.AddTriangle (indexD, indexC, vertexIndex);

            x = borderedWidth - 1;
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
        meshData.AddTriangle (vertexIndicies[borderedWidth,1], vertexIndicies[borderedWidth+1,1], vertexIndicies[borderedWidth,0]);
        meshData.AddTriangle (vertexIndicies[borderedWidth,0], vertexIndicies[borderedWidth+1,1], vertexIndex);
        meshData.AddTriangle (vertexIndicies[1,borderedHeight], vertexIndicies[0,borderedHeight], vertexIndicies[1,borderedHeight+1]);
        meshData.AddTriangle (vertexIndicies[1,borderedHeight+1], vertexIndicies[0,borderedHeight], vertexIndex);
        meshData.AddTriangle (vertexIndicies[borderedWidth,borderedHeight], vertexIndicies[borderedWidth,borderedHeight+1], vertexIndicies[borderedWidth+1,borderedHeight]);
        meshData.AddTriangle (vertexIndicies[borderedWidth+1,borderedHeight], vertexIndicies[borderedWidth,borderedHeight+1], vertexIndex);

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
