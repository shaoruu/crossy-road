using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CharacterList : MonoBehaviour
{
  public float lerpFactor;
  public float rotateSpeed;
  public float characterGap;
  public float characterHeight;

  public GameObject mainCamera;
  public GameObject stageGroup;
  public GameObject characterGroup;
  public GameObject stagePrefab;
  public Image infoBackground;
  public TextMeshProUGUI infoText;
  // public List<GameObject> characterPrefabs;

  private int index;

  private Vector3 nextPosition;
  private GameManager gameManager;
  private List<GameObject> characterPrefabs;
  private List<GameObject> instantiatedPrefabs;
  private List<string> characterInfo;

  // Start is called before the first frame update
  void Start()
  {
    gameManager = GameManager.Instance;
    characterPrefabs = gameManager.characters;

    instantiatedPrefabs = new List<GameObject>();
    characterInfo = new List<string>(){
      "Bunny",
      "Chicky",
      "Dig Dug Man",
      "Ducky",
      "Duckman",
      "Frogger",
      "Luigi",
      "Mario",
      "Steve"
    };

    nextPosition = mainCamera.transform.position;
    InitializeCharacters();
  }

  // Update is called once per frame
  void Update()
  {
    HandleCameraMovements();
  }

  void FixedUpdate()
  {
    foreach (GameObject character in instantiatedPrefabs)
    {
      character.transform.eulerAngles = new Vector3(
         character.transform.eulerAngles.x,
         character.transform.eulerAngles.y - Time.deltaTime * rotateSpeed,
         character.transform.eulerAngles.z
       );
    }

    mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, nextPosition, lerpFactor);
    if (Mathf.RoundToInt(mainCamera.transform.position.z) == Mathf.RoundToInt(GetNearestZ(mainCamera.transform.position.z)))
    {
      index = Mathf.RoundToInt(mainCamera.transform.position.z / characterGap);
      infoText.SetText(characterInfo[index]);
    }
  }

  private void InitializeCharacters()
  {
    for (int i = 0; i < characterPrefabs.Count; i++)
    {
      GameObject prefab = characterPrefabs[i];
      GameObject stage = Instantiate(stagePrefab, new Vector3(0, 0, i * characterGap), Quaternion.identity, transform);
      GameObject instantiated = Instantiate(prefab, new Vector3(0, characterHeight, i * characterGap), prefab.transform.rotation, transform);
      instantiated.transform.eulerAngles = new Vector3(
        instantiated.transform.eulerAngles.x,
        instantiated.transform.eulerAngles.y - 90,
        instantiated.transform.eulerAngles.z
      );
      instantiatedPrefabs.Add(instantiated);
    }
  }

  private void HandleCameraMovements()
  {
    bool isLeftDown = Input.GetKeyDown(KeyCode.LeftArrow);
    bool isRightDown = Input.GetKeyDown(KeyCode.RightArrow);

    bool isEnterDown = Input.GetKeyDown(KeyCode.Return);

    if (isEnterDown)
    {
      GameManager.Instance.SetCharacterIndex(Mathf.RoundToInt(GetNearestZ(mainCamera.transform.position.z) / characterGap));
      SceneManager.LoadScene(0);
      return;
    }

    if (isLeftDown)
    {
      nextPosition = GetNextPosition(mainCamera.transform.position, -1);
    }

    if (isRightDown)
    {
      nextPosition = GetNextPosition(mainCamera.transform.position, +1);
    }
  }

  private Vector3 GetNextPosition(Vector3 position, int direction)
  {
    float nextZ = GetNearestZ(position.z + direction * characterGap);
    if (nextZ < 0) nextZ = 0f;
    if (nextZ > (characterPrefabs.Count - 1) * characterGap) nextZ = (characterPrefabs.Count - 1) * characterGap;
    return new Vector3(position.x, position.y, nextZ);
  }

  private float GetNearestZ(float z)
  {
    return Mathf.RoundToInt(z / characterGap) * characterGap;
  }
}
