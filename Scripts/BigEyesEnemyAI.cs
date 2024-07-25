using System.Collections.Generic;
using System.Linq;
using System.Timers;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace BigEyes.Scripts;

public class BigEyesEnemyAI: EnemyAI
{
    public AudioClip wakeUpSound;
    public AudioClip angrySound;

    public GameObject eyesBody;
    public GameObject normalLight;
    public GameObject angryLight;
    
    //CONFIGS
    private float minSleepTime = 10f;
    private float maxSleepTime = 25f;
    
    private float minSearchTime = 10f;
    private float maxSearchTime = 20f;

    private float wakeUpTime = 2f;
    
    private float visionWidth = 150f;

    private float searchSpeed = 5f;
    private float angrySpeed = 10f;
    private float normalAcceleration = 10f;
    private float angryAcceleration = 13f;
    private float angularSpeed = 400f;

    private float chaseTime = 4f;

    private float openDoorMutliplierNormal = 1.5f;
    private float openDoorMutliplierAngry = 0.8f;

    private float sleepingTimer = 15f;
    private float searchTimer = 15f;
    private float attackPlayerTimer = 2f;
    private float wakeUpTimer = 2f;
    public bool isSleeping;
    public float aiInterval;

    private Renderer _render;
    public Material normalMaterial;
    public Material angryMaterial;
    
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Sleep = Animator.StringToHash("Sleep");
    public int lastBehaviorState;
    private static readonly int DamagePlayer = Animator.StringToHash("DamagePlayer");

    public void ChangeEyesMaterial(bool angry)
    {
        normalLight.SetActive(!angry);
        angryLight.SetActive(angry);
        if(_render != null)
        {
            _render.material = angry ? angryMaterial : normalMaterial;
        }
    }

    public void SetConfigs()
    {
     minSleepTime = BigEyesPlugin.instance.minSleepTimeEntry.Value;
     maxSleepTime = BigEyesPlugin.instance.maxSleepTimeEntry.Value;
    
     minSearchTime = BigEyesPlugin.instance.minSearchTimeEntry.Value;
     maxSearchTime = BigEyesPlugin.instance.maxSearchTimeEntry.Value;
     wakeUpTime = BigEyesPlugin.instance.wakeUpTimeEntry.Value;
     
     visionWidth = BigEyesPlugin.instance.visionWidthEntry.Value;
     searchSpeed = BigEyesPlugin.instance.searchSpeedEntry.Value;
     angrySpeed = BigEyesPlugin.instance.angrySpeedEntry.Value;
     
     normalAcceleration = BigEyesPlugin.instance.normalAccelerationEntry.Value;
     angryAcceleration = BigEyesPlugin.instance.angryAccelerationEntry.Value;
     angularSpeed = BigEyesPlugin.instance.angularSpeedEntry.Value;

     chaseTime = BigEyesPlugin.instance.chaseTime.Value;

     openDoorMutliplierNormal = BigEyesPlugin.instance.openDoorMutliplierNormalEntry.Value;
     openDoorMutliplierAngry = BigEyesPlugin.instance.openDoorMutliplierAngryEntry.Value;
    }

    public override void Start()
    {
        base.Start();
        ChangeEyesMaterial(false);
        SetAnimation();
        SetConfigs();

        List<Renderer> renderers = eyesBody.GetComponents<Renderer>().ToList();
        
        foreach (var r in renderers)
        {
            if (r.material.name.Contains("BigEyeNormalText")) _render = r;

        }

        agent.angularSpeed = angularSpeed;
    }

    public override void Update()
    {
        
        base.Update();
        aiInterval -= Time.deltaTime;
        wakeUpTimer -= Time.deltaTime;
        if(isSleeping) sleepingTimer -= Time.deltaTime;
        else searchTimer -= Time.deltaTime;
        
        attackPlayerTimer -= Time.deltaTime;

        if (lastBehaviorState != currentBehaviourStateIndex)
        {
            lastBehaviorState = currentBehaviourStateIndex;
            SetAnimation();
            ChangeEyesMaterial(currentBehaviourStateIndex == 2);
            isSleeping = currentBehaviourStateIndex == 0;

        }
        
        if (GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(transform.position + Vector3.up * 0.25f, 100f, 60))
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
                    sleepingTimer = Random.Range(minSleepTime, maxSleepTime);
                    wakeUpTimer = wakeUpTime;
                    SwitchToBehaviourState(1);
                }
                break;
            }
            case 1:
            {
                agent.speed = searchSpeed;
                agent.acceleration = normalAcceleration;
                TargetClosestPlayer(requireLineOfSight: true, viewWidth: visionWidth);
                openDoorSpeedMultiplier = openDoorMutliplierNormal;
                
                if (targetPlayer == null)
                {
                    
                    if (searchTimer <= 0)
                    {
                        searchTimer = Random.Range(minSearchTime, maxSearchTime);
                        SwitchToBehaviourState(0);
                    }
                    if (wakeUpTimer > 0)
                    {
                        break;
                    }
                    
                    if (currentSearch.inProgress) break;
                    AISearchRoutine aiSearchRoutine = new AISearchRoutine();
                    aiSearchRoutine.searchWidth = 50f;
                    aiSearchRoutine.searchPrecision = 8f;
                    StartSearch(ChooseFarthestNodeFromPosition(transform.position, true).position, aiSearchRoutine);
                }else if (PlayerIsTargetable(targetPlayer))
                {

                    if ((UnityEngine.Object) targetPlayer !=
                        (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController)
                    {
                        ChangeOwnershipOfEnemy(targetPlayer.actualClientId);
                    }
                    else
                    {
                                            
                        attackPlayerTimer = chaseTime;
                        searchTimer = Mathf.Clamp(searchTimer, 0,100) + 3f;
                        StopSearch(currentSearch);
                        SwitchToBehaviourState(2);
                    }

                }
                
                break;
            }
            case 2:
            {
                agent.speed = angrySpeed;
                agent.acceleration = angryAcceleration;
                openDoorSpeedMultiplier = openDoorMutliplierAngry;
                if (attackPlayerTimer <= 0)
                {
                    TargetClosestPlayer(requireLineOfSight: true, viewWidth: visionWidth);
                    attackPlayerTimer = chaseTime / 2;
                    searchTimer += 2f;
                }
                if (targetPlayer != null && PlayerIsTargetable(targetPlayer))
                {
                    SetMovingTowardsTargetPlayer(targetPlayer);
                }
                else
                {
                    SwitchToBehaviourState(1);
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
                searchTimer = Random.Range(minSearchTime, maxSearchTime);
                creatureVoice.clip = null ;
                creatureAnimator.SetBool(Attack, false);
                creatureAnimator.SetBool(Sleep, true);
                break;
            }
            case 1:
            {
                sleepingTimer = Random.Range(minSearchTime, maxSearchTime);
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