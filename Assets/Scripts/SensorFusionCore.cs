
using UnityEngine;

public class SensorFusionCore
{
    // Complementary Filter 설정
    private float timeConstant = 0.5f;

    // 추정 자세 (도 단위)
    private float angleX;
    private float angleY;
    private float angleZ;

    private Quaternion rawRotation = Quaternion.identity;
    private Quaternion zeroRotation = Quaternion.identity;

    // 자이로 적분 + 가속도 보정
    public void Update(
        float gyroX, float gyroY, float gyroZ,
        float accelX, float accelY, float accelZ,
        float dt)
    {
        if (dt <= 0f) return;

        // 1. 자이로 적분 (rad/s → degree)
        angleX += gyroX * Mathf.Rad2Deg * dt;
        angleY += gyroY * Mathf.Rad2Deg * dt;
        angleZ += gyroZ * Mathf.Rad2Deg * dt;

        // 2. 가속도 크기 확인
        float magnitude = Mathf.Sqrt(
            accelX * accelX +
            accelY * accelY +
            accelZ * accelZ
        );

        // 가속도에 큰 선형 가속이 섞이지 않았을 때만 보정
        if (magnitude > 7.5f && magnitude < 12f)
        {
            // 3. 중력 방향으로 Roll / Pitch 계산
            float accelAngleX =
                Mathf.Atan2(accelY, accelZ) * Mathf.Rad2Deg;

            float accelAngleY =
                Mathf.Atan2(
                    -accelX,
                    Mathf.Sqrt(accelY * accelY + accelZ * accelZ)
                ) * Mathf.Rad2Deg;

            // 4. Complementary Filter
            float alpha = timeConstant / (timeConstant + dt);

            angleX = Mathf.LerpAngle(
                accelAngleX, angleX, alpha);

            angleY = Mathf.LerpAngle(
                accelAngleY, angleY, alpha);
        }

        // 5. Quaternion 변환
        rawRotation = Quaternion.Euler(
            angleX, angleY, angleZ
        );
    }

    // 현재 자세를 영점으로 설정
    public void SetZero()
    {
        zeroRotation = rawRotation;
    }

    // 영점 기준 상대 자세
    public Quaternion GetRotation()
    {
        return Quaternion.Inverse(zeroRotation) * rawRotation;
    }

    public Quaternion GetRawRotation()
    {
        return rawRotation;
    }
}
