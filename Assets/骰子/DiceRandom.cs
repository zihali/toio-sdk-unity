using System.Collections;
using toio.Samples.Sample_Sensor;
using UnityEngine;
using UnityEngine.UI;

public class DiceRandom : MonoBehaviour
{
    public Button diceBtn;
    public Image diceDis;
    public Sprite[] dicesprs;

    public static int moveIndex = 0;//�ƶ����ĸ�����
    private void Start()
    {
        // ��ʼ״̬����Ϊ��ȫ͸��
        diceDis.color = new Color(1, 1, 1, 0);
        // �󶨰�ť����¼�
        diceBtn.onClick.AddListener(StartRoll);
    }

    public void StartRoll()
    {
        diceBtn.interactable = false;
        StopAllCoroutines();
        Time.timeScale = 0f;//linshi
        StartCoroutine(RollDiceCoroutine());
    }

    private IEnumerator RollDiceCoroutine()
    {
        // ��ʼ����
        float fadeDuration = 0.5f;
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            diceDis.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
        diceDis.color = Color.white;

        // ���ó�ʼ�������
        diceDis.sprite = dicesprs[Random.Range(0, dicesprs.Length)];

        // ���ӹ���ʱ��
        float rollDuration = 2f;
        float switchInterval = 0.2f;
        float elapsed = 0f;

        while (elapsed < rollDuration)
        {
            // ������ǰ����
            timer = 0f;
            while (timer < switchInterval / 2)
            {
                timer += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(1, 0, timer / (switchInterval / 2));
                diceDis.color = new Color(1, 1, 1, alpha);
                yield return null;
            }

            // �л�����
            diceDis.sprite = dicesprs[Random.Range(0, dicesprs.Length)];

            // �����¾���
            timer = 0f;
            while (timer < switchInterval / 2)
            {
                timer += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(0, 1, timer / (switchInterval / 2));
                diceDis.color = new Color(1, 1, 1, alpha);
                yield return null;
            }

            elapsed += switchInterval;
        }

        // ��ʾ���ս��
        int finalIndex = Random.Range(0, dicesprs.Length);
        diceDis.sprite = dicesprs[finalIndex];
        diceDis.color = Color.white;
        Debug.Log("����Ϊ��" + (finalIndex + 1));

        diceBtn.interactable = true;
        //Sample_Sensor.Instance.CubeMoveByRoll(finalIndex + 1);
    }
}