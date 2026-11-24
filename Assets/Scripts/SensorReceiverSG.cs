
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class SensorReceiverSG : MonoBehaviour
{
    [SerializeField] private int port = 5005;

    public float gyroX, gyroY, gyroZ;
    public float accelX, accelY, accelZ;

    public Quaternion orientation = Quaternion.identity;

    private UdpClient udpClient;
    private Thread receiveThread;
    private volatile bool isRunning;

    private readonly object dataLock = new object();

    private SensorPacket pendingGyro;
    private SensorPacket pendingAccel;

    private SensorFusionCore fusion = new SensorFusionCore();

    private long lastTimestamp;

    [Serializable]
    public class SensorPacket
    {
        public string type;
        public long timestamp;
        public float[] values;
    }

    void Start()
    {
        udpClient = new UdpClient(port);
        isRunning = true;

        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        Debug.Log("SensorReceiverSG 시작");
    }

    void ReceiveData()
    {
        IPEndPoint endpoint =
            new IPEndPoint(IPAddress.Any, port);

        while (isRunning)
        {
            try
            {
                byte[] data = udpClient.Receive(ref endpoint);
                string json = Encoding.UTF8.GetString(data);

                SensorPacket packet =
                    JsonUtility.FromJson<SensorPacket>(json);

                if (packet == null ||
                    packet.values == null ||
                    packet.values.Length < 3)
                    continue;

                lock (dataLock)
                {
                    if (packet.type == "android.sensor.gyroscope")
                        pendingGyro = packet;

                    else if (packet.type == "android.sensor.accelerometer")
                        pendingAccel = packet;
                }
            }
            catch (Exception)
            {
                if (isRunning)
                    Debug.LogWarning("센서 수신 오류");
            }
        }
    }

    void Update()
    {
        SensorPacket gyro = null;
        SensorPacket accel = null;

        lock (dataLock)
        {
            gyro = pendingGyro;
            accel = pendingAccel;

            pendingGyro = null;
            pendingAccel = null;
        }

        if (accel != null)
        {
            accelX = accel.values[0];
            accelY = accel.values[1];
            accelZ = accel.values[2];
        }

        if (gyro != null)
        {
            gyroX = gyro.values[0];
            gyroY = gyro.values[1];
            gyroZ = gyro.values[2];

            float dt = Time.deltaTime;

            if (lastTimestamp > 0 &&
                gyro.timestamp > lastTimestamp)
            {
                dt = (gyro.timestamp - lastTimestamp)
                    * 0.000000001f;
            }

            lastTimestamp = gyro.timestamp;

            // 비정상적으로 큰 시간 간격 방지
            dt = Mathf.Clamp(dt, 0.001f, 0.05f);

            fusion.Update(
                gyroX, gyroY, gyroZ,
                accelX, accelY, accelZ,
                dt
            );

            orientation = fusion.GetRotation();
        }
    }

    // 현재 자세를 영점으로 설정
    public void CalibrateZero()
    {
        fusion.SetZero();
        orientation = Quaternion.identity;

        Debug.Log("영점 설정 완료");
    }

    void OnDestroy()
    {
        isRunning = false;
        udpClient?.Close();

        if (receiveThread != null &&
            receiveThread.IsAlive)
        {
            receiveThread.Join(500);
        }
    }
}
