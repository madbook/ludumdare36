using UnityEngine;
using System.Collections;

public class BoardDisplay : MonoBehaviour {
    public enum DisplayMode {Mesh, Cube, MeshWithBillboards};
    public DisplayMode displayMode;
    public enum ColorMode {Biome, Debug};
    public ColorMode colorMode;
    public Transform camera;

    Color tundraColor = new Color (.7f, .65f, .75f);
    Color desertColor = new Color (1, .85f, .5f);
    Color rainForestColor = new Color (.25f, .5f, 0);
    Color forestColor = new Color (0f, 1f, .25f);
    Color iceShelfColor = new Color (1, 1, 1);
    Color oceanColor = new Color (0, .25f, 1);

    // Snap altitude rendering to n discrete levels
    [Range(0, 100)]
    public int quantizationLevels;
    [Range(1, 100)]
    public int verticalScale;

    // Because the current value of altitude is in the range of 0-100; 
    const float MAX_ALTITUDE = 100;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    void Start () {
        meshFilter = GetComponent<MeshFilter> ();
        meshRenderer = GetComponent<MeshRenderer> ();
        MeshCollider meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
    }

    public void DrawBoard (BoardNode[,] board) {
        float[,] heightMap = GenerateHeightMap (board);
        Color[] colorMap;

        if (colorMode == ColorMode.Biome) {
            colorMap = GenerateBiomeColorMap (board);
        } else {
            colorMap = GenerateDebugColorMap (board);
        }

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

    Color[] GenerateDebugColorMap (BoardNode[,] board) {
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

    enum MoistureBiomeCategory {Desert, Forest, Water, Ice};
    enum TemperatureBiomeCategory {Freezing, Temperate, Tropical};

    Color[] GenerateBiomeColorMap (BoardNode[,] board) {
        int width = board.GetLength (0);
        int height = board.GetLength (1);

        Color[] colorMap = new Color[width*height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                BoardNode node = board[x,y];

                float scaledTemp = node.temperature / MAX_ALTITUDE;
                float scaledMoist = node.moisture / MAX_ALTITUDE;
                float scaledAltitude = node.altitude / MAX_ALTITUDE;

                float moistureOffset = scaledMoist - scaledTemp/3;
                MoistureBiomeCategory moistureCategory;

                if (moistureOffset < 0f) {
                    moistureCategory = MoistureBiomeCategory.Desert;
                } else if (moistureOffset < .33f) {
                    moistureCategory = MoistureBiomeCategory.Forest;
                } else if (moistureOffset < .66f) {
                    moistureCategory = MoistureBiomeCategory.Water;
                } else {
                    moistureCategory = MoistureBiomeCategory.Ice;
                }

                TemperatureBiomeCategory temperatureCategory;

                if (scaledTemp < .33f) {
                    temperatureCategory = TemperatureBiomeCategory.Freezing;
                } else if (scaledTemp < .66f) {
                    temperatureCategory = TemperatureBiomeCategory.Temperate;
                } else {
                    temperatureCategory = TemperatureBiomeCategory.Tropical;
                }

                Color color;
                Color color2;

                if (moistureCategory == MoistureBiomeCategory.Desert) {
                    color = desertColor;
                } else if (moistureCategory == MoistureBiomeCategory.Forest) {
                    if (temperatureCategory == TemperatureBiomeCategory.Freezing) {
                        color = tundraColor;
                    } else {
                        color = forestColor;
                    }
                } else if (moistureCategory == MoistureBiomeCategory.Water) {
                    if (temperatureCategory == TemperatureBiomeCategory.Tropical) {
                        color = rainForestColor;
                    } else {
                        color = oceanColor;
                    }
                } else {
                    color = iceShelfColor;
                }

                color2 = Color.Lerp (Color.black, Color.white, scaledAltitude);
                colorMap[x + width*y] = Color.Lerp (color, color2, .5f);
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

        //boop the collider into updating
        GetComponent<MeshCollider>().sharedMesh = null;
        GetComponent<MeshCollider>().sharedMesh = meshFilter.mesh;


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
