using UnityEngine;
using System.Collections;

public static class BiomeGenerator {
    const float BOARD_NODE_SCALE = 100;

    public static Biome[,] GenerateBiomeData (BoardNode[,] board) {
        int width = board.GetLength (0);
        int height = board.GetLength (1);
        Biome[,] biomeMap = new Biome[width,height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                BoardNode node = board[x, y];
                biomeMap[x,y] = GetBiome (node);
            }
        }

        return biomeMap;
    }

    public static Biome GetBiome (BoardNode node) {
        float scaledTemp = node.temperature / BOARD_NODE_SCALE;
        float scaledMoist = node.moisture / BOARD_NODE_SCALE;
        
        float moistureOffset = scaledMoist - scaledTemp/3;
        
        MoistureBiome moistureCategory;

        if (moistureOffset < 0f) {
            moistureCategory = MoistureBiome.Dry;
        } else if (moistureOffset < .33f) {
            moistureCategory = MoistureBiome.Moist;
        } else if (moistureOffset < .66f) {
            moistureCategory = MoistureBiome.Wet;
        } else {
            moistureCategory = MoistureBiome.Water;
        }

        TemperatureBiome temperatureCategory;

        if (scaledTemp < .33f) {
            temperatureCategory = TemperatureBiome.Cold;
        } else if (scaledTemp < .66f) {
            temperatureCategory = TemperatureBiome.Temperate;
        } else {
            temperatureCategory = TemperatureBiome.Tropical;
        }

        return new Biome (moistureCategory, temperatureCategory);
    }
}

public enum MoistureBiome {Dry, Moist, Wet, Water, Any};
public enum TemperatureBiome {Cold, Temperate, Tropical, Any};

public struct Biome {
    public readonly MoistureBiome moisture;
    public readonly TemperatureBiome temperature;

    public Biome (MoistureBiome moisture, TemperatureBiome temperature) {
        this.moisture = moisture;
        this.temperature = temperature;
    }

    public static bool operator == (Biome a, Biome b) {
        return (
            (a.moisture == b.moisture || a.moisture == MoistureBiome.Any || b.moisture == MoistureBiome.Any) &&
            (a.temperature == b.temperature || a.temperature == TemperatureBiome.Any || b.temperature == TemperatureBiome.Any)
        );
    }

    public static bool operator != (Biome a, Biome b) {
        return !(a == b);
    }

    public override bool Equals (object other) {
        try {
            return (bool)(this == (Biome)other);
        }
        catch {
            return false;
        }
    }

    // Added to stop VSCode from complaining at me.  Shouldn't need to hash these anyways.
    public override int GetHashCode () {
        return 0;
    }
}