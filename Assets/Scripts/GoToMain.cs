using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToMain : MonoBehaviour
{
  public LevelChanger levelChanger;

  public void GoToMenu()
  {
    levelChanger.FadeToLevel(0);
  }
}
