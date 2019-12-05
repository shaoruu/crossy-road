using System.Collections.Generic;
using UnityEngine;

public class RowManager : MonoBehaviour
{
  public Grid grid;
  public CarsManager carsManager;
  public RiversManager riversManager;
  public GameObject obstacleGroup;
  public GameObject rowPrefab;
  public GameObject rowGroup;
  public GameObject roadPrefab;
  public Material plainRowMaterial;
  public Material roadRowMaterial;
  public Material riverRowMaterial;
  public AudioSource advanceLvlSFX;
  public List<GameObject> treePrefabs;
  public List<GameObject> obstaclePrefabs;

  public int rowsAhead;
  public int rowsBehind;
  public int maxBehind;
  public int deleteBehind;
  public int maxTreeInner;
  public int starterRows;
  public int minimumPathway;
  public float riverLevel;
  public float roadLevel;
  public float plainLevel;
  public float scatteredTreePossibility;

  private int maxLevel;
  private int currentLevel;
  private bool isFirstTime;

  private Dictionary<int, GameObject> rowObjects = new Dictionary<int, GameObject>();
  private Dictionary<string, GameObject> treeObjects = new Dictionary<string, GameObject>();
  private Dictionary<int, int[]> rowsData = new Dictionary<int, int[]>();
  private Dictionary<int, int> rowsTypes = new Dictionary<int, int>();

  void Start()
  {
    maxLevel = 0;
    currentLevel = 0;

    InitializeRows();
  }

  public bool LoadMoreRows(int x)
  {
    return TrySetupRowAt(x + rowsAhead);
  }

  public bool HasObstacleAt(Vector3 position)
  {
    int x = grid.GetGridCoordFromGlobalCoord(position.x);
    int z = grid.GetGridCoordFromGlobalCoord(position.z);
    return treeObjects.ContainsKey(GetTreeRep(x, z));
  }

  public void CheckMaxLevel(int x)
  {
    currentLevel = x;
    if (x > maxLevel)
    {
      advanceLvlSFX.Play();
      maxLevel = x;
    }
    // SET TEXT HERE
  }

  public bool CanGoBack(int x)
  {
    return !(maxLevel - x > maxBehind);
  }

  public void CleanUpAt(int x)
  {
    int index = x - deleteBehind;
    if (rowObjects.ContainsKey(index))
    {
      GameObject row = rowObjects[index];
      rowObjects.Remove(index);

      Destroy(row);
    }

    for (int i = -(grid.size + 1) / 2; i <= (grid.size + 1) / 2; i++)
    {
      string rep = GetTreeRep(index, i);

      if (treeObjects.ContainsKey(rep))
      {
        GameObject tree = treeObjects[rep];
        treeObjects.Remove(rep);

        Destroy(tree);
      }
    }

    carsManager.CleanUpAt(x, deleteBehind);
    riversManager.CleanUpAt(x, deleteBehind);
  }

  public bool IsLogRiverAt(int x)
  {
    return riversManager.HasLogRiverAt(x);
  }

  public bool IsLilyRiverAt(int x)
  {
    return riversManager.HasLilyRiverAt(x);
  }

  public bool IsPlainRowAt(int x)
  {
    return rowsData.ContainsKey(x);
  }

  public int[] GetPlainRowDataAt(int x)
  {
    return rowsData[x];
  }

  public int GetMaxLevel()
  {
    return maxLevel;
  }

  private void InitializeRows()
  {
    for (int i = -rowsBehind; i <= rowsAhead; i++)
      TrySetupRowAt(i, !(i <= starterRows));
  }

  private bool TrySetupRowAt(int x, bool shouldScatter = true)
  {
    bool success = TryPlacingRowAt(x);
    InitializeRowAt(x, shouldScatter);

    return success;
  }

  private bool TryPlacingRowAt(int x)
  {
    if (rowObjects.ContainsKey(x)) return false;
    int rowType = DecideRowType();
    if (x <= starterRows) rowType = Configs.ROW_PLAIN_TYPE;

    float height = 0f;
    switch (rowType)
    {
      case Configs.ROW_PLAIN_TYPE:
        height = plainLevel;
        break;
      case Configs.ROW_RIVER_TYPE:
        height = riverLevel;
        break;
      case Configs.ROW_ROAD_TYPE:
        height = roadLevel;
        break;
    }

    GameObject newRow = Instantiate(
      rowType == Configs.ROW_ROAD_TYPE ? roadPrefab : rowPrefab,
      new Vector3(grid.GetGlobalCoordFromGridCoord(x), height, 0),
      Quaternion.identity,
      rowGroup.transform
    );
    rowObjects.Add(x, newRow);


    rowsTypes.Add(x, rowType);
    switch (rowType)
    {
      case Configs.ROW_PLAIN_TYPE:
        newRow.GetComponent<MeshRenderer>().material = plainRowMaterial;
        break;
      case Configs.ROW_RIVER_TYPE:
        newRow.GetComponent<MeshRenderer>().material = riverRowMaterial;
        break;
      case Configs.ROW_ROAD_TYPE:
        newRow.GetComponent<MeshRenderer>().material = roadRowMaterial;
        break;
    }

    return true;
  }

  private void InitializeRowAt(int x, bool shouldScatter)
  {
    if (rowsData.ContainsKey(x)) return;

    int[] rowData = new int[grid.size];

    switch (GetRowTypeAt(x))
    {
      case Configs.ROW_PLAIN_TYPE:
        SetupPlainRowData(rowData, x, shouldScatter);
        break;
      case Configs.ROW_ROAD_TYPE:
        carsManager.SetupCarsAt(x);
        break;
      case Configs.ROW_RIVER_TYPE:
        riversManager.SetupRiverAt(x);
        break;
    }

    // string rowDataStr = "";
    // foreach (int num in rowData) rowDataStr += num + " ";
    // Debug.Log(rowDataStr);

    rowsData.Add(x, rowData);
  }

  private void SetupPlainRowData(int[] rowData, int x, bool shouldScatter)
  {
    bool startedBlockingHead = false, startedBlockingEnd = false;
    for (int i = maxTreeInner - 1; i >= 0; i--)
    {
      if (i == 0)
      {
        startedBlockingHead = true;
        startedBlockingEnd = true;
      }

      if (startedBlockingHead)
        rowData[i] = Configs.OBSTACLE_STATE;
      else if (ShouldPlaceObstacle())
        startedBlockingHead = true;
      else
        rowData[i] = Configs.WALKABLE_STATE;

      if (startedBlockingEnd)
        rowData[grid.size - i - 1] = Configs.OBSTACLE_STATE;
      else if (ShouldPlaceObstacle())
        startedBlockingEnd = true;
      else
        rowData[i] = Configs.WALKABLE_STATE;
    }

    if (shouldScatter)
      for (int i = maxTreeInner; i < grid.size - maxTreeInner; i++)
      {
        if (ShouldPlaceScatteredTree())
          rowData[i] = Configs.OBSTACLE_STATE;
      }

    if (IsLilyRiverAt(x - 1))
    {
      int count = 0;
      bool[] liliesDatum = riversManager.GetLiliesDatumAt(x - 1);
      for (int i = 0; i < liliesDatum.Length; i++)
        if (liliesDatum[i])
        {
          rowData[i] = Configs.WALKABLE_STATE;
          count++;
          if (count == minimumPathway) break;
        }
    }

    for (int i = 0; i < rowData.Length; i++)
    {

      int state = rowData[i];
      switch (state)
      {
        case Configs.WALKABLE_STATE:
          break;
        case Configs.OBSTACLE_STATE:
          if (i >= maxTreeInner && i < grid.size - maxTreeInner)
            PlaceObstacleAt(x, i - (grid.size - 1) / 2);
          else
            PlaceTreeAt(x, i - (grid.size - 1) / 2);
          break;
      }
    }
  }

  private void PlaceTreeAt(int x, int z)
  {
    string rep = GetTreeRep(x, z);
    if (treeObjects.ContainsKey(rep)) return;

    GameObject newObstacle = Instantiate(
      GetRandomTreePrefab(),
      new Vector3(grid.GetGlobalCoordFromGridCoord(x), 0, grid.GetGlobalCoordFromGridCoord(z)),
      Quaternion.identity,
      obstacleGroup.transform
    );
    treeObjects.Add(rep, newObstacle);
  }

  private void PlaceObstacleAt(int x, int z)
  {
    string rep = GetTreeRep(x, z);
    if (treeObjects.ContainsKey(rep)) return;

    GameObject newObstacle = Instantiate(
       GetRandomeObstaclePrefab(),
       new Vector3(grid.GetGlobalCoordFromGridCoord(x), 0, grid.GetGlobalCoordFromGridCoord(z)),
       Quaternion.identity,
       obstacleGroup.transform
     );
    treeObjects.Add(rep, newObstacle);
  }

  private GameObject GetRandomTreePrefab()
  {
    return treePrefabs[Mathf.FloorToInt(Random.Range(0.0f, 1.0f) * treePrefabs.Count)];
  }

  private GameObject GetRandomeObstaclePrefab()
  {
    return obstaclePrefabs[Mathf.FloorToInt(Random.Range(0.0f, 1.0f) * obstaclePrefabs.Count)];

  }

  private bool ShouldPlaceObstacle()
  {
    return Random.Range(0.0f, 1.0f) > 0.5f;
  }

  private bool ShouldPlaceScatteredTree()
  {
    return Random.Range(0.0f, 1.0f) < scatteredTreePossibility;
  }

  private string GetTreeRep(int x, int z)
  {
    return "" + x + "::" + z;
  }

  private int DecideRowType()
  {
    float possibility = Random.Range(0.0f, 1.0f);
    if (possibility < 0.5f) return Configs.ROW_PLAIN_TYPE;
    if (possibility > 0.5f && possibility < 0.7f) return Configs.ROW_RIVER_TYPE;
    return Configs.ROW_ROAD_TYPE;
  }

  private int GetRowTypeAt(int x)
  {
    return rowsTypes[x];
  }
}
