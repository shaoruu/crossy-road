using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
  public float drownY;
  public float lerpFactor;
  public float jumpHeight;
  public float jumpFactor;
  public float drownFactor;
  public float deathLerpFactor;
  public float deathDelay;

  public Grid grid;
  public RowManager rowManager;
  public RiversManager riversManager;
  public AudioSource playerJumpSFX;
  public GameManager gameManager;

  private Vector3 nextPosition;
  private Vector3 jumpVector;
  private GameObject latchTarget;
  private float originalY;
  private float latchOffset;
  private float totalDistance = 1f;
  private bool isLeftDown = false;
  private bool isRightDown = false;
  private bool isFrontDown = false;
  private bool isBackDown = false;
  private bool isDead = false;
  private bool isDrown = false;

  IEnumerator Death()
  {
    yield return new WaitForSeconds(deathDelay);
    gameManager.Restart();
  }

  // Start is called before the first frame update
  void Start()
  {
    latchTarget = null;
    nextPosition = grid.GetNearestGridPoint(transform.position);
    originalY = transform.position.y;
  }

  void FixedUpdate()
  {
    if (isDead)
    {
      transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(transform.localScale.x, 0.1f, transform.localScale.z), deathLerpFactor);
      transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, 0.1f, transform.position.z), deathLerpFactor);
      StartCoroutine("Death");
      return;
    }
    if (latchTarget != null)
    {
      Vector3 newPos = latchTarget.transform.position;
      newPos.z += latchOffset;
      newPos.y = originalY;

      transform.position = Vector3.Lerp(transform.position, newPos, lerpFactor);
    }
    else
    {
      float deltaX = transform.position.x - nextPosition.x;
      float deltaZ = transform.position.z - nextPosition.z;

      Vector3 newPos = Vector3.Lerp(transform.position, nextPosition, lerpFactor);
      float distToNextPosition = Mathf.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
      float potentialY = GetJumpY(distToNextPosition);
      newPos.y = (potentialY > originalY || isDrown) ? potentialY : originalY;
      // if (isDrown) transform.position = Vector3.Lerp(transform.position, newPos, lerpFactor);
      transform.position = newPos;
    }

    if (isDrown) StartCoroutine("Death");
  }

  // Update is called once per frame
  void Update()
  {
    if (isDead || isDrown) return;

    HandleHorizontalMovements();
    HandleVerticalMovements();
  }

  public void SetDead()
  {
    isDead = true;
  }

  public void SetDrown()
  {
    isDrown = true;
  }

  public void LatchOn(GameObject target, float delta)
  {
    latchTarget = target;
    latchOffset = delta;
  }

  public void ClearLatch()
  {
    latchTarget = null;
    latchOffset = 0;
  }

  private void HandleHorizontalMovements()
  {
    bool isLeftBDown = Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
    bool isRightBDown = Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
    bool isLeftBUp = Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A);
    bool isRightBUp = Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D);

    if (isLeftBDown && !isLeftDown)
    {
      isLeftDown = true;
      nextPosition = grid.GetLeftPosition(transform.position);
      transform.rotation = Quaternion.Euler(0, -180, 0);
      UpdateTotalDistance();
      playerJumpSFX.Play();
    }

    if (isRightBDown && !isRightDown)
    {
      isRightDown = true;
      nextPosition = grid.GetRightPosition(transform.position);
      transform.rotation = Quaternion.Euler(0, 0, 0);
      UpdateTotalDistance();
      playerJumpSFX.Play();
    }

    if (isLeftBUp) isLeftDown = false;
    if (isRightBUp) isRightDown = false;
  }

  private void HandleVerticalMovements()
  {
    bool isFrontBDown = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
    bool isBackBDown = Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
    bool isFrontBUp = Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W);
    bool isBackBUp = Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S);

    if (isFrontBDown && !isFrontDown)
    {
      isFrontDown = true;
      nextPosition = grid.GetForwardPosition(transform.position);
      transform.rotation = Quaternion.Euler(0, -90, 0);
      UpdateTotalDistance();
      playerJumpSFX.Play();
    }

    if (isBackBDown && !isBackDown)
    {
      isBackDown = true;
      nextPosition = grid.GetBackwardPosition(transform.position);
      transform.rotation = Quaternion.Euler(0, 90, 0);
      UpdateTotalDistance();
      playerJumpSFX.Play();
    }

    if (isFrontBUp) isFrontDown = false;
    if (isBackBUp) isBackDown = false;
  }

  private void UpdateTotalDistance()
  {
    float deltaZ = transform.position.z - nextPosition.z;
    float deltaX = transform.position.x - nextPosition.x;

    totalDistance = Mathf.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
  }

  private float GetJumpY(float distance)
  {
    if (isDrown) return -drownFactor * (distance - totalDistance) * (distance - totalDistance) + jumpHeight + drownY;
    if (totalDistance == 0) return originalY;
    return -jumpFactor * (distance - totalDistance) * (distance - totalDistance) + jumpHeight + originalY;
  }
}
