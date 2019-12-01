using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
  public List<GameObject> prefabs;
  public ParticleSystem waterParticles;
  public AudioSource waterSplash;
  public AudioSource carCrash;

  private PlayerController playerController;

  void Start()
  {
    GameObject prefab = GetRandomPlayerPrefab();
    Instantiate(prefab, transform.position, prefab.transform.rotation, transform);
    playerController = GetComponent<PlayerController>();
    waterParticles.Stop();
  }

  void FixedUpdate()
  {
    waterParticles.transform.position = transform.position;
  }

  void OnTriggerEnter(Collider other)
  {
    if (other.gameObject.CompareTag("Car") || other.gameObject.CompareTag("Bus"))
    {
      carCrash.Play();
      playerController.SetDead();
    }
    else if (other.gameObject.CompareTag("Lily"))
    {
    }
  }

  public void Drown()
  {
    waterParticles.Play();
    playerController.SetDrown();
    waterSplash.PlayDelayed(.1f);
  }

  private GameObject GetRandomPlayerPrefab()
  {
    // return prefabs[3];
    return prefabs[Mathf.FloorToInt(Random.Range(0.0f, 1.0f) * prefabs.Count)];
  }
}
