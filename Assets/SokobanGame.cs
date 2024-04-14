using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SokobanGame : MonoBehaviour
{
    public GameObject SpawnObjects;
    public Sprite SpritePlayerUp;
    public Sprite SpritePlayerDown;
    public Sprite SpritePlayerRight;
    public Sprite SpritePlayerLeft;
    public TMP_Text TextSteps;
    public TMP_Text TextTime;
    public AudioClip backgroundMusic;
    public AudioClip victoryMusic;
    public AudioClip moveBoxSound;
    public AudioSource backgroundMusicSource;
    public AudioSource victoryMusicSource;
    private GameObject player;
    private GameObject[] boxes;
    private int _steps;
    private float _startTime; // Tiempo en el que se inició el nivel
    private float tileSize; // Tamaño de la casilla
    private Vector3 initialPlayerPosition;
    private Dictionary<GameObject, Vector3> initialBoxPositions = new Dictionary<GameObject, Vector3>();
    private bool isVictoryMusicPlaying = false;

    void Start()
    {
        _steps = 0;
        _startTime = Time.time; // Al iniciar el nivel, se guarda el tiempo actual
        SpawnObjects.GetComponent<SpawnObjects>().FillMatrix();
        SpawnObjects.GetComponent<SpawnObjects>().SpawnObjectsMatrix();

        player = GameObject.FindGameObjectWithTag("Player");
        boxes = GameObject.FindGameObjectsWithTag("Box");

        // Obtener el tamaño de la casilla (ancho o altura, que son iguales)
        tileSize = player.GetComponent<SpriteRenderer>().bounds.size.x;

        // Guardar la posición inicial del jugador
        initialPlayerPosition = player.transform.position;

        // Guardar las posiciones iniciales de las cajas
        foreach (GameObject box in boxes)
        {
            initialBoxPositions.Add(box, box.transform.position);
        }

        // Crear y configurar AudioSource para música de fondo
        backgroundMusicSource = gameObject.AddComponent<AudioSource>();
        backgroundMusicSource.clip = backgroundMusic;
        backgroundMusicSource.loop = true;
        backgroundMusicSource.Play();

        // Crear AudioSource para música de victoria
        victoryMusicSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (!isVictoryMusicPlaying && player != null)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                RestartLevel();
                return; // Salir del método para evitar procesamiento adicional
            }
            Vector3 moveDirection = Vector3.zero;
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                moveDirection = Vector3.up;
                player.GetComponent<SpriteRenderer>().sprite = SpritePlayerUp;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                moveDirection = Vector3.down;
                player.GetComponent<SpriteRenderer>().sprite = SpritePlayerDown;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                moveDirection = Vector3.left;
                player.GetComponent<SpriteRenderer>().sprite = SpritePlayerLeft;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                moveDirection = Vector3.right;
                player.GetComponent<SpriteRenderer>().sprite = SpritePlayerRight;
            }

            if (moveDirection != Vector3.zero)
            {
                AttemptMovePlayer(moveDirection);
            }

            CheckWinCondition();
            refreshUI();
            updateTimer(); // Actualizar el temporizador en cada fotograma
        }
    }

    private void AttemptMovePlayer(Vector3 moveDirection)
    {
        Vector3 newPosition = player.transform.position + moveDirection * tileSize;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(newPosition, 0.1f);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Wall"))
            {
                // El jugador no puede moverse a través de paredes
                return;
            }
            else if (collider.CompareTag("Box"))
            {
                // Si hay una caja en la dirección de movimiento, intentamos moverla
                AttemptMoveBox(collider.gameObject, moveDirection);
                return; // Importante: salir del método después de intentar mover la caja
            }
        }

        // Movemos al jugador a la nueva posición
        player.transform.position = newPosition;
        _steps++;
    }

    private void AttemptMoveBox(GameObject box, Vector3 moveDirection)
    {
        Vector3 newPosition = box.transform.position + moveDirection * tileSize;

        // Comprobar si la nueva posición está ocupada por una pared o otra caja
        Collider2D[] colliders = Physics2D.OverlapCircleAll(newPosition, 0.1f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Wall") || collider.CompareTag("Box"))
            {
                // Si la nueva posición está ocupada por una pared o otra caja, la caja no puede moverse
                return;
            }
        }

        // Movemos la caja a la nueva posición
        box.transform.position = newPosition;
        
        // Reproducir sonido de mover caja
        AudioSource.PlayClipAtPoint(moveBoxSound, newPosition);
    }

    private void CheckWinCondition()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Target");
        bool allBoxesOnTargets = true;

        foreach (GameObject target in targets)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(target.transform.position, 0.1f);

            bool boxOnTarget = false;

            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Box"))
                {
                    boxOnTarget = true;
                    break;
                }
            }

            if (!boxOnTarget)
            {
                allBoxesOnTargets = false;
                break;
            }
        }

        if (allBoxesOnTargets)
        {
            Debug.Log("¡Ganaste!");
            StartCoroutine(PlayVictoryMusic());
        }
    }

    IEnumerator PlayVictoryMusic()
    {
        isVictoryMusicPlaying = true;

        // Detener la música de fondo antes de reproducir la música de victoria
        backgroundMusicSource.Stop();

        // Reproducir la música de victoria
        victoryMusicSource.clip = victoryMusic;
        victoryMusicSource.Play();

        // Esperar a que termine la música de victoria antes de cargar la siguiente escena
        yield return new WaitForSeconds(victoryMusic.length);
        loadNextLevel();
    }

    private void loadNextLevel()
    {
        var currentScene = SceneManager.GetActiveScene();
        var currentSceneName = currentScene.name;
        string[] levelName = currentSceneName.Split("Level");
        int nextLevel = int.Parse(levelName[1]) + 1;

        if(nextLevel == 11)
        {
            nextLevel = 1;
        }

        SceneManager.LoadScene("Level"+nextLevel);
    }

    private void updateTimer()
    {
        float elapsedTime = Time.time - _startTime; // Calcular el tiempo transcurrido
        int seconds = Mathf.RoundToInt(elapsedTime); // Redondear al segundo más cercano
        TextTime.text = "Tiempo: " + seconds.ToString(); // Actualizar el texto de tiempo
    }

    private void refreshUI()
    {
        TextSteps.text = "Pasos: " + _steps;
    }

    private void RestartLevel()
    {
        // Reiniciar la posición del jugador
        player.transform.position = initialPlayerPosition;

        // Reiniciar la posición de las cajas
        foreach (var entry in initialBoxPositions)
        {
            GameObject box = entry.Key;
            Vector3 initialPosition = entry.Value;
            box.transform.position = initialPosition;
        }

        // Reiniciar el contador de pasos y tiempo
        _steps = 0;
        _startTime = Time.time;
    }
}
