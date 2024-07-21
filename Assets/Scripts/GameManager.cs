using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    // ���� �ð�
    float currTime = 0;

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

    // ���ʹ̰� �׾����� ����ȭ������ ��ȯ�ϴ� �Լ�
    public void EnemyKilled()
    {
        killedEnemies++;

        // ��� ���ʹ̸� �׿����� Ȯ��
        if(killedEnemies >= totalEnemies)
        {

            // 10 �� �Ŀ� ���� ������ ��ȯ�ϰ� �Ѵ�.
            StartCoroutine(WaitAndSuccessRoadScene(5f));
        }
    }

    private IEnumerator WaitAndSuccessRoadScene(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        // ���� ���� ������ ��ȯ
        SceneManager.LoadScene("GameEndingScene");
    }

    // ������ �����ϴ� �Լ�
    public void OnClickQuit()
    {
        // ����� ���Ͽ��� ����
        Application.Quit();
    }
}
