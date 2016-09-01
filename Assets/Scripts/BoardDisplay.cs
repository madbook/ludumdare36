using UnityEngine;
using System.Collections.Generic;

public class BoardDisplay : MonoBehaviour {
    // Rendering options
    public bool drawHDTexture = true;
    public bool drawTerrainAsCubes = false;
    public bool drawBiomeDoodads = true;
    public bool drawBiomeClouds = true;
    public bool drawOnEditorUpdate = true;

    public BiomeTheme biomeColorTheme;
    public MeshFilter bottomMeshFilter;
    public Transform waterCube;
    public Material cloudMaterial;

    // Snap altitude rendering to n discrete levels
    const int quantizationLevels = 5;
    const int verticalScale = 4;
    // Because the current value of altitude is in the range of 0-100; 
    const float MAX_ALTITUDE = 100;
    // Adjust the water level to prevent z-fighting when water and altitude are both 0.
    const float waterLevelOffset = .1f;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    List<GameObject> doodads = new List<GameObject> ();
    // This is used in the calculation of water levels, though it can probably be simplified.
    Dictionary<AltitudeBiome,int> altitudeBiomeIndices = new Dictionary<AltitudeBiome,int> ();
    // This is necessary to enable autoUpdate.
    BoardNode[,] lastBoard;

    void Start () {
        meshFilter = GetComponent<MeshFilter> ();
        meshRenderer = GetComponent<MeshRenderer> ();
        meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();

        altitudeBiomeIndices.Add(AltitudeBiome.Valley, 0);
        altitudeBiomeIndices.Add(AltitudeBiome.Plain, 1);
        altitudeBiomeIndices.Add(AltitudeBiome.Hill, 2);
        altitudeBiomeIndices.Add(AltitudeBiome.Mountain, 3);
    }

    public void DrawBoard () {
        if (lastBoard != null) {
            DrawBoard (lastBoard);
        }
    }

    public void DrawBoard (BoardNode[,] board) {
        lastBoard = board;

        foreach (GameObject obj in doodads) {
            Destroy (obj);
        }
        doodads.Clear ();

        int width = board.GetLength (0);
        int height = board.GetLength (1);
        float[,] heightMap = GenerateHeightMap (board);
        Biome[,] biomeMap = BiomeGenerator.GenerateBiomeData (board);
        Color[] colorMap = GenerateBiomeColorMap (board, heightMap, biomeMap);

        if (drawTerrainAsCubes) {
            meshFilter.mesh = null;
            bottomMeshFilter.mesh = null;
            DrawBoardCubes (heightMap, colorMap);
        } else {
            DrawBoardMesh (heightMap, colorMap);
        }

        if (drawBiomeDoodads) {
            DrawBiomeDoodads (heightMap, biomeMap);
        }

        if (drawBiomeClouds) {
            DrawBiomeClouds (board, heightMap);
        }

        if (waterCube != null) {
            float waterLevel = GetWaterLevel (board);
            waterCube.transform.localScale = new Vector3 (width + 1, waterLevel, height + 1);
            waterCube.transform.localPosition = new Vector3 (0, waterLevel/2 - waterLevelOffset, 0);
        }
    }

    float GetWaterLevel (BoardNode[,] board) {
        int width = board.GetLength (0);
        int height = board.GetLength (1);
        int waterLevels = 4;
        float totalWater = 0;
        float waterLevel = 0f;
        int[] numberOfTilesPerLevel = new int[waterLevels];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                BoardNode node = board[x,y];
                int index = altitudeBiomeIndices[BiomeGenerator.GetAltitudeBiome(node.altitude)];
                totalWater += node.moisture;
                // Add volume for each level at or above this altitude.
                for (int i = index; i < waterLevels; i++) {
                    numberOfTilesPerLevel[i] += 1;
                }
            }
        }

        for (int i = 0; i < waterLevels; i++) {
            int tiles = numberOfTilesPerLevel[i];
            int availableVolumeOnThisLevel = tiles * 100;
            float waterOnThisLevel = totalWater / availableVolumeOnThisLevel;

            if (waterOnThisLevel > 1f) {
                waterLevel += 1f;
                totalWater = totalWater - availableVolumeOnThisLevel;
            } else {
                waterLevel += waterOnThisLevel;
                break;
            }
        }

        return waterLevel;
    }

    float[,] GenerateHeightMap (BoardNode[,] board) {
        int width = board.GetLength (0);
        int height = board.GetLength (1);
        float[,] heightMap = new float[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                BoardNode node = board[x,y];
                float altitude = node.altitude / MAX_ALTITUDE;
                altitude *= quantizationLevels - 1;
                altitude = Mathf.Round(altitude);
                altitude /= quantizationLevels - 1;
                heightMap[x,y] = altitude * verticalScale;
            }
        }

        return heightMap;
    }

    Color[] GenerateBiomeColorMap (BoardNode[,] board, float[,] heightMap, Biome[,] biomeMap) {
        int width = board.GetLength (0);
        int height = board.GetLength (1);

        int colorMapLength = (drawHDTexture) ? width*height*4 : width*height;
        Color[] colorMap = new Color[colorMapLength];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                BoardNode node = board[x,y];
                Biome biome = biomeMap[x,y];
                Color baseColor = ColorFromBiome (biome);
                Color altitudeColor = Color.Lerp (Color.black, Color.white, node.altitude / MAX_ALTITUDE);

                // Original thought was to have this be on a biome-basis, e.g. to only apply it to "fluid" biomes.
                // Each tile has a 2x2 pixel texture. If any quadrant of that texture has a neighboring tile with a
                // greater altitude, that tile's texture is blended into that pixel.
                if (drawHDTexture) {
                    Color colorA = baseColor;
                    Color colorB = baseColor;
                    Color colorC = baseColor;
                    Color colorD = baseColor;

                    bool nIsHigher = y > 0 && heightMap[x,y-1] > heightMap[x,y];
                    bool neIsHigher = y > 0 && x < width - 1 && heightMap[x+1,y-1] > heightMap[x,y];
                    bool eIsHigher = x < width - 1 && heightMap[x+1,y] > heightMap[x,y];
                    bool seIsHigher = x < width - 1 && y < height - 1 && heightMap[x+1,y+1] > heightMap[x,y];
                    bool sIsHigher = y < height - 1 && heightMap[x,y+1] > heightMap[x,y];
                    bool swIsHigher = y < height - 1 && x > 0 && heightMap[x-1,y+1] > heightMap[x,y];
                    bool wIsHigher = x > 0 && heightMap[x-1,y] > heightMap[x,y];
                    bool nwIsHigher = x > 0 && y > 0 && heightMap[x-1,y-1] > heightMap[x,y];

                    if (nwIsHigher) { colorA = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x-1,y-1]), .5f); }
                    else if (nIsHigher) { colorA = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x,y-1]), .5f); }
                    else if (wIsHigher) { colorA = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x-1,y]), .5f); }

                    if (neIsHigher) { colorB = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x+1,y-1]), .5f); }
                    else if (nIsHigher) { colorB = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x,y-1]), .5f); }
                    else if (eIsHigher) { colorB = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x+1,y]), .5f); }

                    if (swIsHigher) { colorC = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x-1,y+1]), .5f); }
                    else if (sIsHigher) { colorC = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x,y+1]), .5f); }
                    else if (wIsHigher) { colorC = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x-1,y]), .5f); }

                    if (seIsHigher) { colorD = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x+1,y+1]), .5f); }
                    else if (sIsHigher) { colorD = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x,y+1]), .5f); }
                    else if (eIsHigher) { colorD = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x+1,y]), .5f); }

                    colorMap[x*2 + width*y*4] = Color.Lerp (colorA, altitudeColor, .33f);
                    colorMap[x*2 + width*y*4 + 1] = Color.Lerp (colorB, altitudeColor, .33f);
                    colorMap[x*2 + width*y*4 + 2*width] = Color.Lerp (colorC, altitudeColor, .33f);
                    colorMap[x*2 + width*y*4 + 2*width + 1] = Color.Lerp (colorD, altitudeColor, .33f);
                } else {
                    colorMap[x + width*y] = Color.Lerp (baseColor, altitudeColor, .33f);
                }
            }
        }

        return colorMap;
    }

    public Color ColorFromBiome (Biome biome) {
        return biomeColorTheme.GetColor (biome);
    }

    public void DrawBoardMesh (float[,] heightMap, Color[] colorMap) {
        int width = heightMap.GetLength (0);
        int height = heightMap.GetLength (1);

        MeshData meshData = MeshGenerator.GenerateMeshData (heightMap);
        meshFilter.sharedMesh = meshData.GenerateMesh ();

        int textureMultiplier = (drawHDTexture) ? 2 : 1;
        Texture2D texture = TextureGenerator.GenerateTexture (colorMap, width * textureMultiplier, height * textureMultiplier);
        meshRenderer.material.mainTexture = texture;

        MeshData bottomMeshData = MeshGenerator.GenerateBottomMeshData (heightMap);
        bottomMeshFilter.sharedMesh = bottomMeshData.GenerateMesh ();

        //boop the collider into updating
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = meshFilter.mesh;
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
                doodads.Add (obj);
            }
        }
    }

    public void DrawBiomeClouds (BoardNode[,] board, float[,] heightMap) {
        int width = board.GetLength (0);
        int height = board.GetLength (1);

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                BoardNode node = board[x,y];
                Biome biome = BiomeGenerator.GetBiome (node);

                if (biome.moisture == MoistureBiome.Dry || biome.moisture == MoistureBiome.Moist) {
                    continue;
                }

                Vector3 scale;

                if (biome.moisture == MoistureBiome.Wet) {
                    scale = new Vector3 (.75f, .25f, .75f);
                } else {
                    scale = new Vector3 (1f, .5f, 1f);
                }
                float nodeHeight = heightMap[x,y];
                Vector3 position = new Vector3 (x - width/2 + .5f, nodeHeight/2 + 5, y - width/2 + .5f);

                GameObject obj = GameObject.CreatePrimitive (PrimitiveType.Cube);
                MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer> ();
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.receiveShadows = false;
                obj.transform.localScale = scale;
                obj.transform.localPosition = position;
                obj.transform.parent = transform;
                obj.GetComponent<MeshRenderer> ().sharedMaterial = cloudMaterial;
                doodads.Add (obj);
            }
        }
    }

    public void DrawBiomeDoodads (float[,] heightMap, Biome[,] biomeMap) {
        int width = heightMap.GetLength (0);
        int height = heightMap.GetLength (1);

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Biome biome = biomeMap[x,y];
                BiomeTheme.Doodad doodad = biomeColorTheme.GetDoodad (biome);

                if (doodad != null) {
                    GameObject obj = new GameObject ("Biome Doodad");
                    obj.AddComponent<MeshRenderer> ();
                    obj.AddComponent<MeshFilter> ();
                    obj.GetComponent<MeshFilter> ().sharedMesh = doodad.mesh;
                    obj.GetComponent<Renderer>().material = doodad.material;
                    obj.GetComponent<Renderer>().material.color = doodad.color;
                    obj.transform.localPosition = new Vector3 (x - width/2 + .5f, heightMap[x,y] + doodad.height/2, y - width/2 + .5f);
                    obj.transform.parent = transform;
                    doodads.Add (obj);
                }
            }
        }
    }
}
