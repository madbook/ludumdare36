using UnityEngine;
using System.Collections;

public class BoardDisplay : MonoBehaviour {
	// Because the current value of altitude is in the range of 0-100; 
	float defaultScale = 0.01f;

	MeshFilter meshFilter;

	void Start () {
		meshFilter = GetComponent<MeshFilter> ();
	}

	public void DrawBoard (BoardNode[,] board) {
		float[,] heightMap = GenerateHeightMap (board);
		MeshData meshData = MeshGenerator.GenerateMeshData (heightMap);
		Mesh mesh = meshData.GenerateMesh ();
		meshFilter.sharedMesh = meshData.GenerateMesh ();
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

	public void DrawBoardCubes (BoardNode[,] board) {
		int width = board.GetLength (0);
		int height = board.GetLength (1);

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				BoardNode node = board[x,y];
				float scaledAltitude = node.altitude * defaultScale;
				GameObject obj = GameObject.CreatePrimitive (PrimitiveType.Cube);
				obj.transform.localScale = new Vector3 (1, scaledAltitude, 1);
				obj.transform.localPosition = new Vector3 (x, scaledAltitude / 2f, y);
				obj.transform.parent = transform;
                obj.GetComponent<Renderer>().material.color = new Color((float)(node.temperature)/50f -.5f, 0, (float)(node.moisture) / 50f - .5f);

            }
		}
	}
}
