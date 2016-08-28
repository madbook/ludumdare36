using UnityEngine;
using System.Collections;

public static class BoardGenerator {
    private const float ALTITUDE_Y_OFFSET = 1234;
    private const float MOISTURE_Y_OFFSET = 2234;
    private const float TEMPERATURE_Y_OFFSET = 3234;

    public static BoardNode[,] GenerateBoard (int width, int height) {
		BoardNode[,] board = new BoardNode[width, height];
		
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
     
                int altitude = (int)(100*Mathf.PerlinNoise((float)x/10, (float)(y+ALTITUDE_Y_OFFSET)/ 10));
                int moisture = (int)(100 * Mathf.PerlinNoise((float)x / 10, (float)(y + MOISTURE_Y_OFFSET) / 10));
                int temperature = (int)(100 * Mathf.PerlinNoise((float)x / 10, (float)(y + TEMPERATURE_Y_OFFSET) / 10));

                Debug.Log(x + ", " + y + " alt: " + altitude + " moisture: " + moisture + " temp: " + temperature);

                board[x,y] = new BoardNode (altitude, moisture, temperature);
			}
		}

		return board;
	}
}

public struct BoardNode {
	public int altitude;
	public int moisture;
	public int temperature; 

	public BoardNode (int altitude, int moisture, int temperature) {
		this.altitude = altitude;
		this.moisture = moisture;
		this.temperature = temperature;
	}
}
