using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiversManager : MonoBehaviour
{
  public Grid grid;
  public GameObject logAndLilyGroup;
  public GameObject logPrefab;
  public GameObject lilyPrefab;
  public RowManager rowManager;
  public LayerMask logsMask;
  public LayerMask liliesMask;

  public int logGridWidth;
  public float riverMaxSpeed;
  public float riverMinSpeed;
  public float objectHeight;
  public float logPossibility;
  public float lilyPossibility;
  public float logRiverProbability;


  private float logWidth;

  private Dictionary<int, List<GameObject>> lilyRows = new Dictionary<int, List<GameObject>>();
  private Dictionary<int, List<GameObject>> logRows = new Dictionary<int, List<GameObject>>();
  private Dictionary<int, float> logDeltas = new Dictionary<int, float>();
  private Dictionary<int, bool[]> liliesData = new Dictionary<int, bool[]>();
  private Dictionary<int, int> riverDirections = new Dictionary<int, int>();
  private Dictionary<int, float> riverVelocities = new Dictionary<int, float>();

  public void SetupRiverAt(int x)
  {
    int direction = GetRandomDirection();
    if (rowManager.IsLogRiverAt(x - 1)) direction = -1 * riverDirections[x - 1];

    float globalX = grid.GetGlobalCoordFromGridCoord(x);

    if (IsLogRiverType())
    {
      List<GameObject> logs = new List<GameObject>();

      for (int i = 0; i < grid.size; i++)
      {
        float logZ = grid.GetGlobalCoordFromGridCoord(direction * (-(grid.size + 1) / 2 + i));
        if (ShouldPlaceLog())
        {
          GameObject newLog = Instantiate(
            logPrefab,
            new Vector3(globalX, objectHeight, logZ),
            Quaternion.identity,
            logAndLilyGroup.transform
          );

          logs.Add(newLog);

          i += logGridWidth + 1;
        }
      }
      logRows.Add(x, logs);
      logDeltas.Add(x, 0);
    }
    else
    {
      bool[] liliesDatum = new bool[grid.size];
      List<GameObject> lilies = new List<GameObject>();
      for (int i = 0; i < grid.size; i++) liliesDatum[i] = false;

      if (rowManager.IsPlainRowAt(x - 1))
      {
        int[] data = rowManager.GetPlainRowDataAt(x - 1);

        int count = 0;
        for (int i = 0; i < grid.size; i++)
          if (data[i] == Configs.WALKABLE_STATE)
          {
            liliesDatum[i] = true;
            count++;
            if (count == 2)
              break;
          }
      }
      else if (rowManager.IsLilyRiverAt(x - 1))
      {
        bool[] lilyDatum = GetLiliesDatumAt(x - 1);

        int count = 0;
        for (int i = grid.size - 1; i >= 0; i--)
          if (lilyDatum[i])
          {
            liliesDatum[i] = true;
            count++;
            if (count == 2)
              break;
          }
      }

      for (int i = 0; i < grid.size; i++)
      {
        float lilyZ = grid.GetGlobalCoordFromGridCoord(direction * (-(grid.size + 1) / 2 + i));
        if (ShouldPlaceLily() || liliesDatum[i])
        {
          GameObject newLily = Instantiate(
            lilyPrefab,
            new Vector3(globalX, objectHeight, lilyZ),
            Quaternion.identity,
            logAndLilyGroup.transform
          );

          liliesDatum[i] = true;
          lilies.Add(newLily);
        }
      }
      lilyRows.Add(x, lilies);
      liliesData.Add(x, liliesDatum);
    }

    riverVelocities.Add(x, riverMinSpeed + Random.Range(0.0f, 1.0f) * (riverMaxSpeed - riverMinSpeed));
    riverDirections.Add(x, direction);
  }

  public void CleanUpAt(int x, int deleteBehind)
  {
    int index = x - deleteBehind;
    if (lilyRows.ContainsKey(index))
    {
      List<GameObject> lilyRow = lilyRows[index];
      lilyRows.Remove(index);

      foreach (GameObject lily in lilyRow)
      {
        Destroy(lily);
      }

      riverDirections.Remove(index);
      riverVelocities.Remove(index);
    }
    else if (logRows.ContainsKey(index))
    {
      List<GameObject> logRow = logRows[index];
      logRows.Remove(index);

      foreach (GameObject log in logRow)
      {
        Destroy(log);
      }

      riverDirections.Remove(index);
      riverVelocities.Remove(index);
    }
  }

  public GameObject GetLogAt(Vector3 position)
  {
    Collider[] logs = Physics.OverlapSphere(position, grid.dimension / 4, logsMask);
    if (logs.Length == 0) return null;
    return logs[0].gameObject;
  }

  public GameObject GetLilyAt(Vector3 position)
  {
    Collider[] lillies = Physics.OverlapSphere(position, grid.dimension / 2, liliesMask);
    if (lillies.Length == 0) return null;
    return lillies[0].gameObject;
  }

  public bool HasLogRiverAt(int x)
  {
    return logRows.ContainsKey(x);
  }

  public bool HasLilyRiverAt(int x)
  {
    return lilyRows.ContainsKey(x);
  }

  public float GetLogDeltaAt(int x)
  {
    return logDeltas[x];
  }

  public bool[] GetLiliesDatumAt(int x)
  {
    return liliesData[x];
  }

  private int GetRandomDirection()
  {
    return Random.Range(0.0f, 1.0f) > 0.5f ? 1 : -1;
  }

  private bool ShouldPlaceLog()
  {
    return Random.Range(0.0f, 1.0f) < logPossibility;
  }

  private bool ShouldPlaceLily()
  {
    return Random.Range(0.0f, 1.0f) < lilyPossibility;
  }

  private bool IsLogRiverType()
  {
    return Random.Range(0.0f, 1.0f) < logRiverProbability;
  }

  void Start()
  {
    Vector3 logScale = logPrefab.transform.localScale;
    logScale.z = grid.dimension * (logGridWidth + 1);
    logPrefab.transform.localScale = logScale;

    logWidth = logPrefab.GetComponent<BoxCollider>().bounds.size.z;
  }

  void FixedUpdate()
  {
    foreach (KeyValuePair<int, List<GameObject>> logRow in logRows)
    {
      int direction = riverDirections[logRow.Key];
      Vector3 velocity = new Vector3(0.0f, 0.0f, riverVelocities[logRow.Key]);

      foreach (GameObject log in logRow.Value)
      {
        log.transform.position += direction * velocity * Time.deltaTime;

        if (direction > 0 ?
          log.transform.position.z > grid.GetGlobalCoordFromGridCoord((grid.size + 1) / 2) :
          log.transform.position.z < grid.GetGlobalCoordFromGridCoord(-(grid.size + 1) / 2))
        {
          log.transform.position = new Vector3(
            log.transform.position.x,
            log.transform.position.y,
            grid.GetGlobalCoordFromGridCoord((direction > 0 ? -1 : 1) * (grid.size + 1) / 2)
          );
        }
      }
      logDeltas[logRow.Key] += direction * velocity.z * Time.deltaTime;
      if (logDeltas[logRow.Key] >= logWidth || logDeltas[logRow.Key] <= -logWidth)
        logDeltas[logRow.Key] = 0;
    }
  }
}
