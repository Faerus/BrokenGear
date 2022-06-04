using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InstanciateOnClick : MonoBehaviour, IPointerClickHandler
{
    [field: SerializeField]
    public GameObject ItemToInstanciate { get; set;  }

    [field: SerializeField]
    public float ScaleVariance { get; set; }

    [field: SerializeField]
    public float SpeedVariance { get; set; }

    public Color TeamColor { get; set; }

    public Animator Animator { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        this.TeamColor = this.GetComponent<SpriteRenderer>().color;
        this.Animator = this.GetComponent<Animator>();

        InvokeRepeating("GenerateItem", 2.5f, 3);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GenerateItem()
    {
        // Check max allowed
        int currentRobot = GameManager.GetRobotCount(this.TeamColor);
        if (currentRobot >= GameManager.MAX_ROBOT_PER_TEAM)
        {
            return;
        }

        // Spend gears
        if(!GameManager.RemoveGears(this.TeamColor, GameManager.PRICE_ROBOT))
        {
            return;
        }

        GameObject item = Instantiate(this.ItemToInstanciate, transform.position, Quaternion.identity);
        item.name = $"{(this.TeamColor == Color.white ? "White" : "Red")} {GameManager.TotalRobotCreated + 1}";
        float speedFactor = Random.Range(this.SpeedVariance * -1, this.SpeedVariance);
        float scaleFactor = Random.Range(this.ScaleVariance * -1, this.ScaleVariance);

        IMoveVelocity move = item.GetComponentInChildren<IMoveVelocity>();
        if (move != null)
        {
            move.Speed += move.Speed * speedFactor;
        }

        SpriteRenderer spriteRenderer = item.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = this.TeamColor;
            spriteRenderer.transform.localScale += spriteRenderer.transform.localScale * scaleFactor;
        }

        IGetHealthSystem living = item.GetComponentInChildren<IGetHealthSystem>();
        if (living != null)
        {
            living.HealthSystem.MaxHealth += living.HealthSystem.MaxHealth * scaleFactor;
            living.HealthSystem.HealFull();
        }

        Robot robot = item.GetComponent<Robot>();
        if (robot != null)
        {
            robot.Power += (int)(robot.Power * scaleFactor);
        }

        RobotAI robotAi = item.GetComponent<RobotAI>();
        if (robotAi != null)// && this.TeamColor == Color.white)
        {
            robotAi.Strategy = (RobotAI.Strategies)(currentRobot % 3 + 1);
            item.name += " (" + robotAi.Strategy + ")";

            robotAi.enabled = false; // Disabled for now
        }

        GameManager.RegisterRobotCreated(this.TeamColor);
        this.Animator.Play("Build", -1, 0f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            this.GenerateItem();
        }
    }
}