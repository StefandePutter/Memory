using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public enum GameStatus
{
    waiting_on_first_card = 0,
    waiting_on_second_card,
    match_found,
    no_match_found
}

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

    private GameStatus status;

    [SerializeField] private GameObject[] selectedCards;

    private float timeoutTimer;

    [SerializeField] private float timeoutTarget;

    private int foundPairs;

    private GameObject parent;

    private void Start()
    {
        MakeCards();
        DistributeCards();

        selectedCards = new GameObject[2];
        status = GameStatus.waiting_on_first_card;
    }

    private void Update()
    {
        if (status == GameStatus.match_found || status == GameStatus.no_match_found)
        {
            RotateBackOrRemovePair();
        }
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
        parent = new GameObject();
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

    public void SelectCard(GameObject card)
    {
        if ( status == GameStatus.waiting_on_first_card )
        {
            selectedCards[0] = card;

            status = GameStatus.waiting_on_second_card;
        }
        else if (status == GameStatus.waiting_on_second_card)
        {
            selectedCards[1] = card;

            CheckForMatchingPair();
        }
    }

    private void CheckForMatchingPair()
    {
        timeoutTimer = 0f;

        if (selectedCards[0].name == selectedCards[1].name)
        {
            status = GameStatus.match_found;
        } else
        {
            status = GameStatus.no_match_found;
        }
    }

    private void RotateBackOrRemovePair()
    {
        timeoutTimer += Time.deltaTime;

        if (timeoutTimer >= timeoutTarget)
        {
            if (status == GameStatus.match_found)
            {
                // if match
                selectedCards[0].GetComponent<Card>().CardFound();
                selectedCards[1].GetComponent<Card>().CardFound();

                foundPairs += 1;

                if (foundPairs == selectedFrontSprites.Count)
                {
                    foundPairs = 0;
                    Restart();
                }
            } 
            else if (status == GameStatus.no_match_found)
            {
                // if not a pair rotate back
                selectedCards[0].GetComponent<Card>().TurnToBack();
                selectedCards[1].GetComponent<Card>().TurnToBack();
            }

            selectedCards[0] = null;
            selectedCards[1] = null;

            status = GameStatus.waiting_on_first_card;
        }
    }

    public bool AllowedToSelectCard(Card card)
    {
        // check if first card is not yet selected
        if (selectedCards[0] == null)
        {
            return true;
        }

        // check if second card is not yet selected
        if (selectedCards[1] == null)
        {
            // check if card isnt the first card
            if (selectedCards[0] != card.gameObject)
            {
                return true;
            }
        }

        return false;
    }

    private void Restart()
    {
        Destroy(parent);

        MakeCards();
        DistributeCards();

        selectedCards = new GameObject[2];
        status = GameStatus.waiting_on_first_card;
    }
}
