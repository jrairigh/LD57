using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LD57
{
    public class GameRoundManager : MonoBehaviour
    {
        [Tooltip("Time during idle stage of the game round in seconds")]
        public float idleStageSeconds = 30.0f;

        public int RemainingEnemies
        {
            get
            {
                var count = 0;
                aliveKillables.TryGetValue(Team.Team2, out count);
                return count;
            }
        }

        private readonly StateMachine<GameState> stateMachine = new();
        private float idleTimeStart;

        private KillableEventHandler killableEventHandler = null;
        private GameRoundEventHandler gameRoundEventHandler = null;
        private Dictionary<Team, int> aliveKillables = new();
        private Team playerTeam = Team.Neutral;
        private int currentDepth;

        void Start()
        {
            stateMachine
                .Configure(GameState.Initialize)
                .Configure(GameState.GameIdle)
                    .OnEnter(GameIdleOnEnter)
                    .OnUpdate(GameIdleOnUpdate)
                    .Permit(GameState.GameRound)
                .Configure(GameState.GameRound)
                    .OnEnter(GameRoundOnEnter)
                    .OnUpdate(GameRoundOnUpdate)
                    .OnExit(GameRoundOnExit)
                    .Permit(GameState.GameRoundComplete)
                    .Permit(GameState.GameRoundFailed)
                .Configure(GameState.GameRoundComplete)
                    .OnEnter(GameRoundCompleteOnEnter)
                    .Permit(GameState.GameIdle)
                .Configure(GameState.GameRoundFailed)
                    .OnEnter(GameRoundFailedOnEnter)
                    .Permit(GameState.GameIdle); // retry?

            killableEventHandler = GameObject.FindGameObjectWithTag("EventHandler").GetComponent<KillableEventHandler>();
            gameRoundEventHandler = GameObject.FindGameObjectWithTag("EventHandler").GetComponent<GameRoundEventHandler>();
            stateMachine.ChangeState(GameState.GameIdle);
        }

        void Update()
        {
            stateMachine.Update();
        }

        private void GameIdleOnEnter(GameState fromState)
        {
            idleTimeStart = Time.time;
            aliveKillables.Clear();
        }

        private void GameIdleOnUpdate()
        {
            if (idleTimeStart + idleStageSeconds <= Time.time)
            {
                stateMachine.ChangeState(GameState.GameRound);
            }
        }

        private void GameRoundOnEnter(GameState fromState)
        {
            var killables = GameObject.FindObjectsByType<Killable>(FindObjectsSortMode.None);
            foreach (var killable in killables)
            {
                if (killable.health > 0)
                {
                    if (!aliveKillables.ContainsKey(killable.team))
                    {
                        aliveKillables.Add(killable.team, 1);
                    }
                    else
                    {
                        aliveKillables[killable.team]++;
                    }
                }
            }

            killableEventHandler.onSpawned.AddListener(GameRoundAddKillable);
            killableEventHandler.onKilled.AddListener(GameRoundRemoveKillable);

            var spawners = GameObject.FindObjectsByType<EnemySpawnController>(FindObjectsSortMode.None);
            foreach (var spawner in spawners)
            {
                spawner.StartSpawningEnemies();
            }

            playerTeam = GameObject.FindGameObjectWithTag("Player").GetComponent<Killable>().team;

            currentDepth++;
            gameRoundEventHandler.NotifyOnRoundStart(currentDepth);
        }

        private void GameRoundAddKillable(Killable killable)
        {
            if (!aliveKillables.ContainsKey(killable.team))
            {
                aliveKillables.Add(killable.team, 1);
            }
            else
            {
                aliveKillables[killable.team]++;
            }
        }

        private void GameRoundRemoveKillable(Killable killable)
        {
            aliveKillables[killable.team]--;

            if (killable.gameObject.CompareTag("Player"))
            {
                stateMachine.ChangeState(GameState.GameRoundFailed);
            }
        }

        private void GameRoundOnUpdate()
        {
            if (aliveKillables.All(team => team.Key == Team.Neutral || team.Key == playerTeam || team.Value == 0))
            {
                stateMachine.ChangeState(GameState.GameRoundComplete);
            }
        }

        private void GameRoundOnExit(GameState toState)
        {
            killableEventHandler.onSpawned.RemoveListener(GameRoundAddKillable);
            killableEventHandler.onKilled.RemoveListener(GameRoundRemoveKillable);
        }

        private void GameRoundCompleteOnEnter(GameState fromState)
        {
            // TODO: Load next scene before going to next state
            var spawners = GameObject.FindObjectsByType<EnemySpawnController>(FindObjectsSortMode.None);
            foreach (var spawner in spawners)
            {
                spawner.StopSpawningEnemies();
            }

            stateMachine.ChangeState(GameState.GameIdle);
        }

        private void GameRoundFailedOnEnter(GameState fromState)
        {
            // try again
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);

            stateMachine.ChangeState(GameState.GameIdle);
        }

        private enum GameState
        {
            Initialize = 0,
            GameIdle,
            GameRound,
            GameRoundComplete,
            GameRoundFailed
        }
    }
}