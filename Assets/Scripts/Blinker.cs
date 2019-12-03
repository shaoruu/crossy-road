using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Blinker : MonoBehaviour
{
  public float delay;

  public Color firstColor;
  public Color secondColor;

  private bool switcher;
  private TextMeshProUGUI text;

  // Start is called before the first frame update
  void Start()
  {
    switcher = false;
    text = GetComponent<TextMeshProUGUI>();

    StartBlinking();
  }

  IEnumerator Blink()
  {
    while (true)
    {
      if (switcher)
      {
        text.color = firstColor;
        switcher = false;
        yield return new WaitForSeconds(delay);
      }
      else
      {
        text.color = secondColor;
        switcher = true;
        yield return new WaitForSeconds(delay);
      }
    }
  }

  public void StartBlinking()
  {
    StopCoroutine("Blink");
    StartCoroutine("Blink");
  }

  public void StopBlinking()
  {
    StopCoroutine("Blink");
  }
}
