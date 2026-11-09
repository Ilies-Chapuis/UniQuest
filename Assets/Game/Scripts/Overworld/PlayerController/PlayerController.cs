using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Boîte vide pour l'instant : elle contiendra le RigidBody2D du joueur.
    private Rigidbody2D rb;

    // Vitesse du joueur
    public float speed = 5f;

    // Boîte qui retient la direction voulue (x, y)
    private Vector2 direction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // On remplit la boîte avec le Rigidbody2D qui est sur le Player
        rb = GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void Update()
    {
        direction = new Vector2 (0, 0);

        // Si la flèche droite est enfoncée en ce moment
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            direction = new Vector2 (1, 0);
        }

        // Si la flèche gauche est enfoncée en ce moment
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            direction = new Vector2 (-1, 0);
        }

        // Si la flèche du bas est enfoncée en ce moment
        if (Keyboard.current.downArrowKey.isPressed)
        {
            direction = new Vector2 (0, -1);
        }

        // Si la flèche du haut est enfoncée en ce moment
        if (Keyboard.current.upArrowKey.isPressed)
        {
            direction = new Vector2 (0, 1);
        }
    }

    // FixedUpdate est appelée à rythme fixe (50 fois par seconde), comme la physique d'Unity.
    // On y applique le mouvement au Rigidbody2D, pour que la vitesse soit la même sur tous les PC.
    void FixedUpdate()
    {
        // La vitesse devient : direction x vitesse (on remplace l'ancienne vitesse)
        rb.linearVelocity = direction * speed;
    }
}