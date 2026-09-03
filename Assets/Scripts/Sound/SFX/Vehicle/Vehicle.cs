using System;
using CY.Network;
using UnityEngine;

namespace CY.Vehicle
{
    public class Vehicle : MonoBehaviour
    {
        // 액셀 관련 이벤트 (시작 / 중지)
        public static event Action OnAccelStart;
        public static event Action OnAccelStop;

        // 브레이크 관련 이벤트 (시작 / 중지)
        public static event Action OnBrakeStart;
        public static event Action OnBrakeStop;

        [SerializeField] private VehicleSyncManager vehicleSync;

        private bool isAccelling = false;
        private bool isBraking = false;

        private void Awake()
        {
            if (vehicleSync == null)
                vehicleSync = GetComponent<VehicleSyncManager>();
        }

        private void Update()
        {
            if (vehicleSync == null) return;

            // 역할별 입력이 병합된 최신 VehicleInput 데이터를 읽어옴
            VehicleInput input = vehicleSync.ReadMergedInput();

            // --- [1. 액셀 제어] ---
            // Accel 역할을 맡은 플레이어가 입력을 주면 input.Accel > 0f가 됨
            if (input.Accel > 0f && !isAccelling)
            {
                isAccelling = true;
                OnAccelStart?.Invoke();
            }
            else if (input.Accel <= 0f && isAccelling)
            {
                isAccelling = false;
                OnAccelStop?.Invoke();
            }

            // --- [2. 브레이크 제어] ---
            // Brake 역할을 맡은 플레이어가 입력을 주면 input.Brake > 0f가 됨
            if (input.Brake > 0f && !isBraking)
            {
                isBraking = true;
                OnBrakeStart?.Invoke();
            }
            else if (input.Brake <= 0f && isBraking)
            {
                isBraking = false;
                OnBrakeStop?.Invoke();
            }
        }
    }
}