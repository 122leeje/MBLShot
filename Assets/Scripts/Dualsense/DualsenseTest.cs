
using UnityEngine;
using UnityEngine.InputSystem;

public class DualSenseTest : MonoBehaviour
{
    void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    void Start()
    {
        Debug.Log("=== Connected Devices ===");

        foreach (var device in InputSystem.devices)
        {
            PrintDevice(device);
        }
    }

    void OnDeviceChange(
        InputDevice device,
        InputDeviceChange change)
    {
        Debug.Log($"Device Change: {device.name} / {change}");

        if (change == InputDeviceChange.Added)
            PrintDevice(device);
    }

    void PrintDevice(InputDevice device)
    {
        Debug.Log(
            $"Name: {device.name}, " +
            $"Layout: {device.layout}, " +
            $"Manufacturer: {device.description.manufacturer}, " +
            $"Product: {device.description.product}"
        );
    }
}
