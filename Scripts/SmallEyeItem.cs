using UnityEngine;

namespace BigEyes.Scripts;

public class SmallEyeItem: NoisemakerProp
{
    public Animator Animator;
    private static readonly int Angry = Animator.StringToHash("Angry");
    public AudioClip grabSFX;

    public override void Start()
    {
        base.Start();
        ResetVolume();
    }

    public override void PocketItem()
    {
        base.PocketItem();
        ResetVolume();
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        ResetVolume();
        base.ItemActivate(used, buttonDown);
        Animator.SetTrigger(Angry);
    }

    public void ResetVolume()
    {
        noiseAudio.volume = BigEyesPlugin.instance.smallEyesScrapVolume.Value;
        noiseAudioFar.volume = BigEyesPlugin.instance.smallEyesScrapVolume.Value;
        minLoudness = BigEyesPlugin.instance.smallEyesScrapVolume.Value;
        maxLoudness = BigEyesPlugin.instance.smallEyesScrapVolume.Value;
    }

    public override void GrabItem()
    {
        ResetVolume();
        noiseAudio.PlayOneShot(grabSFX);
        base.GrabItem();
    }
}