
using UnityEngine;

public class AimController : MonoBehaviour
{
    [SerializeField] private SensorReceiverSG sensor;
    [SerializeField] private Transform target;

    void Update()
    {
        if (sensor == null || target == null)
            return;

        target.rotation = sensor.orientation;
    }
}
