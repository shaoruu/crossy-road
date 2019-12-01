using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarsManager : MonoBehaviour
{
  public Grid grid;
  public GameObject carGroup;
  public List<GameObject> carPrefabs;

  public int maxBusCount;
  public float carMaxSpeed;
  public float carMinSpeed;
  public float carHeight;
  public float carPossibility;

  private Dictionary<int, List<GameObject>> carRows = new Dictionary<int, List<GameObject>>();
  private Dictionary<int, int> carDirections = new Dictionary<int, int>();
  private Dictionary<int, float> carVelocities = new Dictionary<int, float>();

  public void SetupCarsAt(int x)
  {
    List<GameObject> cars = new List<GameObject>();
    int direction = GetRandomDirection();

    int busCount = 0;
    float globalX = grid.GetGlobalCoordFromGridCoord(x);

    for (int i = 0; i < grid.size; i += 4)
    {
      if (ShouldPlaceCar())
      {
        float carZ = grid.GetGlobalCoordFromGridCoord(direction * (-(grid.size + 1) / 2 + i));

        GameObject newCar = Instantiate(
          GetRandomCarPrefab(),
          new Vector3(globalX, carHeight, carZ),
          Quaternion.identity,
          carGroup.transform
        );

        if (newCar.CompareTag("Bus"))
        {
          busCount++;
          if (busCount > maxBusCount)
          {
            Destroy(newCar);
            continue;
          }
        }

        if (direction < 0)
          newCar.transform.rotation = Quaternion.Euler(0, 180, 0);

        cars.Add(newCar);
      }
    }

    carVelocities.Add(x, carMinSpeed + Random.Range(0.0f, 1.0f) * (carMaxSpeed - carMinSpeed));
    carDirections.Add(x, direction);
    carRows.Add(x, cars);
  }

  public void CleanUpAt(int x, int deleteBehind)
  {
    int index = x - deleteBehind;
    if (carRows.ContainsKey(index))
    {
      List<GameObject> row = carRows[index];
      carRows.Remove(index);

      foreach (GameObject car in row)
      {
        Destroy(car);
      }
      carDirections.Remove(index);
      carVelocities.Remove(index);
    }

  }

  private bool ShouldPlaceCar()
  {
    return Random.Range(0.0f, 1.0f) < carPossibility;
  }

  private int GetRandomDirection()
  {
    return Random.Range(0.0f, 1.0f) >= 0.5f ? 1 : -1;
  }

  private GameObject GetRandomCarPrefab()
  {
    return carPrefabs[Mathf.FloorToInt(Random.Range(0.0f, 1.0f) * carPrefabs.Count)];
  }

  void Start()
  {
  }

  void FixedUpdate()
  {
    foreach (KeyValuePair<int, List<GameObject>> row in carRows)
    {
      int direction = carDirections[row.Key];
      Vector3 carVelocity = new Vector3(0.0f, 0.0f, carVelocities[row.Key]);

      foreach (GameObject car in row.Value)
      {
        car.transform.position += direction * carVelocity * Time.deltaTime;

        if (direction > 0 ?
          car.transform.position.z > grid.GetGlobalCoordFromGridCoord((grid.size + 1) / 2) :
          car.transform.position.z < grid.GetGlobalCoordFromGridCoord(-(grid.size + 1) / 2))
        {
          car.transform.position = new Vector3(
            car.transform.position.x,
            car.transform.position.y,
            grid.GetGlobalCoordFromGridCoord((direction > 0 ? -1 : 1) * (grid.size + 1) / 2)
          );
        }
      }
    }
  }
}
