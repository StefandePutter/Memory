using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public enum CardStatus
{
    show_back = 0,
    show_front,
    rotating_to_back,
    rotating_to_front,
    found
}

public class Card : MonoBehaviour
{
    [SerializeField] private CardStatus status;

    [SerializeField] private float turnTargetTime;

    private Vector3 deckLocation = new Vector3(0,-15,0);

    private SpriteRenderer backRenderer;
    private SpriteRenderer frontRenderer;

    private float turnTimer;

    private float counter;

    private Quaternion startRotation;
    private Quaternion targetRotation;

    private Game game;

    private void Awake()
    {
        status = CardStatus.show_back;
        GetFrontAndBackSpriteRenderers();

        game = FindAnyObjectByType<Game>();
    }

    private void Update()
    {
        if (status == CardStatus.rotating_to_front || status == CardStatus.rotating_to_back)
        {
            turnTimer += Time.deltaTime;
            float percentage = turnTimer / turnTargetTime;

            // rotate card
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, percentage);
            
            if (percentage >= 1f)
            {
                // change state when done rotating
                switch (status)
                {
                    case CardStatus.rotating_to_front:
                        status = CardStatus.show_front;
                        break;
                    case CardStatus.rotating_to_back:
                        status = CardStatus.show_back;
                        break;
                }
            }
        }
        if (status == CardStatus.found)
        {
            counter += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, deckLocation, counter / 1f);
        }
    }

    private void OnMouseUp()
    {
        if (game.AllowedToSelectCard(this) == true)
        {
            if (status == CardStatus.show_back)
            {
                game.SelectCard(gameObject);
                TurnToFront();
            }
            else if (status == CardStatus.show_front)
            {
                TurnToBack();
            }
        }
    }

    public void TurnToFront()
    {
        status = CardStatus.rotating_to_front;
        turnTimer = 0f;
        startRotation = transform.rotation;
        targetRotation = Quaternion.Euler(0,180,0);
    }

    public void TurnToBack()
    {
        status = CardStatus.rotating_to_back;
        turnTimer = 0f;
        startRotation = transform.rotation;
        targetRotation = Quaternion.Euler(0, 0, 0);
    }

    public void CardFound()
    {
        status = CardStatus.found;

    }

    private void GetFrontAndBackSpriteRenderers()
    {
        // get components from children
        foreach (Transform t in transform)
        {
            switch (t.name)
            {
                case "Front":
                    frontRenderer = t.GetComponent<SpriteRenderer>();
                    break;
                case "Back":
                    backRenderer = t.GetComponent<SpriteRenderer>();
                    break;
            }
        }
    }
    

    // setters

    public void SetFront(Sprite sprite)
    {
        if (frontRenderer != null)
        {
            frontRenderer.sprite = sprite;
        }
    }

    public void SetBack(Sprite sprite)
    {
        if (backRenderer != null)
        {
            backRenderer.sprite = sprite;
        }
    }

    // getters

    public Vector2 GetFrontSize()
    {
        if (frontRenderer == null)
        {
            Debug.LogError("There is no frontRenderer found.");
        }
        return frontRenderer.bounds.size;
    }

    public Vector2 GetBackSize()
    {
        if (backRenderer == null)
        {
            Debug.LogError("There is no backRenderer found.");
        }
        return backRenderer.bounds.size;
    }
}
