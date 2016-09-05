using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public int width;
    public int height;
    public bool upateEachTick = true;
    public bool useTemplateNode = false;

    public BoardNode templateNode;
    public Text actionInfoText;

    //The amount adjacent blocks "blur" their props per tick.  Magnified by 4, since 4 cardinal neighbors influence you.
    float ATMOSPHERIC_DIFFUSION = .01f;

    BoardNode[,] board;
    Size boardSize;
    BoardDisplay display;
    Position cursorPosition;

    void Start() {
        if (useTemplateNode) {
            board = BoardGenerator.CreateUniformBoard (width, height, templateNode);
        } else {
            board = BoardGenerator.GenerateBoard(width, height);
        }

        display = FindObjectOfType<BoardDisplay>();
        if (display != null) {
            display.DrawBoard(board);
        }
    }

    void SetInspectText (BoardNode node, Biome biome, int x, int y) {
        actionInfoText.text = "Inspecting {" + x + "," + y + "}: " + biome.moisture + "(" + node.moisture + ") " + biome.altitude + "(" + node.altitude + ") " + biome.temperature + "(" + node.temperature + ")";
    }
   
    void Update () {
        if (upateEachTick) {
            ApplyAtmosphericDiffusion ();
        }
    }

    void ApplyAtmosphericDiffusion () {
        BoardNode[, ] board_after = new BoardNode[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                BoardNode old = board[x, y];

                // If a tile is on the edge, it's starting condition is weighed more (since there isn't diffusion from off the board).
                float edges = 0;
                if(x == 0 || x == width - 1)
                {
                    edges++;
                }
                if (y == 0 || y == height - 1)
                {
                    edges++;
                }
                
                                                        // I don't understand why this isn't 4.
                float newMoisture = old.moisture * (1f - (3.5f-edges) * ATMOSPHERIC_DIFFUSION);
                float newTemperature = old.temperature * (1f - (3.5f - edges) * ATMOSPHERIC_DIFFUSION);
                
                 if (x > 0)
                 {
                    newMoisture += board[x - 1, y].moisture * ATMOSPHERIC_DIFFUSION;
                    newTemperature += board[x - 1, y].temperature * ATMOSPHERIC_DIFFUSION;
                 }
                 if (x < width-1)
                 {
                    newMoisture += board[x + 1, y].moisture * ATMOSPHERIC_DIFFUSION;
                    newTemperature += board[x + 1, y].temperature * ATMOSPHERIC_DIFFUSION;
                 }
                 if (y > 0)
                 {
                    newMoisture +=board[x, y-1].moisture * ATMOSPHERIC_DIFFUSION;
                    newTemperature += board[x, y-1].temperature * ATMOSPHERIC_DIFFUSION;
                 }
                if (y < height-1)
                 {
                    newMoisture += board[x, y + 1].moisture * ATMOSPHERIC_DIFFUSION;
                    newTemperature += board[x, y + 1].temperature * ATMOSPHERIC_DIFFUSION;
                 }
                board_after[x, y] = new BoardNode(old.altitude, (int)newMoisture, (int)newTemperature, old.wind);
            }
            
        }
        board = board_after;

        if (display != null) {
            display.DrawBoard(board);
        }
    }

    public class Position {
        public readonly int row;
        public readonly int col;

        public Position (int row, int col) {
            this.row = row;
            this.col = col;
        }
    }

    public struct Size {
        public readonly int width;
        public readonly int height;

        public Size (int width, int height) {
            this.width = width;
            this.height = height;
        }
    }

    public enum Action {
        Wet,
        Dry,
        Cold,
        Hot,
        Raise,
        Lower,
        Inspect,
    };

    public Size GetBoardSize () {
        if (board == null) {
            return new Size (0, 0);
        } else {
            int width = board.GetLength(0);
            int height = board.GetLength(1);
            return new Size (width, height);
        }
    }

    public void UpdateInput (Action action, int brushMagnitude) {
        actionInfoText.text = action.ToString () + " | " + brushMagnitude;
    }

    public void CursorDisabled () {
        cursorPosition = null;
        if (display != null) {
            display.HideCursor ();
        }
    }

    public void CursorEnabled (int row, int col) {
        cursorPosition = new Position (row, col);
        if (display != null) {
            display.SetCursorPosition (board, row, col);
        }
    }

    public void CursorUpdate (int row, int col) {
        cursorPosition = new Position (row, col);
        if (display != null) {
            display.SetCursorPosition (board, row, col);
        }
    }

    public void ApplyActionAtCursor (Action action, int brushMagnitude) {
        if (board == null || cursorPosition == null) {
            return;
        }

        BoardNode node = board[cursorPosition.row, cursorPosition.col];
        if (action == Action.Wet) {
            node.moisture = Mathf.Clamp (node.moisture + brushMagnitude, 0, 100);
        } else if (action == Action.Dry) {
            node.moisture = Mathf.Clamp (node.moisture - brushMagnitude, 0, 100);
        } else if (action == Action.Raise) {
            node.altitude = Mathf.Clamp (node.altitude + brushMagnitude, 0, 100);
        } else if (action == Action.Lower) {
            node.altitude = Mathf.Clamp (node.altitude - brushMagnitude, 0, 100);
        } else if (action == Action.Hot) {
            node.temperature = Mathf.Clamp (node.temperature + brushMagnitude, 0, 100);
        } else if (action == Action.Cold) {
            node.temperature = Mathf.Clamp (node.temperature - brushMagnitude, 0, 100);
        } else if (action == Action.Inspect) {
            Biome biome = BiomeGenerator.GetBiome (node);
            SetInspectText (node, biome, cursorPosition.row, cursorPosition.col);
        }

        board[cursorPosition.row, cursorPosition.col] = node;

        if (display != null) {
            display.DrawBoard(board);
        }
    }
}
