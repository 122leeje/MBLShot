using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class SensorReceiver : MonoBehaviour
{
    private UdpClient udpClient;
    private Thread receiveThread;

    public float force;
    public float gyroX;
    public float gyroY;
    public float gyroZ;

    [SerializeField] private Transform aim;
    [SerializeField] private float gyroSensitivity = 2f;


    void Start()
    {
        udpClient = new UdpClient(5005);

        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        Debug.Log("UDP 수신 시작!");
    }

    void ReceiveData()
    {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 5005);

        while (true)
        {
            try
            {
                byte[] data = udpClient.Receive(ref endPoint);

                string json = Encoding.UTF8.GetString(data);

                SensorData sensorData =
                    JsonUtility.FromJson<SensorData>(json);

                force = sensorData.Force;
                gyroX = sensorData.GyroX;
                gyroY = sensorData.GyroY;
                gyroZ = sensorData.GyroZ;
            }
            catch
            {
                break;
            }
        }
    }

    //void Update()
    //{
    //    float pitch = gyroX * gyroSensitivity * Time.deltaTime;
    //    float yaw = gyroY * gyroSensitivity * Time.deltaTime;

    //    aim.Rotate(
    //        -pitch,
    //        yaw,
    //        0f,
    //        Space.World
    //    );
    //}

    void OnDestroy()
    {
        udpClient?.Close();
        receiveThread?.Abort();
    }

    [Serializable]
    public class SensorData
    {
        public float Force;
        public float GyroX;
        public float GyroY;
        public float GyroZ;
    }
}