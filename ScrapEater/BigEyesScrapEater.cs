using System.Collections;
using System.Collections.Generic;
using com.github.zehsteam.SellMyScrap.MonoBehaviours;
using UnityEngine;

namespace BigEyes.ScrapEater;

public class BigEyesScrapEater: ScrapEaterExtraBehaviour
{

    public Animator animator;
    private static readonly int Angry = Animator.StringToHash("Angry");

    public List<Transform> mouthPos;

    protected override IEnumerator StartAnimation() {
    
        // Move ScrapEater to startPosition
        yield return StartCoroutine(MoveToPosition(spawnPosition, startPosition, 2f));
        PlayOneShotSFX(landSFX, landIndex);
        ShakeCamera();

    
        yield return new WaitForSeconds(1f);

        // Move ScrapEater to endPosition
        PlayAudioSource(movementAudio);
        yield return StartCoroutine(MoveToPosition(startPosition, endPosition, movementDuration));
        StopAudioSource(movementAudio);
        yield return new WaitForSeconds(pauseDuration);

        // Move targetScrap to mouthTransform over time.
        MoveScrapsToEyes( suckDuration - 0.1f);
        yield return new WaitForSeconds(suckDuration);
        
        animator.SetBool(Angry, true);
        
        HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
        HUDManager.Instance.ShakeCamera(ScreenShakeType.Long);
        
        yield return new WaitForSeconds(PlayOneShotSFX(eatSFX));

        yield return new WaitForSeconds(pauseDuration);

        // Move ScrapEater to startPosition
        PlayAudioSource(movementAudio);
        yield return StartCoroutine(MoveToPosition(endPosition, startPosition, movementDuration));
        StopAudioSource(movementAudio);

        yield return new WaitForSeconds(1f);

        // Move ScrapEater to spawnPosition
        PlayOneShotSFX(takeOffSFX);
        yield return StartCoroutine(MoveToPosition(startPosition, spawnPosition, takeOffSFX.length));
    }

    private void MoveScrapsToEyes(float duration)
    {
        targetScrap.ForEach(item =>
        {
            if (item == null) return;

            SuckBehaviour suckBehaviour = item.gameObject.AddComponent<SuckBehaviour>();
            suckBehaviour.StartEvent(mouthPos[Random.Range(0, mouthPos.Count)], Random.Range(duration * 0.75f, duration * 1.3f));
        });
    }
    
}