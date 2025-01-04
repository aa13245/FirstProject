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

    public void EnemyKilled()
    {
        killedEnemies++;

        if(killedEnemies >= totalEnemies)
        {
            StartCoroutine(WaitAndSuccessRoadScene(5f));
        }
    }

    private IEnumerator WaitAndSuccessRoadScene(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        // 엔딩 씬으로 이동한다
        SceneManager.LoadScene("GameEndingScene");
    }

    public void OnClickQuit()
    {
        // 어플 나가기
        Application.Quit();
    }
}
