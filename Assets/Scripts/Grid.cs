using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
  public GameObject player;
  public RowManager rowManager;
  public RiversManager riversManager;

  // horizontal size
  public int size;
  // dimension of grid cells
  public float dimension;

  private PlayerController playerController;
  private PlayerState playerState;

  public Vector3 GetForwardPosition(Vector3 position)
  {
    // make sure it's snapped to grid
    Vector3 newPos = GetNearestGridPoint(position);
    newPos.x += dimension;
    int gridX = GetGridCoordFromGlobalCoord(newPos.x);

    if (rowManager.HasObstacleAt(newPos)) return position;

    rowManager.CheckMaxLevel(gridX);
    if (rowManager.LoadMoreRows(gridX))
    {
      rowManager.CleanUpAt(gridX);
    }

    // CHECK IF IT'S A RIVER
    if (rowManager.IsLogRiverAt(gridX))
    {
      position.x += dimension;
      GameObject landingLog = riversManager.GetLogAt(position);
      if (landingLog == null)
      {
        Debug.Log("SHIT NO LOG");
        playerState.Drown();
      }
      else
      {
        Debug.Log(position.z);
        int logGridZ = GetGridCoordFromGlobalCoord(landingLog.transform.position.z);
        Debug.Log(logGridZ);
        int newPosGridZ = GetGridCoordFromGlobalCoord(position.z);
        float delta = (newPosGridZ - logGridZ) * dimension;
        playerController.LatchOn(landingLog, delta);
        return position;
      }
    }
    else if (rowManager.IsLilyRiverAt(gridX))
    {
      GameObject landingLily = riversManager.GetLilyAt(newPos);
      if (landingLily == null)
      {
        Debug.Log("SHIT NO LILY");
        playerState.Drown();
      }
    }

    playerController.ClearLatch();

    return newPos;
  }

  public Vector3 GetBackwardPosition(Vector3 position)
  {
    // make sure it's snapped to grid
    Vector3 newPos = GetNearestGridPoint(position);
    newPos.x -= dimension;

    if (rowManager.HasObstacleAt(newPos)) return position;
    int gridX = GetGridCoordFromGlobalCoord(newPos.x);
    if (!rowManager.CanGoBack(gridX)) return position;
    rowManager.LoadMoreRows(gridX);

    // CHECK IF IT'S A RIVER
    if (rowManager.IsLogRiverAt(gridX))
    {
      position.x -= dimension;
      GameObject landingLog = riversManager.GetLogAt(position);
      if (landingLog == null)
      {
        Debug.Log("SHIT NO LOG");
        playerState.Drown();
      }
      else
      {
        int logGridZ = GetGridCoordFromGlobalCoord(landingLog.transform.position.z);
        int newPosGridZ = GetGridCoordFromGlobalCoord(position.z);
        float delta = (newPosGridZ - logGridZ) * dimension;
        playerController.LatchOn(landingLog, delta);
        return position;
      }
    }
    else if (rowManager.IsLilyRiverAt(gridX))
    {
      GameObject landingLily = riversManager.GetLilyAt(newPos);
      if (landingLily == null)
      {
        Debug.Log("SHIT NO LILY");
        playerState.Drown();
      }
    }

    playerController.ClearLatch();

    return newPos;
  }

  public Vector3 GetLeftPosition(Vector3 position)
  {
    int gridX = GetGridCoordFromGlobalCoord(GetNearestGridPoint(position).x);
    if (!rowManager.IsLogRiverAt(gridX))
    {
      // make sure it's snapped to grid
      Vector3 newPos = GetNearestGridPoint(position);
      newPos.z += dimension;

      if (rowManager.IsLilyRiverAt(gridX))
      {
        GameObject landingLily = riversManager.GetLilyAt(newPos);
        if (landingLily == null)
        {
          Debug.Log("SHIT NO LILY");
          playerState.Drown();
        }
      }

      if (rowManager.HasObstacleAt(newPos)) return position;
      return newPos;
    }

    // RIVER JUMPING
    position.z += dimension;
    GameObject landingLog = riversManager.GetLogAt(position);
    if (landingLog == null)
    {
      Debug.Log("SHIT NO LOG");
      playerState.Drown();
    }
    else
    {
      int logGridZ = GetGridCoordFromGlobalCoord(landingLog.transform.position.z);
      int newPosGridZ = GetGridCoordFromGlobalCoord(position.z);
      float delta = (newPosGridZ - logGridZ) * dimension;
      playerController.LatchOn(landingLog, delta);
      return position;
    }

    playerController.ClearLatch();
    return GetNearestGridPoint(position);
  }

  public Vector3 GetRightPosition(Vector3 position)
  {
    int gridX = GetGridCoordFromGlobalCoord(GetNearestGridPoint(position).x);
    if (!rowManager.IsLogRiverAt(gridX))
    {
      // make sure it's snapped to grid
      Vector3 newPos = GetNearestGridPoint(position);
      newPos.z -= dimension;

      if (rowManager.IsLilyRiverAt(gridX))
      {
        GameObject landingLily = riversManager.GetLilyAt(newPos);
        if (landingLily == null)
        {
          Debug.Log("SHIT NO LILY");
          playerState.Drown();
        }
      }

      if (rowManager.HasObstacleAt(newPos)) return position;
      return newPos;
    }

    // RIVER JUMPING
    position.z -= dimension;
    GameObject landingLog = riversManager.GetLogAt(position);
    if (landingLog == null)
    {
      Debug.Log("SHIT NO LOG");
      playerState.Drown();
    }
    else
    {
      int logGridZ = GetGridCoordFromGlobalCoord(landingLog.transform.position.z);
      int newPosGridZ = GetGridCoordFromGlobalCoord(position.z);
      float delta = (newPosGridZ - logGridZ) * dimension;
      playerController.LatchOn(landingLog, delta);
      return position;
    }

    playerController.ClearLatch();
    return GetNearestGridPoint(position);
  }

  public Vector3 GetNearestGridPoint(Vector3 position)
  {
    position.x = RoundToGrid(position.x);
    // position.y = RoundToGrid(position.y);
    position.z = RoundToGrid(position.z);

    return position;
  }

  public float RoundToGrid(float number)
  {
    return Mathf.RoundToInt(number / dimension) * dimension;
  }

  public float GetGlobalCoordFromGridCoord(int coord)
  {
    return coord * dimension;
  }

  public int GetGridCoordFromGlobalCoord(float coord)
  {
    return (int)(coord / dimension);
  }

  // Start is called before the first frame update
  void Start()
  {
    playerController = player.GetComponent<PlayerController>();
    playerState = player.GetComponent<PlayerState>();
  }

  // Update is called once per frame
  void Update()
  {

  }
}
