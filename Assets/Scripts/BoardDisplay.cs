﻿using UnityEngine;
using System.Collections;

public class BoardDisplay : MonoBehaviour {
    public enum DisplayMode {Mesh, Cube};
    public DisplayMode displayMode;

    // Because the current value of altitude is in the range of 0-100; 
    float defaultScale = 0.01f;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    void Start () {
        meshFilter = GetComponent<MeshFilter> ();
        meshRenderer = GetComponent<MeshRenderer> ();
    }

    public void DrawBoard (BoardNode[,] board) {
        float[,] heightMap = GenerateHeightMap (board);
        Color[] colorMap = GenerateColorMap (board);

        if (displayMode == DisplayMode.Mesh) {
            MeshData meshData = MeshGenerator.GenerateMeshData (heightMap);
            Mesh mesh = meshData.GenerateMesh ();
            meshFilter.sharedMesh = meshData.GenerateMesh ();

            int width = board.GetLength (0);
            int height = board.GetLength (1); 
            Texture2D texture = TextureGenerator.GenerateTexture (colorMap, width, height);
            meshRenderer.material.mainTexture = texture;
        } else if (displayMode == DisplayMode.Cube) {
            DrawBoardCubes (heightMap, colorMap);
        }
    }

    float[,] GenerateHeightMap (BoardNode[,] board) {
        int width = board.GetLength (0);
        int height = board.GetLength (1);
        float[,] heightMap = new float[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                BoardNode node = board[x,y];
                heightMap[x, y] = node.altitude * defaultScale;;
            }
        }

        return heightMap;
    }

    Color[] GenerateColorMap (BoardNode[,] board) {
        int width = board.GetLength (0);
        int height = board.GetLength (1);

        Color[] colorMap = new Color[width*height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                BoardNode node = board[x,y];
                colorMap[x + width*y] = new Color ((float)(node.temperature)/50f -.5f, 0, (float)(node.moisture) / 50f - .5f);
            }
        }

        return colorMap;
    }

    public void DrawBoardCubes (float[,] heightMap, Color[] colorMap) {
        int width = heightMap.GetLength (0);
        int height = heightMap.GetLength (1);

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                float scaledAltitude = heightMap[x,y];
                GameObject obj = GameObject.CreatePrimitive (PrimitiveType.Cube);
                obj.transform.localScale = new Vector3 (1, scaledAltitude, 1);
                obj.transform.localPosition = new Vector3 (x - width/2 + .5f, scaledAltitude / 2f, y - width/2 + .5f);
                obj.transform.parent = transform;
                obj.GetComponent<Renderer>().material.color = colorMap[x + width*y];
            }
        }
    }
}
