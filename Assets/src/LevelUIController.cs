using LD57;
using TMPro;
using UnityEngine;

public class LevelUIController : MonoBehaviour
{
    private const string DepthText = "Depth {0}";
    private const string RemainingEnemiesText = "{0} Heros Remain";
    private const int HealthFillMaxWidth = 164;

    public GameRoundManager gameRoundManager;
    public GameObject DepthGameObject;
    public GameObject RemainingEnemiesGameObject;
    public TMP_Text CoinTextMesh;
    public RectTransform HealthRect;

    private TMP_Text DepthTextMesh;
    private TMP_Text RemainingEnemiesTextMesh;
    
    private KillableEventHandler killableEventHandler = null;
    private GameRoundEventHandler gameRoundEventHandler = null;

    void Awake()
    {
        DepthTextMesh = DepthGameObject.GetComponent<TMP_Text>();
        RemainingEnemiesTextMesh = RemainingEnemiesGameObject.GetComponent<TMP_Text>();

        DepthTextMesh.text = string.Empty;
        RemainingEnemiesTextMesh.text = string.Empty;

        killableEventHandler = GameObject.FindGameObjectWithTag("EventHandler").GetComponent<KillableEventHandler>();
        gameRoundEventHandler = GameObject.FindGameObjectWithTag("EventHandler").GetComponent<GameRoundEventHandler>();

        killableEventHandler.onSpawned.AddListener(UpdateRemainingEnemies);
        killableEventHandler.onKilled.AddListener(UpdateRemainingEnemies);

        gameRoundEventHandler.onRoundStart.AddListener(HandleRoundStart);
        gameRoundEventHandler.onPlayerHealthChanged.AddListener(HandlePlayerHealthChanged);
        gameRoundEventHandler.onCoinChanged.AddListener(HandleCoinChanged);
    }

    private void UpdateRemainingEnemies(Killable _)
    {
        RemainingEnemiesTextMesh.text = string.Format(RemainingEnemiesText, gameRoundManager.RemainingEnemies);
    }

    private void HandleRoundStart(int depth)
    {
        DepthTextMesh.text = string.Format(DepthText, depth);
        RemainingEnemiesTextMesh.text = string.Format(RemainingEnemiesText, gameRoundManager.RemainingEnemies);
    }

    private void HandlePlayerHealthChanged(Killable killable)
    {
        HealthRect.sizeDelta = new Vector2(HealthFillMaxWidth * (killable.health / killable.maxHealth), HealthRect.sizeDelta.y);
    }
    
    private void HandleCoinChanged(int count)
    {
        CoinTextMesh.text = count.ToString();
    }
}
