using UnityEngine;

public class CameraController : MonoBehaviour
{
  public GameObject player;

  public float cameraLerp;
  public float cameraVelocity;
  public float cameraTransitionBase;
  public float maxPlayerDistance;
  public float initialHeight;

  public Vector3 initialRotation;

  private bool started;
  private Vector3 offset;
  private Vector3 velocity;

  // Start is called before the first frame update
  void Start()
  {
    started = false;

    offset = transform.position - player.transform.position;
    velocity = new Vector3(cameraVelocity, 0, 0);
  }

  // Update is called once per frame
  void FixedUpdate()
  {
    if (!started) return;
    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(initialRotation), cameraLerp);

    Vector3 newPos = new Vector3(transform.position.x, initialHeight, player.transform.position.z + offset.z);

    float delta = player.transform.position.x - transform.position.x - maxPlayerDistance;
    if (delta > 0)
      velocity.x = cameraVelocity + Mathf.Pow(cameraTransitionBase, delta);

    newPos += velocity * Time.deltaTime;

    transform.position = Vector3.Lerp(transform.position, newPos, cameraLerp);
  }

  public void StartGame()
  {
    started = true;
  }
}
