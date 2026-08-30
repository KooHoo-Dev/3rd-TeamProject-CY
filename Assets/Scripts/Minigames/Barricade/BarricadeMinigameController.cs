using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CY.Minigame
{

    public class BarricadeMinigameController : MonoBehaviour
    {
        public BarricadeMinigameView view;
        
        private List<int> correctSequence = new List<int>(); // 이번 판의 정답 순서
        private int currentStep = 0; // 현재 맞춰야 할 순서 인덱스 (0~2)
        private bool isOverloaded = false; // 과부하(쿨타임) 상태인지 확인
        private bool isGameStarted = false;
        
        private void Start()
        {
            view.InitializeView();
            GenerateSequence();
            
            StartCoroutine(ShowExplanationRoutine());
        }
        
        private void GenerateSequence()
        {
            correctSequence.Clear();
            correctSequence.Add(0);
            correctSequence.Add(1);
            correctSequence.Add(2);

            for (int i = 0; i < correctSequence.Count; i++)
            {
                int temp = correctSequence[i];
                int randomIndex = Random.Range(i, correctSequence.Count);
                correctSequence[i] = correctSequence[randomIndex];
                correctSequence[randomIndex] = temp;
            }

            Debug.Log($"정답 순서: {correctSequence[0]} -> {correctSequence[1]} -> {correctSequence[2]}");
        }
        
        private IEnumerator ShowExplanationRoutine()
        {
            yield return new WaitForSeconds(1.5f);
            view.ShowExplanationPanel();
        }
        
        public void OnGameStartButtonClicked()
        {
            view.HideExplanationPanel();
            StartCoroutine(StartGameRoutine());
        }
        
        private IEnumerator StartGameRoutine()
        {
            yield return new WaitForSeconds(1.0f);
            view.FadeInGameTexts(); 
            isGameStarted = true;   
        }
        
        public void OnFuseClicked(int fuseIndex)
        {
            if (isGameStarted == false || isOverloaded || currentStep >= 3) return;

            if (fuseIndex == correctSequence[currentStep])
            {
                view.SetFuseState(fuseIndex, true);
                currentStep++;

                if (currentStep >= 3)
                {
                    view.ShowResultText("미션 성공!", Color.green);
                    // TODO: 서버 연동 시 여기에 미션 성공 패킷 전송 로직 추가
                }
            }
            else
            {
                StartCoroutine(OverloadRoutine());
            }
        }
        
        private IEnumerator OverloadRoutine()
        {
            isOverloaded = true;
            view.ShowOverloadEffect();

            yield return new WaitForSeconds(2.0f);

            view.HideOverloadEffect();
            
            currentStep = 0;
            isOverloaded = false;
        }
    }

}
