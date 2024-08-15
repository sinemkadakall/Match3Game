using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static Potion;

public class PotionBoard : MonoBehaviour
{

    public int width = 6;
    public int height = 8;

    public float spacingX;
    public float spacingY;

    public GameObject[] potionPrefabs;

    public Node[,] potionBoard;
    public GameObject potionBoardGO;

    public ArrayLayout arrayLayout;

    public static PotionBoard instance;

    public List<GameObject> potionToDestroy = new();

    [SerializeField]
    private Potion selectedPotion;


    [SerializeField]
    private bool isProcessingMove;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        InitializeBoard();
    }
    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray =Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin,ray.direction);

            if (hit.collider != null && hit.collider.gameObject.GetComponent<Potion>())
            {
                if (isProcessingMove)
                    return;
                Potion potion = hit.collider.gameObject.GetComponent<Potion>();
                Debug.Log("I have a clicked a potion it is:" + potion.gameObject);
                SelectPotion(potion);

            }
        }


    }

    void InitializeBoard()
    {
        DestroyPotions();
        potionBoard = new Node[width, height];

        spacingX = (float)(width - 1) / 2;
        spacingY = (float)((height - 1) / 2) + 1;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 position = new Vector2((x * 1.1f) - spacingX, (y * 1.3f) - spacingY);

                if (arrayLayout.rows[y].row[x])
                {
                    potionBoard[x, y] = new Node(false, null);
                }
                else
                {
                    int randomIndex = Random.Range(0, potionPrefabs.Length);

                    GameObject potion = Instantiate(potionPrefabs[randomIndex], position, Quaternion.identity);
                    potion.GetComponent<Potion>().SetIndicies(x, y);
                    potionBoard[x, y] = new Node(true, potion);
                    potionToDestroy.Add(potion);

                }
            }
        }
       if(CheckBoard())
        {
            Debug.Log("We have matches let's re-create the board");
            InitializeBoard();
        }
        else
        {
            Debug.Log("There are not matches");
        }
    }

    private void DestroyPotions()
    {
        if(potionToDestroy != null)
        {
            foreach (GameObject potion in potionToDestroy)
            {
                Destroy(potion);
            }
            potionToDestroy.Clear();
        }



    }

    public bool CheckBoard()
    {
        bool hasMatched = false;
        List<Potion> potionToRemove = new();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y].isUsable)
                {
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();

                    if (!potion.isMatched)
                    {
                        MatchResult matchedPotions = IsConnected(potion);

                        if(matchedPotions.connectedPotions.Count >= 3)
                        {
                            potionToRemove.AddRange(matchedPotions.connectedPotions);

                            foreach (Potion pot in matchedPotions.connectedPotions)
                                pot.isMatched = true;
                            
                            hasMatched = true;

                        }
                    }

                }
            }
        }

        return hasMatched;

    }

    MatchResult IsConnected(Potion potion)
    {
        List<Potion> connectedPotions = new();
        PotionType potionType = potion.potionType;

        connectedPotions.Add(potion);

        CheckDirection(potion, new Vector2Int(1, 0), connectedPotions);

        CheckDirection(potion, new Vector2Int(-1, 0), connectedPotions);
        if (connectedPotions.Count == 3)
        {
            Debug.Log("I have a normal horizontal match,the color of my match is:" + connectedPotions[0].potionType);
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Horizontal
            };
        }
        else if (connectedPotions.Count > 3)
        {
            Debug.Log("I have a long horizontal match,the color of my match is:" + connectedPotions[0].potionType);
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongHorizontal
            };
        }
        connectedPotions.Clear();

        connectedPotions.Add(potion);



        CheckDirection(potion, new Vector2Int(0, 1), connectedPotions);

        CheckDirection(potion, new Vector2Int(0, -1), connectedPotions);

        if (connectedPotions.Count == 3)
        {
            Debug.Log("I have a normal vertical match,the color of my match is:" + connectedPotions[0].potionType);
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Vertical
             };
        }
        else if (connectedPotions.Count > 3)
        {
            Debug.Log("I have a long vertical match,the color of my match is:" + connectedPotions[0].potionType);
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongVertical
            };
        }
        else
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.None
            };
        }


    }

    void CheckDirection(Potion pot,Vector2Int direction, List<Potion> connectedPotions)
    {
        PotionType potionType = pot.potionType;
        int x = pot.xIndex + direction.x;
        int y = pot.yIndex + direction.y;

        while(x>=0 && x<width && y>=0 && y<height)
        {
            if (potionBoard[x,y].isUsable) 
            {
                Potion neighbourPotion = potionBoard[x, y].potion.GetComponent<Potion>();

                if(!neighbourPotion.isMatched && neighbourPotion.potionType == potionType )
                {
                    connectedPotions.Add(neighbourPotion);
                    x += direction.x;
                    y += direction.y;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

    }

    #region Swapping Potion

    public void SelectPotion(Potion _potion)
    {
        if(selectedPotion == null)
        {
            Debug.Log(_potion);
            selectedPotion = _potion;
        }
        else if(selectedPotion == _potion)
        {
            selectedPotion = null;
        }
        else if (selectedPotion != _potion)
        {
            SwapPotion(selectedPotion,_potion);
            selectedPotion = null;
        }

    }
     
    private void SwapPotion(Potion _currentPotion,Potion _targetPotion)
    {
        if(!IsAdjacent(_currentPotion, _targetPotion))
        {
            return;
        }

        DoSwap(_currentPotion,_targetPotion);

        isProcessingMove = true;

        StartCoroutine(ProcessMatches(_currentPotion,_targetPotion));
    }

    private void DoSwap(Potion _currentPotion, Potion _targetPotion)
    {

        GameObject temp = potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion;

        potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion = potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion;

        potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion = temp;

        int tempXIndex = _currentPotion.xIndex;
        int tempYIndex = _currentPotion.yIndex;
        _currentPotion.xIndex =_targetPotion.xIndex;
        _currentPotion.yIndex =_targetPotion.yIndex;
        _targetPotion.xIndex = tempXIndex;
        _targetPotion.yIndex = tempYIndex;


        _currentPotion.MoveToTarget(potionBoard[_targetPotion.xIndex,_targetPotion.yIndex].potion.transform.position);
        _targetPotion.MoveToTarget(potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion.transform.position);

    }

    private IEnumerator ProcessMatches(Potion _currentPotion,Potion _targetPotion)
    {

        yield return new WaitForSeconds(0.2f);
        bool hasMatch = CheckBoard();
        if (!hasMatch)
        {
            DoSwap(_currentPotion,_targetPotion);
        }
        isProcessingMove = false;
    }

    private bool IsAdjacent(Potion _currentPotion, Potion _targetPotion)
    {
        return Mathf.Abs(_currentPotion.xIndex-_targetPotion.xIndex) + Mathf.Abs(_currentPotion.yIndex - _targetPotion.yIndex) == 1;
    }

    #endregion 

}
public class MatchResult
{
    public List<Potion> connectedPotions;
    public MatchDirection direction;

}

public enum MatchDirection
{
    Vertical,
    Horizontal,
    LongVertical,
    LongHorizontal,
    Super,
    None
}