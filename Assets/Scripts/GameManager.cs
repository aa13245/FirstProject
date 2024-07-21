using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    // 현재 시간
    float currTime = 0;

    // 총 에너미 수 
    public int totalEnemies = 20;
    // 죽인 에너미 수 
    public int killedEnemies = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 에너미가 죽었으면 성공화면으로 전환하는 함수
    public void EnemyKilled()
    {
        killedEnemies++;

        // 모든 에너미를 죽였는지 확인
        if(killedEnemies >= totalEnemies)
        {

            // 10 초 후에 엔딩 씬으로 전환하게 한다.
            StartCoroutine(WaitAndSuccessRoadScene(5f));
        }
    }

    private IEnumerator WaitAndSuccessRoadScene(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        // 게임 엔딩 씬으로 전환
        SceneManager.LoadScene("GameEndingScene");
    }

    // 게임을 종료하는 함수
    public void OnClickQuit()
    {
        // 빌드된 파일에서 종료
        Application.Quit();
    }
}
