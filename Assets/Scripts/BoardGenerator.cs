using UnityEngine;
using System.Collections;

public static class BoardGenerator {
	public static BoardNode[,] GenerateBoard (int width, int height, int seed) {
		BoardNode[,] board = new BoardNode[width, height];
		
		System.Random prng = new System.Random(seed);

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				int altitude = prng.Next (0, 100);
				int moisture = prng.Next (0, 100);
				int temperature = prng.Next (0, 100);
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
