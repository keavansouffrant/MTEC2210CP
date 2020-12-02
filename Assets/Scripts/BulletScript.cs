// This is the library this script uses
using UnityEngine;

public class BulletScript : MonoBehaviour{

    Rigidbody2D rb;

    [HideInInspector]
    public float speed;

    public bool enemyBullet;

    // Start is always called once at the start of the game, or when the object containing this script first becomes active. 
    private void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    // FixedUpdate is a an Update function that runs at a fixed time Step (Normal update's will vary slightly since it depends on the computer's frametime in milliseconds.
    // In Unity, movement that uses the physics engine should run at a fixed timestep so we put this code in FixedUpdate() 
    private void FixedUpdate() {

        // Here we check if a bullet is an enemy bullet or a player bullet. 
        //If it is an enemy bullet have it move to the left, if it is a player bullet, to the right.
        if (enemyBullet) {
            rb.velocity = (Vector2.left * speed * Time.deltaTime);

        } else {
            rb.velocity = (Vector2.right * speed * Time.deltaTime);
        }
    }

    //OnTriggerEnter2D is a Unity method for detecting collisions with Triggers. In this case the bullet box collider is set to a trigger.
    //Whatever the bullet hits, have it destroy itself
    private void OnTriggerEnter2D(Collider2D collision) {
        Destroy(gameObject);
    }

}
