// These are the libraries this script uses

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour{

    
    //Using headers to label variable groups in the Inspector
    [Header("ENEMY SETTINGS")]
    
    //These are the variables that will set the enemy parameters. We are setting them in the GameManager because there is... 
    //...no enemy in the scene by default. They get created at runtime from the GameManager script (see SpawnEnemy() below)
    public float enemyMoveSpeed;
    public float enemyMoveWaveFrequency;
    public float enemyMoveWaveAmplitude;
    [Range(1,10)]public int enemyHealth;
    public float enemyDodgeAmount; 
    public float enemyBulletDetectionRadius;
    public float enemyBulletDetectionDistance;
    public float initialEnemySpawnDelay;
    public float timeBetweenEnemySpawns;
   
     [Range(1,10)] public int maxEnemysOnScreen;
    public float enemyBulletSpeed;
    public float enemyFireDelay;
    [Range(1,15)]public int maxEnemyBulletsOnScreen;

    [Header("POWERUP SETTINGS")]
    public GameObject powerUpPrefab;

    [Header("CAMERA SETTINGS")]
    [Range (0,1)] public float cameraShakePower;
    CameraShake camShake;


    public static int score;
    public static int highScore;

    [Header("UI SETTINGS")]
    public bool enableUI;
    public Gradient healthBarGradient;
    public Color textColor;

    public Text scoreText;
    public Text highScoreText;
    public Text restartText;
    public Image healthBar;
    float targetAmount;

    [Header("OTHER SETTINGS")]
    public SoundManager soundManager;
    public bool enableAudio;
    public bool enableParticles;

    [HideInInspector]
    public GameObject enemyBulletHolder;
    [HideInInspector]
    public GameObject bulletHolder;

    int killCount;
    int killCountMax = 3;
    Vector2 enemyPos;

    GameObject enemyPrefab;
    GameObject enemyBulletPrefab;

    GameObject explosionPrefab;

    ParticleSystem stars;

    float xPos = 9.5f;
    float t;

    public static bool gameOver;



    // Start is always called once at the start of the game, or when the object containing this script first becomes active. 
    private void Start() {
       

        score = 0;

        enemyBulletHolder = new GameObject("EnemyBulletHolder");
        bulletHolder = new GameObject("BulletHolder");

        camShake = Camera.main.gameObject.GetComponent<CameraShake>();

        t = initialEnemySpawnDelay;
        
        //In Unity, when you put a Prefab in a folder called "Assets/Resources", you can then use a Resources.Load method that loads the prefab at runtime.
        //This way you don't need to have the prefab in the scene when the game starts. 
        enemyPrefab = Resources.Load<GameObject>("Enemy") as GameObject;
        enemyBulletPrefab = Resources.Load<GameObject>("EnemyBullet") as GameObject;
        explosionPrefab = Resources.Load<GameObject>("ShipExplosion") as GameObject;

        //Here we get a reference to the Star particle system we have parented to the Camera GameObject in the hierarchy.
        // We only set it as Active if enable particles is set to true;
        stars = Camera.main.gameObject.GetComponentInChildren<ParticleSystem>();
        stars.gameObject.SetActive(enableParticles);

        //We set the camera shake power on the camera to whatever value we set in the inspector
        camShake.shakeAmount = cameraShakePower;

        //Persistent Data has to be written to and fetched from the PlayerPrefs class, which writes the data in your filesystem
        //This way the Highscore remains and can be fetched even when you exit the game.
        highScore = PlayerPrefs.GetInt("highscore", highScore);

        if (enableUI)
        {

            targetAmount = healthBar.fillAmount;


            scoreText.color = textColor;
            highScoreText.color = textColor;
            restartText.color = textColor;

            // This is the Restart text, we are disabling it when the game starts
            restartText.gameObject.SetActive(false);

        }
        scoreText.gameObject.SetActive(enableUI);
        highScoreText.gameObject.SetActive(enableUI);
        restartText.gameObject.SetActive(enableUI);
        healthBar.gameObject.SetActive(enableUI);

}

    // In Unity, Update() is a function that runs every frame. 
    private void Update() {

        if (killCount >= 3)
        {
            SpawnPowerUp();
            killCount = 0;
        }



        // This is a basic timer for spawning enemies
        //It uses the timeBetweenEnemySpawns value to space Spawning apart. We call SpawnEnemy() to do the actual spawning
        if (t > 0) {
            t -= Time.deltaTime;
        } else {
            SpawnEnemy();
            t = timeBetweenEnemySpawns;
        }

        //If we reach a Game Over state, Activate the restart text and reload the scene when the appropriate button is pressed
        if (gameOver) {

            if (Input.GetKeyDown(KeyCode.Return)) {
                gameOver = false;
                SceneManager.LoadScene(0); 
            }
        }

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("highscore", highScore);
        }

        if (enableUI)
        {
            // Here we control the health bar movement, lerping it smoothly when the player takes damage
            float s = 5;
            healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, targetAmount, s * Time.deltaTime);
            // This controls the color of the bar. We take our custom Color Gradient and use the fillAmount property to tell use where in the...
            //...color gradient to take the color from (fillAmount is a value between 0-1). 
            healthBar.color = healthBarGradient.Evaluate(healthBar.fillAmount);


            DisplayScores();

            restartText.gameObject.SetActive(gameOver);
        }


    }

    // Here we spawn an enemy in a random position along the Y axis
    // We then access all the variables of the enemy, setting them to the values we specificed in the Inspector.  
    void SpawnEnemy() {
        var pos = new Vector2(xPos, Random.Range(-3.5f, 3.4f));
        GameObject enemy = Instantiate(enemyPrefab, pos, enemyPrefab.transform.localRotation);

        var es = enemy.GetComponent<EnemyScript>();
        es.moveSpeed = enemyMoveSpeed;
        es.moveWaveFreq = enemyMoveWaveFrequency;
        es.moveWaveAmp = enemyMoveWaveAmplitude;
        es.health = enemyHealth;
        es.dodge = enemyDodgeAmount;
        es.bulletDetectionRad = enemyBulletDetectionRadius;
        es.bulletDetectionDist = enemyBulletDetectionDistance;
        es.bulletHolder = enemyBulletHolder.transform;
        es.bulletPrefab = enemyBulletPrefab;
        es.bulletSpeed = enemyBulletSpeed;
        es.maxBulletsOnScreen = maxEnemyBulletsOnScreen;
        es.fireDelay = enemyFireDelay;
    }

    void SpawnPowerUp()
    {
        var pos = new Vector2(xPos, Random.Range(-3.5f, 3.4f));
        GameObject powerUp = Instantiate(powerUpPrefab, enemyPos, Quaternion.identity);
    }

    public void IncreaseScore() {
        score += 1;
    }

    public void IncreaseKillCounter(Vector2 pos)
    {
        killCount++;
        enemyPos = pos;
    }

    public void GameOver() {
        gameOver = true;
    }



    //This gets called from PlayerScript, when the player takes damage
    public void SetNewFillAmount(int fill, int maxFill) {
       targetAmount = (float)fill / (float)maxFill;////
    }
    void DisplayScores()
    {
        scoreText.text = "Score  " + GameManager.score.ToString("000");
        highScoreText.text = "Best  " + GameManager.highScore.ToString("000");
    }

    //A public method we can call from anywhere to trigger an explosion at a specific spot
    public void TriggerExplosion(Vector2 pos, float shake) {

        //Returns and ends the code execution if Enable Particles is not set to true
        if (!enableParticles) return;

        GameObject e = Instantiate(explosionPrefab, pos, Quaternion.identity);
        camShake.shakeDuration = shake;
        soundManager.PlaySoundAtPosition(pos, 0);

        //See Comment below
        StartCoroutine(RemoveExplosionObject(e));
    }

    // In Unity, IEnumerators are used in Coroutines. 
    // We can use them to run a certain behavior over time even outside of Update().
    // In this case we're telling the game to wait 2.5 secs before executing the code below it.
    // We want to remove the ParticleSystem object we instantiated but we want to make sure it is done playing before we do this, hence the wait time.
    // Ideally we would get the exact lifetime of the particle system instead of putting in an arbitrary value, but this is fine for our current purposes 
    IEnumerator RemoveExplosionObject(GameObject o) {
        yield return new WaitForSeconds(2.5f);
        Destroy(o);
    }
}
