using System;
using UnityEngine;

public class ForceController : MonoBehaviour
{
    [SerializeField] private SensorReceiver sensor;

    [SerializeField] private float pullThreshold = 5f;
    [SerializeField] private float maxForce = 65f;

    public bool IsPulling { get; private set; }
    public float Power { get; private set; }

    public event Action<float> OnRelease;

    private float peakForce;

    void Update()
    {
        float force = sensor.force;

        if (!IsPulling)
        {
            if (force > pullThreshold)
            {
                IsPulling = true;
                peakForce = force;

                Debug.Log("Pull Start");
            }
        }
        else
        {
            // 당기는 동안 가장 큰 힘을 저장
            if (force > peakForce)
            {
                peakForce = force;
            }

            // 현재까지의 최대 힘을 0~1로 변환
            Power = Mathf.Clamp01(peakForce / maxForce);

            if (force <= pullThreshold)
            {
                float releasedPower = Power;

                Debug.Log($"Release! Peak Force: {peakForce:F2} N / Power: {releasedPower:F2}");

                IsPulling = false;
                peakForce = 0f;
                Power = 0f;

                OnRelease?.Invoke(releasedPower);
            }
        }
    }
}