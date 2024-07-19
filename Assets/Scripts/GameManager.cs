using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // �� ���ʹ� �� 
    public int totalEnemies = 20;
    // ���� ���ʹ� �� 
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

        // ��� ���ʹ̸� �׿����� Ȯ��
        if(killedEnemies >= totalEnemies)
        {
            // ���� ���� ������ ��ȯ
            SceneManager.LoadScene("GameEndingScene");
        }
    }
}
