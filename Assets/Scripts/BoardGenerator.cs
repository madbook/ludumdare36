using UnityEngine;

public static class BoardGenerator {
    private const float ALTITUDE_Y_OFFSET = 2234;
    private const float MOISTURE_Y_OFFSET = 3234;
    private const float TEMPERATURE_Y_OFFSET = 4234;
    private const float EAST_WIND_Y_OFFSET = 5234;
    private const float SOUTH_WIND_Y_OFFSET = 6234;


    public static BoardNode[,] GenerateBoard (int width, int height) {
		BoardNode[,] board = new BoardNode[width, height];
		
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
     
                int altitude = (int)(100*Mathf.PerlinNoise((float)x/10, (float)(y+ALTITUDE_Y_OFFSET)/ 10));
                int moisture = (int)(100 * Mathf.PerlinNoise((float)x / 10, (float)(y + MOISTURE_Y_OFFSET) / 10));
                int temperature = (int)(100 * Mathf.PerlinNoise((float)x / 10, (float)(y + TEMPERATURE_Y_OFFSET) / 10));
                Vector2 wind = new Vector2();
                wind.x = Mathf.PerlinNoise((float)x / 10, (float)(y + EAST_WIND_Y_OFFSET) / 10) - .5f;
                wind.y = Mathf.PerlinNoise((float)x / 10, (float)(y + SOUTH_WIND_Y_OFFSET) / 10) - .5f;

                Debug.Log(x + ", " + y + " alt: " + altitude + " moisture: " + moisture + " temp: " + temperature);

                board[x,y] = new BoardNode (altitude, moisture, temperature, wind);
			}
		}

		return board;
	}

    public static BoardNode[,] CreateUniformBoard (int width, int height, BoardNode node) {
        BoardNode[,] board = new BoardNode[width, height];

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
                board[x,y] = node;
            }
        }

        return board;
    }
}

[System.Serializable]
public struct BoardNode {
	public int altitude;
	public int moisture;
	public int temperature;
    public Vector2 wind;

	public BoardNode (int altitude, int moisture, int temperature, Vector2 wind) {
		this.altitude = altitude;
		this.moisture = moisture;
		this.temperature = temperature;
        this.wind = wind;
	}

    public static bool operator == (BoardNode a, BoardNode b) {
        // Ignores wind for the time being.
        return (
            a.altitude == b.altitude &&
            a.moisture == b.moisture &&
            a.temperature == b.temperature
        );
    }

    public static bool operator != (BoardNode a, BoardNode b) {
        return !(a == b);
    }

    public override bool Equals (object other) {
        try {
            return (bool)(this == (BoardNode)other);
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
