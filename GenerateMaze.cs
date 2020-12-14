using System.Collections.Generic;
using UnityEngine;

public class GenerateMaze : MonoBehaviour
{
    Cell[,] cells;

    public int rows = 5, columns = 5;
    public float scale = 1.0f;
    public GameObject floor, wall;

    int currentRow = 0, currentColumn = 0;
    float halfFloorHeight;

    enum Direction { North, East, South, West };
    int random;
    [Range(0.2f, 1f)]
    public float openPlanFactor = 1f;

    void Awake() {
        cells = new Cell[rows, columns];
        halfFloorHeight = scale / 20;
    }


    void Start() {
        GenerateMap();
    }


    public void GenerateMap() {
        GenerateCells();
        cells[currentRow, currentColumn].visited = true;
        CarveMaze();
        OpenPlanFactor();
    }


    void GenerateCells() {
        for (int row = 0; row < rows; row++) {
            for (int column = 0; column < columns; column++) {
                Cell cell = new Cell();

                cell.floor = Instantiate(floor, new Vector3(column * scale, -halfFloorHeight, row * scale), Quaternion.identity);
                cell.floor.name = $"{column},{row} Floor";
                cell.floor.transform.localScale *= scale;
                cell.floor.transform.parent = transform;

                cell.wallNorth = Instantiate(wall, new Vector3(column * scale, scale / 2, row * scale + scale / 2), Quaternion.identity);      // north wall
                cell.wallNorth.name = $"{column},{row} Wall - North";
                cell.wallNorth.transform.localScale *= scale;
                cell.wallNorth.transform.parent = transform;

                cell.wallEast = Instantiate(wall, new Vector3(column * scale + scale / 2, scale / 2, row * scale), Quaternion.Euler(0, 90, 0));      // east wall
                cell.wallEast.name = $"{column},{row} Wall - East";
                cell.wallEast.transform.localScale *= scale;
                cell.wallEast.transform.parent = transform;

                if (row == 0) {
                    cell.wallSouth = Instantiate(wall, new Vector3(column * scale, scale / 2, row * scale - scale / 2), Quaternion.identity);      // south wall
                    cell.wallSouth.name = $"{column},{row} Wall - South";
                    cell.wallSouth.transform.localScale *= scale;
                    cell.wallSouth.transform.parent = transform;
                }

                if (column == 0) {
                    cell.wallWest = Instantiate(wall, new Vector3(column * scale - scale / 2, scale / 2, row * scale), Quaternion.Euler(0, 90, 0));      // west wall
                    cell.wallWest.name = $"{column},{row} Wall - West";
                    cell.wallWest.transform.localScale *= scale;
                    cell.wallWest.transform.parent = transform;
                }

                cells[row, column] = cell;
            }
        }
    }


    void CarveMaze() {
        List<Direction> neighbouringUnvisitedCells = NeighbourCells(currentRow, currentColumn, false);

        if (neighbouringUnvisitedCells.Count > 0) {
            random = Random.Range(0, neighbouringUnvisitedCells.Count);

            if (neighbouringUnvisitedCells[random] == Direction.North) {
                DestroyImmediate(cells[currentRow, currentColumn].wallNorth);
                DestroyImmediate(cells[currentRow + 1, currentColumn].wallSouth);
                currentRow++;
            }
            else if (neighbouringUnvisitedCells[random] == Direction.East) {
                DestroyImmediate(cells[currentRow, currentColumn].wallEast);
                DestroyImmediate(cells[currentRow, currentColumn + 1].wallWest);
                currentColumn++;
            }
            else if (neighbouringUnvisitedCells[random] == Direction.South) {
                DestroyImmediate(cells[currentRow, currentColumn].wallSouth);
                DestroyImmediate(cells[currentRow - 1, currentColumn].wallNorth);
                currentRow--;
            }
            else if (neighbouringUnvisitedCells[random] == Direction.West) {
                DestroyImmediate(cells[currentRow, currentColumn].wallWest);
                DestroyImmediate(cells[currentRow, currentColumn - 1].wallEast);
                currentColumn--;
            }
            neighbouringUnvisitedCells.Clear();

            cells[currentRow, currentColumn].visited = true;

            CarveMaze();
        }
        else {
            if (!MazeCompleted()) {
                GoToNextUnvisited();
                CarveMaze();
            }
            else
                return;
        }
                
    }

    List<Direction> NeighbourCells(int row, int column, bool visited) {
        List<Direction> neighbourCells = new List<Direction>();

        if (row < rows - 1 && cells[row + 1, column].visited == visited)
            neighbourCells.Add(Direction.North);

        if (column < columns - 1 && cells[row, column + 1].visited == visited)
            neighbourCells.Add(Direction.East);

        if (row > 0 && cells[row - 1, column].visited == visited)
            neighbourCells.Add(Direction.South);

        if (column > 0 && cells[row, column - 1].visited == visited)
            neighbourCells.Add(Direction.West);

        return neighbourCells;
    }


    void GoToNextUnvisited() {
        for (int row = 0; row < rows; row++) {
            for (int column = 0; column < columns; column++) {
                if (!cells[row, column].visited) {
                    List<Direction> neighbouringVisitedCells = NeighbourCells(row, column, true);
                    int random = Random.Range(0, neighbouringVisitedCells.Count);

                    if (neighbouringVisitedCells[random] == Direction.North) {
                        DestroyImmediate(cells[row, column].wallNorth);
                        DestroyImmediate(cells[row + 1, column].wallSouth);
                    }

                    else if (neighbouringVisitedCells[random] == Direction.East) {
                        DestroyImmediate(cells[row, column].wallEast);
                        DestroyImmediate(cells[row, column + 1].wallWest);
                    }

                    else if (neighbouringVisitedCells[random] == Direction.South) {
                        DestroyImmediate(cells[row, column].wallSouth);
                        DestroyImmediate(cells[row - 1, column].wallNorth);
                    }

                    else if (neighbouringVisitedCells[random] == Direction.West) {
                        DestroyImmediate(cells[row, column].wallWest);
                        DestroyImmediate(cells[row, column - 1].wallEast);
                    }
                    neighbouringVisitedCells.Clear();

                    cells[row, column].visited = true;
                    currentRow = row;
                    currentColumn = column;
                    return;
                }
            }
        }
    }


    bool MazeCompleted() {
        for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
                if (!cells[row, column].visited)
                    return false;
        return true;
    }


    void OpenPlanFactor() {
        float initialInternalWalls = InternalWallCount();

        while (InternalWallCount() / initialInternalWalls > openPlanFactor) {
            int randomRow = Random.Range(0, rows - 1);
            int randomColumn = Random.Range(0, columns - 1);

            int randomWall = Random.Range(0, 2);
            if (randomWall == 0 && randomRow < rows && cells[randomRow, randomColumn].wallNorth != null)
                DestroyImmediate(cells[randomRow, randomColumn].wallNorth);
            else if (randomWall == 1 && randomRow < columns && cells[randomRow, randomColumn].wallEast != null)
                DestroyImmediate(cells[randomRow, randomColumn].wallEast);
        }
    }


    float InternalWallCount() {
        float totalInternalWalls = 0f;

        for (int column = 0; column < columns; column++) {
            for (int row = 0; row < rows; row++) {
                if (column != columns - 1 && cells[row, column].wallEast != null)
                    totalInternalWalls++;
                if (row != rows - 1 && cells[row, column].wallNorth != null)
                    totalInternalWalls++;
            }
        }
        return totalInternalWalls;
    }
}
