using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
  public static GameManager Instance;

  public List<GameObject> characters;

  public CameraController cameraController;
  public PlayerController playerController;
  public LevelChanger levelChanger;

  public RectTransform scorePanel;
  public RectTransform menuPanel;
  public TextMeshProUGUI scoreText;
  public TextMeshProUGUI maxScoreText;
  public TextMeshProUGUI startButton;

  public RowManager rowManager;

  public float uiLerpFactor;
  public float gameStartDelay;

  private int maxScore;
  private int topScore;
  private int characterIndex;
  private bool started;
  private bool isPrepared;
  private bool hasRestarted;
  private float menuToTheRight;

  private Vector3 menuPanelInitPos;
  private Vector3 menuPanelFinalPos;

  IEnumerator GameStarter()
  {
    yield return new WaitForSeconds(gameStartDelay);
    isPrepared = true;
  }

  void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    }
    else if (Instance != this)
    {
      Instance.cameraController = this.cameraController;
      Instance.playerController = this.playerController;
      Instance.scorePanel = this.scorePanel;
      Instance.menuPanel = this.menuPanel;
      Instance.scoreText = this.scoreText;
      Instance.maxScoreText = this.maxScoreText;

      Instance.rowManager = this.rowManager;
      Instance.levelChanger = this.levelChanger;
      Instance.startButton = this.startButton;

      Instance.isPrepared = false;
      Instance.started = false;

      if (Instance.hasRestarted)
        Instance.startButton.SetText("Restart");

      Instance.UpdateMaxScore();

      Destroy(gameObject);
    }

    StartCoroutine("GameStarter");
  }

  // Start is called before the first frame update
  void Start()
  {
    started = false;
    isPrepared = false;
    maxScore = 0;
    characterIndex = 0;

    menuToTheRight = Screen.width * 2;

    menuPanelInitPos = menuPanel.offsetMax;
    menuPanelFinalPos = new Vector2(-menuToTheRight, menuPanel.offsetMax.y);

    menuPanel.offsetMax = menuPanelFinalPos;

    GetComponent<AudioSource>().Play();
  }

  void FixedUpdate()
  {
    if (!started)
    {
      if (isPrepared)
        ShowMenu();

      return;
    }
    else
    {
      HideMenu();
      ShowScore();

      UpdateScore();
    }
  }

  public void Restart()
  {
    SetHasRestarted();
    levelChanger.FadeToLevel(SceneManager.GetActiveScene().buildIndex);
  }

  public GameObject GetPlayerPrefab()
  {
    // return characters[8];
    // if (characterIndex == -1)
    //   return characters[Mathf.FloorToInt(Random.Range(0.0f, 1.0f) * characters.Count)];
    return characters[characterIndex];
  }

  public void StartGame()
  {
    started = true;

    playerController.StartGame();
    cameraController.StartGame();
    // playerController
  }

  public void GoToCharactersPage()
  {
    isPrepared = false;
    levelChanger.FadeToLevel(1);
  }

  public void SetCharacterIndex(int index)
  {
    characterIndex = index;
  }

  public int GetCharacterIndex()
  {
    return characterIndex;
  }

  private void ShowMenu()
  {
    menuPanel.offsetMax = Vector2.Lerp(menuPanel.offsetMax, menuPanelInitPos, uiLerpFactor);
  }

  private void HideMenu()
  {
    menuPanel.offsetMax = Vector2.Lerp(menuPanel.offsetMax, menuPanelFinalPos, uiLerpFactor);
  }

  private void ShowScore()
  {
    scorePanel.offsetMax = Vector2.Lerp(scorePanel.offsetMax, new Vector2(scorePanel.offsetMax.x, 0), uiLerpFactor);
  }

  private void SetHasRestarted()
  {
    hasRestarted = true;
  }

  private void UpdateScore()
  {
    int score = rowManager.GetMaxLevel();
    scoreText.SetText(score.ToString());

    if (score > maxScore)
    {
      maxScore = score;
      maxScoreText.SetText("top: " + maxScore.ToString());
    }
  }

  private void UpdateMaxScore()
  {
    Debug.Log(maxScore);
    maxScoreText.SetText("top: " + maxScore.ToString());
  }
}
