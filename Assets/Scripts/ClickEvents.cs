using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickEvents : MonoBehaviour
{
  private AudioSource buttonClickSFX;

  void Start()
  {
    buttonClickSFX = GetComponent<AudioSource>();
  }

  public void StartGame()
  {
    buttonClickSFX.Play();
    GameManager.Instance.StartGame();
  }

  public void GoToCharactersPage()
  {
    buttonClickSFX.Play();
    GameManager.Instance.GoToCharactersPage();
  }
}
