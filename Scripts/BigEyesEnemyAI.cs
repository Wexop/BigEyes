using System.Timers;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace BigEyes.Scripts;

public class BigEyesEnemyAI: EnemyAI
{
    public Material normalMaterial;
    public Material angryMaterial;

    public GameObject[] eyes;
    public GameObject normalLight;
    public GameObject angryLight;

    private float sleepingTimer = 15f;
    private float searchTimer = 15f;
    public bool isSleeping;
    public float aiInterval;
    
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Sleep = Animator.StringToHash("Sleep");
    public int lastBehaviorState;

    public void ChangeEyesMaterial(bool angry)
    {
        normalLight.SetActive(!angry);
        angryLight.SetActive(angry);
        foreach (var o in eyes)
        {
            o.GetComponent<Renderer>().material = angry ? angryMaterial : normalMaterial;
        }
    }

    public override void Start()
    {
        base.Start();
        ChangeEyesMaterial(false);
    }

    public override void Update()
    {
        
        base.Update();
        aiInterval -= Time.deltaTime;
        if(isSleeping)sleepingTimer -= Time.deltaTime;
        else searchTimer -= Time.deltaTime;

        if (lastBehaviorState != currentBehaviourStateIndex)
        {
            lastBehaviorState = currentBehaviourStateIndex;
            SetAnimation();
            ChangeEyesMaterial(currentBehaviourStateIndex == 2);
            if (targetPlayer == GameNetworkManager.Instance.localPlayerController)
            {
                GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(8f);
            }
        }
        
        
        if (aiInterval <= 0 && IsOwner)
        {
            aiInterval = AIIntervalTime;
            DoAIInterval();
        }
    }

    public override void DoAIInterval()
    {
        base.DoAIInterval();

        switch (currentBehaviourStateIndex)
        {
            case 0:
            {
                SetAnimation();
                agent.speed = 0f;
                isSleeping = true;
                openDoorSpeedMultiplier = 0f;
                if (sleepingTimer <= 0)
                {
                    isSleeping = false;
                    sleepingTimer = Random.Range(10f, 25f);
                    SwitchToBehaviourClientRpc(1);
                }
                break;
            }
            case 1:
            {
                SetAnimation();
                agent.speed = 5f;
                TargetClosestPlayer(requireLineOfSight: true);
                openDoorSpeedMultiplier = 1.5f;
                if (searchTimer <= 0)
                {
                    searchTimer = Random.Range(10f, 20f);
                    SwitchToBehaviourClientRpc(0);
                }
                
                if (targetPlayer == null)
                {
                    if (currentSearch.inProgress) break;
                    AISearchRoutine aiSearchRoutine = new AISearchRoutine();
                    aiSearchRoutine.searchWidth = 50f;
                    aiSearchRoutine.searchPrecision = 8f;
                    StartSearch(ChooseFarthestNodeFromPosition(transform.position, true).position, aiSearchRoutine);
                }else if (PlayerIsTargetable(targetPlayer))
                {
                    StopSearch(currentSearch);
                    SwitchToBehaviourClientRpc(2);
                }
                
                break;
            }
            case 2:
            {
                SetAnimation();
                agent.speed = 10f;
                openDoorSpeedMultiplier = 0.8f;
                TargetClosestPlayer(requireLineOfSight: true);
                if (targetPlayer != null && PlayerIsTargetable(targetPlayer))
                {
                    SetMovingTowardsTargetPlayer(targetPlayer);
                }
                else
                {
                    SwitchToBehaviourClientRpc(1);
                }
                break;
            }
            default: break;
                
        }
        

    }

    public void SetAnimation()
    {

        switch (currentBehaviourStateIndex)
        {
            case 0:
            {
                creatureAnimator.SetBool(Attack, false);
                creatureAnimator.SetBool(Sleep, true);
                break;
            }
            case 1:
            {
                creatureAnimator.SetBool(Attack, false);
                creatureAnimator.SetBool(Sleep, false);
                break;
            }
            case 2:
            {
                creatureAnimator.SetBool(Attack, true);
                creatureAnimator.SetBool(Sleep, false);
                break;
            }
        }
    }
    
    public override void OnCollideWithPlayer(Collider other)
    {
        if(isSleeping) return;
        if(other.GetComponent<PlayerControllerB>() == GameNetworkManager.Instance.localPlayerController )
        GameNetworkManager.Instance.localPlayerController.KillPlayer(Vector3.forward);
        PlayAnimationOfCurrentState();
    }
}