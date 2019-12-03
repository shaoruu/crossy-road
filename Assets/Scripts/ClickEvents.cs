using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickEvents : MonoBehaviour
{
  public void StartGame()
  {
    GameManager.Instance.StartGame();
  }

  public void GoToCharactersPage()
  {
    GameManager.Instance.GoToCharactersPage();
  }
}
