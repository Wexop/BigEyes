using UnityEngine;

namespace BigEyes.Scripts;

public class SmallEyeItem: NoisemakerProp
{
    public Animator Animator;
    private static readonly int Angry = Animator.StringToHash("Angry");

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);
        Animator.SetTrigger(Angry);
    }
}