// This is the library this script uses
using UnityEngine;

public class EnemyScript : MonoBehaviour {

    //These are the variables that will set the enemy parameters. 
    // We are hiding them from the inspector because, we are setting them in the GameManager and then using those values...
    //...when an enemy is spawned

    GameManager gameManager;
    SoundManager soundManager;

    [HideInInspector]
    public float moveSpeed = 4;

    [HideInInspector]
    public float moveWaveFreq = 5f;

    [HideInInspector]
    public float moveWaveAmp = 0.2f;

    [HideInInspector]
    public int health = 3;

    [HideInInspector]
    public float dodge = 2;

    [HideInInspector]
    public float bulletDetectionRad = 8;

    [HideInInspector]
    public float bulletDetectionDist = 5;

    [HideInInspector]
    public GameObject bulletPrefab;

    [HideInInspector]
    public Transform bulletHolder;

    [HideInInspector]
    public int maxBulletsOnScreen = 10;

    [HideInInspector]
    public float bulletSpeed = 600;

    public GameObject psHit;
    public GameObject enemyAfterburner;
    ParticleSystem hit;

    Rigidbody2D rb;
    SpriteRenderer sr;

    float speedMultiplier = 3f;

    bool bulletFound;
    bool playerFound;
    bool readyToFire;
    bool hitTaken;
    float t;
    float f;
    float flashDuration = 0.1f;

    [HideInInspector]
    public float fireDelay = 1;

    // Start is always called once at the start of the game, or when the object containing this script first becomes active. 
    private void Start() {

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

 
        f = flashDuration;

        psHit.SetActive(gameManager.enableParticles);
        enemyAfterburner.SetActive(gameManager.enableParticles);

        if (psHit.activeSelf)
        {
            hit = psHit.GetComponent<ParticleSystem>();
        }
    }

    // In Unity, Update() is a function that runs every frame. 
    private void Update() {

        BulletDodging();
        Attacking();

        // Here we use a basic timer  for flashing the enemy sprite color when it takes a hit
        if (hitTaken) {
            if (f > 0) {
                sr.color = Color.red;
                f -= Time.deltaTime;
            } else {
                sr.color = Color.white;
                f = flashDuration;
                hitTaken = false;
            }
        }
    }

    //OnCollisionEnter2D is a Unity method for detecting collisions. 
    //If an enemy hits the player, it destroys both ships. 
    // "EnemyBoundary" is an offscreen gameobject that kills the enemy if they make it all the way across the screen... 
    // ...removing enemies that are no longer on screen from the game.
    private void OnCollisionEnter2D(Collision2D collision) {
  
        if (collision.gameObject.tag == "EnemyBoundary") {
            Destroy(gameObject);
        }

        if (collision.gameObject.tag == "Player") {
            collision.gameObject.GetComponent<PlayerScript>().PlayerKilled();
            EnemyKilled();
        }
    }

    //OnTriggerEnter2D is a Unity method for detecting collisions with Triggers. In this case the bullet box collider is set to a trigger.
    //If It hits a GameObject tagged as "PlayerBullet", we call the function TakeHit()
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "PlayerBullet") {
            TakeHit();
        }
    }

    void TakeHit() {
        hitTaken = true;

        //Stop the hit particle system and plays it again if Enable Particles is set to true in the Game Manager
        if (gameManager.enableParticles) {
            hit.Stop();
            hit.Play();

        }

        //Plays the hit sound
        soundManager.PlaySoundAtPosition((Vector2)transform.position, 2);

        //If the enemy health reaches 0, call EnemyKilled()
        if (health - 1 > 0) {
            health -= 1;
        } else {
            EnemyKilled();
        }
    }


    void EnemyKilled() {

        Vector2 pos = transform.position;
        gameManager.IncreaseScore();
        gameManager.TriggerExplosion(pos, 0.3f);
        Destroy(gameObject);
        gameManager.IncreaseKillCounter(pos);
    }

    void Attacking() {
        // This casts a ray from the enemy ship to see if the player is in front of it,
        // If it detects the player, it fires a bullet, assuming it is ready to fire.
        RaycastHit2D playerCheck = Physics2D.Raycast(transform.position, Vector2.left, 15f, LayerMask.GetMask("Player"));

        if (playerCheck) {
            playerFound = true;
        } else {
            playerFound = false;
        }

        if (playerFound && readyToFire) {
            Fire();
            readyToFire = false;
        }

        if (!readyToFire) {

            if (t > 0) {
                t -= Time.deltaTime;
            } else {
                readyToFire = true;
                t = fireDelay;
            }
        }
    }


    void BulletDodging() {
        // This casts a circle ray from the enemy ship to see if a bullet is approaching it. A circle ray has a radius you can adjust to make the detection area larger
        // If it detects a bullet, it tries to move to a random nearby location along the Y axis

        RaycastHit2D bulletCheck = Physics2D.CircleCast(transform.position, bulletDetectionRad / 10, Vector2.left, bulletDetectionDist * 10, LayerMask.GetMask("PlayerBullet"));

        var ranPos = Vector2.zero;

        // By default, the enemy moves in a sin wave pattern towards the left side of the screen
        var defVelocity = Vector2.up * Mathf.Sin(Time.time * moveWaveFreq) * moveWaveAmp;


        if (bulletCheck) {
            bulletFound = true;
            ranPos = Random.insideUnitCircle * dodge;
        } else {
            bulletFound = false;
        }

        if (!bulletFound) {
            rb.velocity = (Vector2.left + defVelocity)  * speedMultiplier;
        } else {
            Vector2 newPos = new Vector2(defVelocity.x, ranPos.y);
            rb.velocity += newPos;
        }
    }

    //When  called, the Fire function checks if the amount of bullets on screen are less then the Max value that we set.
    // If it is , it creates a bullet at a specfic point and sets its speed to the speed variable value we set in the Inspector
    void Fire() {

        // To get the amount of bullers on screen, we get a reference to the Bullet Holder GameObject that is holding all the bullets see how many bullets there are
        var bulletAmount = bulletHolder.gameObject.GetComponentsInChildren<BulletScript>().Length;

        if (bulletAmount < maxBulletsOnScreen) {

            float firePoint = sr.bounds.size.x / 1.5f;
            Vector2 pos = new Vector2(transform.position.x - firePoint, transform.position.y);
            var bullet = Instantiate(bulletPrefab, pos, Quaternion.identity, bulletHolder);
            bullet.GetComponent<BulletScript>().speed = bulletSpeed;

            //Here we access the SoundManager instance and tell it to play the sound at index 1, at the position the bullet was fired.
            soundManager.PlaySoundAtPosition(pos, 1);
        }
    }
}
