// This is the library this script uses
using UnityEngine;

public class CameraShake : MonoBehaviour {


    [HideInInspector]
    public float shakeDuration;

    [HideInInspector]
    public float shakeAmount;
    readonly float decreaseFactor = 1.0f;

    Vector3 originalPos;

    // Start is always called once at the start of the game, or when the object containing this script first becomes active. 
    void Start() {
        originalPos = transform.localPosition;
    }

    // In Unity, Update() is a function that runs every frame. 
    void Update() {
        if (shakeDuration > 0) {
            // this adds a random position within a sphere around the camera's osition multiped by a shakeAmount factor every frame
            // This creates a randomized camera shake for the duration set
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;

            //This decreases the shake duration gradually 
            shakeDuration -= Time.deltaTime * decreaseFactor;
        } else {
            //When the shake duration is 0, return camera to its initial position
            shakeDuration = 0f;
            transform.localPosition = originalPos;
        }
    }
}