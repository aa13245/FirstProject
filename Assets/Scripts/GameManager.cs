using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
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

    public void EnemyKilled()
    {
        killedEnemies++;

        // 모든 에너미를 죽였는지 확인
        if(killedEnemies >= totalEnemies)
        {
            // 게임 엔딩 씬으로 전환
            SceneManager.LoadScene("GameEndingScene");
        }
    }
}
