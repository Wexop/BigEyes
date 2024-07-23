using UnityEngine;

namespace BigEyes.Scripts;

public class SmallEyeItem: NoisemakerProp
{
    public Animator Animator;
    private static readonly int Angry = Animator.StringToHash("Angry");

    public override void Start()
    {
        base.Start();
        noiseAudio.volume = BigEyesPlugin.instance.smallEyesScrapVolume.Value;
        noiseAudioFar.volume = BigEyesPlugin.instance.smallEyesScrapVolume.Value;
    }

    public override void PocketItem()
    {
        base.PocketItem();
        noiseAudio.volume = BigEyesPlugin.instance.smallEyesScrapVolume.Value;
        noiseAudioFar.volume = BigEyesPlugin.instance.smallEyesScrapVolume.Value;
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        noiseAudio.volume = BigEyesPlugin.instance.smallEyesScrapVolume.Value;
        noiseAudioFar.volume = BigEyesPlugin.instance.smallEyesScrapVolume.Value;
        base.ItemActivate(used, buttonDown);
        Animator.SetTrigger(Angry);
    }
}