using System.Collections.Generic;
using System.Timers;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace BigEyes.Scripts;

public class BigEyesEnemyAI: EnemyAI
{
    public Material normalMaterial;
    public Material angryMaterial;

    public AudioClip wakeUpSound;
    public AudioClip angrySound;

    public GameObject[] eyes;
    public GameObject normalLight;
    public GameObject angryLight;

    private float sleepingTimer = 15f;
    private float searchTimer = 15f;
    public bool isSleeping;
    public float aiInterval;

    private List<Renderer> _renders = new List<Renderer>();
    
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Sleep = Animator.StringToHash("Sleep");
    public int lastBehaviorState;

    public void ChangeEyesMaterial(bool angry)
    {
        normalLight.SetActive(!angry);
        angryLight.SetActive(angry);
        foreach (var o in _renders)
        {
            o.material = angry ? angryMaterial : normalMaterial;
        }
    }

    public override void Start()
    {
        base.Start();
        ChangeEyesMaterial(false);
        SetAnimation();
        foreach (var o in eyes)
        {
            _renders.Add(o.GetComponent<Renderer>());
        }
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
            isSleeping = currentBehaviourStateIndex == 0;

        }
        
        if (GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(transform.position + Vector3.up * 0.25f, 80f, 25))
        {
            if (currentBehaviourStateIndex == 2)
                GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(0.8f);
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
                agent.speed = 15f;
                openDoorSpeedMultiplier = 0.8f;
                TargetClosestPlayer(requireLineOfSight: true, viewWidth: 150f);
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
                creatureVoice.clip = null ;
                creatureAnimator.SetBool(Attack, false);
                creatureAnimator.SetBool(Sleep, true);
                break;
            }
            case 1:
            {
                creatureVoice.clip = wakeUpSound ;
                creatureVoice.Play();
                creatureAnimator.SetBool(Attack, false);
                creatureAnimator.SetBool(Sleep, false);
                break;
            }
            case 2:
            {
                creatureVoice.clip = angrySound ;
                creatureVoice.Play();
                creatureAnimator.SetBool(Attack, true);
                creatureAnimator.SetBool(Sleep, false);
                break;
            }
        }
    }
    
    public override void OnCollideWithPlayer(Collider other)
    {
        if(isSleeping) return;
        var player = MeetsStandardPlayerCollisionConditions(other, false, true);
        if (player != null)
        {
            player.KillPlayer(Vector3.forward * 3f);
        }
        
        PlayAnimationOfCurrentState();
    }
}