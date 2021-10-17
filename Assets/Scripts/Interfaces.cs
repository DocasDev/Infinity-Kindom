using UnityEngine;

public interface IDamageable<T>
{
    void GetHit(T amount);
}

public interface IKillable<T>
{
    void UpdateHPBar();
}

public interface IAttacker{
    void TargetViwed(Transform target);
    void NoTargetViwed();
}