using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
  public static GameManager Instance;

  public GameObject mainCamera;
  public GameObject player;
  public GameObject scoreObj;
  public GameObject menuObj;
  public GameObject scoreText;
  public GameObject maxScoreText;

  public RowManager rowManager;

  public float menuToTheRight;
  public float uiLerpFactor;

  private bool started;
  private int maxScore;
  private int topScore;

  private CameraController cameraController;
  private PlayerController playerController;
  private Vector3 menuObjFinalPos;
  private Vector3 scoreObjOgPos;
  private TextMeshProUGUI scoreTextGUI;
  private TextMeshProUGUI maxScoreTextGUI;

  void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    }
    else if (Instance != this)
      Destroy(gameObject);
  }

  // Start is called before the first frame update
  void Start()
  {
    started = false;
    maxScore = 0;

    cameraController = mainCamera.GetComponent<CameraController>();
    playerController = player.GetComponent<PlayerController>();

    scoreTextGUI = scoreText.GetComponent<TextMeshProUGUI>();
    maxScoreTextGUI = maxScoreText.GetComponent<TextMeshProUGUI>();

    scoreObjOgPos = scoreObj.transform.position;

    scoreObj.transform.position = new Vector3(
       scoreObj.transform.position.x,
       4500f,
       scoreObj.transform.position.z
     );

    menuObjFinalPos = new Vector3(
     menuToTheRight,
     menuObj.transform.position.y,
     menuObj.transform.position.z
   );
  }

  void FixedUpdate()
  {
    if (!started) return;
    else
    {
      HideMenu();
      ShowScore();

      UpdateScore();
    }
  }

  private void HideMenu()
  {
    menuObj.transform.position = Vector3.Lerp(menuObj.transform.position, menuObjFinalPos, uiLerpFactor);
  }

  private void ShowScore()
  {
    scoreObj.transform.position =
            Vector3.Lerp(scoreObj.transform.position, scoreObjOgPos, uiLerpFactor);
  }

  private void UpdateScore()
  {
    int score = rowManager.GetMaxLevel();
    scoreTextGUI.SetText(score.ToString());

    if (score > maxScore)
      maxScore = score;

    maxScoreTextGUI.SetText("top: " + maxScore.ToString());
  }

  public void StartGame()
  {
    started = true;

    cameraController.StartGame();
    // playerController
  }
}
