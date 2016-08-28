using UnityEngine;
using System.Collections;

public class BoardDisplay : MonoBehaviour {
    public enum DisplayMode {Mesh, Cube, MeshWithBillboards};
    public DisplayMode displayMode;
    public Transform camera;

    // Snap altitude rendering to n discrete levels
    [Range(0, 100)]
    public int quantizationLevels;
    [Range(1, 100)]
    public int verticalScale;

    // Because the current value of altitude is in the range of 0-100; 
    const float MAX_ALTITUDE = 100;

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
            DrawMesh (heightMap, colorMap);
        } else if (displayMode == DisplayMode.Cube) {
            DrawBoardCubes (heightMap, colorMap);
        } else if (displayMode == DisplayMode.MeshWithBillboards) {
            DrawMesh (heightMap, colorMap);
            DrawBillboards (board, heightMap);
        }
    }

    float[,] GenerateHeightMap (BoardNode[,] board) {
        int width = board.GetLength (0);
        int height = board.GetLength (1);
        float[,] heightMap = new float[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                BoardNode node = board[x,y];
                float altitude = node.altitude / MAX_ALTITUDE;
                if (quantizationLevels > 1) {
                    altitude *= quantizationLevels - 1;
                    altitude = Mathf.Round(altitude);
                    altitude /= quantizationLevels - 1;
                } else if (quantizationLevels == 1) {
                    altitude = 0f;
                }
                heightMap[x,y] = altitude * verticalScale;
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

    public void DrawMesh (float[,] heightMap, Color[] colorMap) {
        int width = heightMap.GetLength (0);
        int height = heightMap.GetLength (1);

        MeshData meshData = MeshGenerator.GenerateMeshData (heightMap);
        Mesh mesh = meshData.GenerateMesh ();
        meshFilter.sharedMesh = meshData.GenerateMesh ();

        Texture2D texture = TextureGenerator.GenerateTexture (colorMap, width, height);
        meshRenderer.material.mainTexture = texture;
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

    public void DrawBillboards (BoardNode[,] board, float[,] heightMap) {
        int width = heightMap.GetLength (0);
        int height = heightMap.GetLength (1);

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                BoardNode node = board[x,y];
                if (node.temperature > 50 && node.moisture > 50) {
                    // GameObject obj = GameObject.CreatePrimitive (PrimitiveType.Cube);
                    GameObject obj = GameObject.CreatePrimitive (PrimitiveType.Quad);
                    obj.AddComponent<Billboard> ();
                    obj.GetComponent<Billboard> ().camera = camera;
                    // Some of this won't be necessary, since we can set up textures properly.
                    MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer> ();
                    meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    meshRenderer.receiveShadows = false;

                    obj.transform.localScale = new Vector3 (0.25f, 0.5f, 0.25f);
                    obj.transform.localPosition = new Vector3 (x - width/2 + .5f, heightMap[x,y] + .25f, y - width/2 + .5f);
                    obj.transform.parent = transform;
                    obj.GetComponent<Renderer>().material.color = Color.Lerp (Color.black, Color.red, .66f);
                }
            }
        }
    }
}
