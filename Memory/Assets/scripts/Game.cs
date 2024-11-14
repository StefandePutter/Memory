using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private int rows = 3;
    [SerializeField] private int columns = 4;

    [SerializeField] private float totalPairs;

    [SerializeField] private string frontsidesFolder = "Sprites/Frontsides/";
    [SerializeField] private string backsidesFolder = "Sprites/Backsides/";

    [SerializeField] private Sprite[] frontSprites;
    [SerializeField] private Sprite[] backSprites;

    [SerializeField] private Sprite selectedBackSprite;
    [SerializeField] private List<Sprite> selectedFrontSprites;

    [SerializeField] private GameObject cardPrefab;

    private Stack<GameObject> stackOfCards;

    private GameObject[,] placedCards;

    [SerializeField] private Transform fieldAnchor;

    [SerializeField] private float offsetX;
    [SerializeField] private float offsetY;

    private void Start()
    {
        MakeCards();
        DistributeCards();
    }

    private void Update()
    {
        
    }

    private void MakeCards()
    {
        CalculateAmountOfPairs();
        LoadSprites();
        SelectFrontSprites();
        SelectBackSprites();
        ConstructCards();
    }

    private void DistributeCards()
    {
        placedCards = new GameObject[columns, rows];
        ShuffleCards();
        PlaceCardsOnField();
    }

    private void CalculateAmountOfPairs()
    {
        if (rows * columns % 2 == 0)
        {
            totalPairs = rows * columns / 2;
        } else
        {
            Debug.Log("You cant play memory with uneven amount of cards.");
        }
    }

    private void LoadSprites()
    {
        frontSprites = Resources.LoadAll<Sprite>(frontsidesFolder);
        backSprites = Resources.LoadAll<Sprite>(backsidesFolder);
    }

    private void SelectFrontSprites()
    {
        if (frontSprites.Length < totalPairs)
        {
            Debug.Log("There are not enough pictures to make " + totalPairs + " pairs.");
            return;
        }
        selectedFrontSprites = new List<Sprite>();
        
        while (selectedFrontSprites.Count < totalPairs)
        {
            int rnd = Random.Range(0, frontSprites.Length);

            if (selectedFrontSprites.Contains(frontSprites[rnd]) == false)
            {
                selectedFrontSprites.Add(frontSprites[rnd]);
            }
        }
    }

    private void SelectBackSprites()
    {
        if (backSprites.Length > 0)
        {
            int rnd = Random.Range(0, backSprites.Length);

            selectedBackSprite = backSprites[rnd];
        } else
        {
            Debug.Log("There are no backgrounds to choose from.");
        }
    }

    private void ConstructCards()
    {
        // make a new deck of cards
        stackOfCards = new Stack<GameObject>();

        // Make a empty GameObject where we can lay cards under
        GameObject parent = new GameObject();
        parent.name = "Cards";

        // for each Sprite in front sprites that were gonna use in the game
        foreach (Sprite sprite in selectedFrontSprites)
        {
            // Make two identical instances of sprites
            for (int i = 0; i < 2; i++)
            {
                GameObject go = Instantiate(cardPrefab);
                Card cardScript = go.GetComponent<Card>();

                cardScript.SetBack(selectedBackSprite);
                cardScript.SetFront(sprite);

                go.name = sprite.name;
                go.transform.parent = parent.transform;

                stackOfCards.Push(go);
            }
        }
    }

    private void ShuffleCards()
    {
        while (stackOfCards.Count > 0)
        {
            int randX = Random.Range(0, columns);
            int randY = Random.Range(0, rows);

            if (placedCards[randX, randY] == null)
            {
                Debug.Log("Card " + stackOfCards.Peek().name + " is placed on x: " + randX + " y: " + randY);
                placedCards[randX, randY] = stackOfCards.Pop();
            }
        }
    }

    private void PlaceCardsOnField()
    {
        // for overy row on y
        for ( int y = 0; y < rows; y++ )
        {
            // for every card on this row
            for ( int x = 0; x < columns; x++)
            {
                GameObject card = placedCards[x, y];

                Card cardScript = card.GetComponent<Card>();

                Vector2 cardSize = cardScript.GetBackSize();

                float xPos = fieldAnchor.transform.position.x + ( x * ( cardSize.x + offsetX ) );
                float yPos = fieldAnchor.transform.position.y - ( y * ( cardSize.y + offsetY ) );

                placedCards[x,y].transform.position = new Vector3 ( xPos, yPos, 0f );
            }
        }
    }
}
