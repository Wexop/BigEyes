using System.Collections.Generic;
using System.Linq;
using System.Timers;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace BigEyes.Scripts;

public class BigEyesEnemyAI: EnemyAI
{
    public Color normalEmissive;
    public Color angryEmissive;

    public AudioClip wakeUpSound;
    public AudioClip angrySound;

    public GameObject eyesBody;
    public GameObject normalLight;
    public GameObject angryLight;

    private float sleepingTimer = 15f;
    private float searchTimer = 15f;
    private float attackPlayerTimer = 2f;
    private float wakeUpTimer = 2f;
    public bool isSleeping;
    public float aiInterval;

    private Renderer _render;
    
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Sleep = Animator.StringToHash("Sleep");
    public int lastBehaviorState;
    private static readonly int DamagePlayer = Animator.StringToHash("DamagePlayer");

    public void ChangeEyesMaterial(bool angry)
    {
        normalLight.SetActive(!angry);
        angryLight.SetActive(angry);
        if(_render != null) _render.material.SetColor("_EmissiveColor", angry ? angryEmissive : normalEmissive);
    }

    public override void Start()
    {
        base.Start();
        ChangeEyesMaterial(false);
        SetAnimation();

        List<Renderer> renderers = eyesBody.GetComponents<Renderer>().ToList();
        
        foreach (var r in renderers)
        {
            if (r.material.name.Contains("UvTestBigEye")) _render = r;
        }

        agent.angularSpeed = 400f;
    }

    public override void Update()
    {
        
        base.Update();
        aiInterval -= Time.deltaTime;
        wakeUpTimer -= Time.deltaTime;
        if(isSleeping)sleepingTimer -= Time.deltaTime;
        else searchTimer -= Time.deltaTime;
        
        attackPlayerTimer -= Time.deltaTime;

        if (lastBehaviorState != currentBehaviourStateIndex)
        {
            lastBehaviorState = currentBehaviourStateIndex;
            SetAnimation();
            ChangeEyesMaterial(currentBehaviourStateIndex == 2);
            isSleeping = currentBehaviourStateIndex == 0;

        }
        
        if (GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(transform.position + Vector3.up * 0.25f, 100f, 25))
        {
            if (currentBehaviourStateIndex == 2)
                GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.8f);
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
                    wakeUpTimer = 2f;
                    SwitchToBehaviourClientRpc(1);
                }
                break;
            }
            case 1:
            {
                agent.speed = 5f;
                agent.acceleration = 10f;
                TargetClosestPlayer(requireLineOfSight: true);
                openDoorSpeedMultiplier = 1.5f;

                if (searchTimer <= 0)
                {
                    searchTimer = Random.Range(10f, 20f);
                    SwitchToBehaviourClientRpc(0);
                }
                if (wakeUpTimer > 0)
                {
                    break;
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
                    attackPlayerTimer = 4f;
                    searchTimer += 3f;
                    StopSearch(currentSearch);
                    SwitchToBehaviourClientRpc(2);
                }
                
                break;
            }
            case 2:
            {
                agent.speed = 10f;
                agent.acceleration = 13f;
                openDoorSpeedMultiplier = 0.8f;
                if (attackPlayerTimer <= 0)
                {
                    TargetClosestPlayer(requireLineOfSight: true, viewWidth: 150f);
                    attackPlayerTimer = 2f;
                    searchTimer += 2f;
                }
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
        creatureAnimator.SetTrigger(DamagePlayer);
        var player = MeetsStandardPlayerCollisionConditions(other, false, true);
        if (player != null)
        {
            player.KillPlayer(Vector3.forward * 3f);
        }
        
        PlayAnimationOfCurrentState();
    }
}