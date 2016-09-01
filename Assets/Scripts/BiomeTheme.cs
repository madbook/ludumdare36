using UnityEngine;

public class BiomeTheme : MonoBehaviour {
    public Material material;

    public Color desertColor = new Color (1, .85f, .5f);
    public Color iceShelfColor = new Color (1, 1, 1);
    public Color tundraColor = new Color (.7f, .65f, .75f);
    public Color rainForestColor = new Color (.25f, .5f, 0);
    public Color deepWaterColor = new Color (0, .25f, 1);
    public Color shallowWaterColor = new Color (0, .5f, 1);
    public Color forestColor = new Color (0f, .75f, .25f);
    public Color mountainForestColor = new Color (.5f, .5f, .25f);
    public Color swampColor = new Color (.25f, .25f, .15f);
    public Color plainsColor = new Color (.5f, .75f, .25f);
    public Color borealColor = new Color (.25f, .5f, .5f);
    public Color mountainBorealColor = new Color (.65f, .75f, .75f);

    // To help spot holes in biome coverage.
    Color defaultColor = Color.magenta;

    static Biome desertBiome = new Biome (MoistureBiome.Dry, TemperatureBiome.Any, AltitudeBiome.Any);
    static Biome iceShelfBiome = new Biome (MoistureBiome.Water, TemperatureBiome.Any, AltitudeBiome.Any);
    static Biome tundraBiome = new Biome (MoistureBiome.Moist, TemperatureBiome.Cold, AltitudeBiome.Any);
    static Biome rainForestBiome = new Biome (MoistureBiome.Wet, TemperatureBiome.Tropical, AltitudeBiome.Any);
    static Biome deepWaterBiome = new Biome (MoistureBiome.Wet, TemperatureBiome.Any, AltitudeBiome.Valley);
    static Biome shallowWaterBiome = new Biome (MoistureBiome.Wet, TemperatureBiome.Any, AltitudeBiome.Plain);
    static Biome forestBiome = new Biome (MoistureBiome.Wet, TemperatureBiome.Any, AltitudeBiome.Hill);
    static Biome mountainForestBiome = new Biome (MoistureBiome.Wet, TemperatureBiome.Any, AltitudeBiome.Mountain);
    static Biome swampBiome = new Biome (MoistureBiome.Moist, TemperatureBiome.Any, AltitudeBiome.Valley);
    static Biome plainsBiome = new Biome (MoistureBiome.Moist, TemperatureBiome.Any, AltitudeBiome.Plain);
    static Biome borealBiome = new Biome (MoistureBiome.Moist, TemperatureBiome.Any, AltitudeBiome.Hill);
    static Biome mountainBorealBiome = new Biome (MoistureBiome.Moist, TemperatureBiome.Any, AltitudeBiome.Mountain);

    static DoodadSpec borealDoodadSpec = new DoodadSpec (1.5f, .28f);
    Doodad borealDoodad;
    static DoodadSpec mountainBorealDoodadSpec = new DoodadSpec (1f, .3f);
    Doodad mountainBorealDoodad;
    static DoodadSpec mountainForestDoodadSpec = new DoodadSpec (.7f, .32f);
    Doodad mountainForestDoodad;

    public Color GetColor (Biome biome) {
        if (biome == desertBiome) {
            return desertColor;
        } else if (biome == iceShelfBiome) {
            return iceShelfColor;
        } else if (biome == tundraBiome) {
            return tundraColor;
        } else if (biome == rainForestBiome) {
            return rainForestColor;
        } else if (biome == deepWaterBiome) {
            return deepWaterColor;
        } else if (biome == shallowWaterBiome) {
            return shallowWaterColor;
        } else if (biome == forestBiome) {
            return forestColor;
        } else if (biome == mountainForestBiome) {
            return mountainForestColor;
        } else if (biome == swampBiome) {
            return swampColor;
        } else if (biome == plainsBiome) {
            return plainsColor;
        } else if (biome == borealBiome) {
            return borealColor;
        } else if (biome == mountainBorealBiome) {
            return mountainBorealColor;
        } else {
            return defaultColor;
        }
    }

    public void ResetDoodads () {
        if (borealDoodad != null)
            borealDoodad = null;
        if (mountainBorealDoodad != null)
            mountainBorealDoodad = null;
        if (mountainForestDoodad != null)
            mountainForestDoodad = null;
    }

    public Doodad GetDoodad (Biome biome) {
        if (biome == borealBiome) {
            if (borealDoodad == null) {
                borealDoodad = new Doodad (borealColor, borealDoodadSpec, material);
            }
            return borealDoodad;
        } else if (biome == mountainBorealBiome) {
            if (mountainBorealDoodad == null) {
                mountainBorealDoodad = new Doodad (mountainBorealColor, mountainBorealDoodadSpec, material);
            }
            return mountainBorealDoodad;
        } else if (biome == mountainForestBiome) {
            if (mountainForestDoodad == null) {
                mountainForestDoodad = new Doodad (mountainForestColor, mountainForestDoodadSpec, material);
            }
            return mountainForestDoodad;
        } else {
            return null;
        }
    }

    public struct DoodadSpec {
        public readonly float height;
        public readonly float baseSize;

        public DoodadSpec (float height, float baseSize) {
            this.height = height;
            this.baseSize = baseSize;
        }
    }

    public class Doodad {
        public readonly Color color;
        public readonly float height;
        public readonly float baseSize;
        public readonly Mesh mesh;
        public readonly Material material;

        public Doodad (Color color, DoodadSpec spec, Material material) {
            this.color = color;
            this.height = spec.height;
            this.baseSize = spec.baseSize;
            this.mesh = MeshGenerator.GeneratePyramid (height, baseSize).GenerateMesh ();
            this.material = material;
        }
    }
}
