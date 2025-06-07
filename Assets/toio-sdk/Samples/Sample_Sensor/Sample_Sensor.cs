using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.PlayerLoop;
using System.Collections;
using System;
using Random = UnityEngine.Random;
using toio.Samples.Sample_ConnectName;
using static UnityEditor.Progress;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;

namespace toio.Samples.Sample_Sensor
{
    public class Sample_Sensor : MonoBehaviour
    {
        public static Sample_Sensor Instance;

        //----------
        public Sample_ConnectType sample_ConnectType;
        public Button diceBtn;
        public Image diceDis;
        public Sprite[] dicesprs;

        public int moveIndex = 0;//移动过的格子数
        public int willMoveIndex = 0;//将要移动的格子数

        public TMP_Text hintTextRight;
        public TMP_Text hintTextLeft;
        public TMP_Text gameOverText;
        public List<PokemonData> pokemonDataWithinGrid = new List<PokemonData>();//存储格子内的PokemonData数据
        public List<AIModelWeights> pokemonAIModelWeightsGrid = new List<AIModelWeights>();//存储格子内的PokemonData权重数据
        public Transform overPlan;
        //------------
        private void Awake()
        {
            Instance = this;
        }
        public ConnectType connectType = ConnectType.Real;

        Cube cube;

        async void Start()
        {
            StartCoroutine(GetPokemonDataWithinGrid());
            // 初始状态设置为完全透明
            diceDis.color = new Color(1, 1, 1, 0);
            // 绑定按钮点击事件
            diceBtn.onClick.AddListener(StartRoll);

            await UniTask.Delay(0); // Avoid warning

#if UNITY_EDITOR || !UNITY_WEBGL
            var btn = GameObject.Find("ButtonConnect").GetComponent<Button>();
            btn.gameObject.SetActive(false);
            await Connect();
#endif
        }
        IEnumerator GetPokemonDataWithinGrid()
        {
            yield return new WaitForSeconds(1);
            pokemonDataWithinGrid = SelectRandomElements(AIManager.Instance.trainingData, 36);
            for (int i = 0; i < 36; i++)
            {
                AIModelWeights itemAIModelWeights = new AIModelWeights();

                // 定义临时数组
                float[] speedWeights = { 0, 0.2f, 0.4f, 0.6f, 0.8f, 1 };

                itemAIModelWeights.SpeedWeight = speedWeights[Random.Range(0, 6)];
                itemAIModelWeights.AttackWeight = speedWeights[Random.Range(0, 6)];
                itemAIModelWeights.DefenseWeight = speedWeights[Random.Range(0, 6)];
                itemAIModelWeights.ColorWeight = speedWeights[Random.Range(0, 6)];
                itemAIModelWeights.HasWingsWeight = speedWeights[Random.Range(0, 6)];
                itemAIModelWeights.WeightWeight = speedWeights[Random.Range(0, 6)];
                itemAIModelWeights.HeightWeight = speedWeights[Random.Range(0, 6)];
                itemAIModelWeights.HabitatAltitudeWeight = speedWeights[Random.Range(0, 6)];
                itemAIModelWeights.HabitatTemperatureWeight = speedWeights[Random.Range(0, 6)];


                pokemonAIModelWeightsGrid.Add(new AIModelWeights());
            }
            Debug.Log(pokemonDataWithinGrid.Count);

        }
        //从list中随即取出另一个list  索引不同
        public List<T> SelectRandomElements<T>(List<T> list, int count)
        {
            if (list == null || list.Count == 0)
            {
                return new List<T>();
            }

            if (count >= list.Count)
            {
                return new List<T>(list);
            }

            List<T> result = new List<T>();
            List<int> indices = new List<int>();
            for (int i = 0; i < list.Count; i++)
            {
                indices.Add(i);
            }

            System.Random random = new System.Random();
            for (int i = 0; i < count; i++)
            {
                int randomIndex = random.Next(0, indices.Count);
                int selectedIndex = indices[randomIndex];
                result.Add(list[selectedIndex]);
                indices.RemoveAt(randomIndex);
            }

            return result;
        }
        public void StartRoll()
        {
            //开始摇动骰子  随机  
            AIManager.Instance.UpdateWeightsFromCards();
            Debug.Log(AIManager.Instance.cardRoot.childCount);
            if (AIManager.Instance.cardRoot.childCount<=0)
            {
                StartCoroutine(WaitResultTextLeft("no training model.please build one"));
                return;
            }
            diceBtn.interactable = false;
            StopAllCoroutines();
            //Time.timeScale = 0f;//linshi
            StartCoroutine(RollDiceCoroutine());
        }
        private IEnumerator RollDiceCoroutine()
        {
            // 初始淡入
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

            // 设置初始随机精灵
            diceDis.sprite = dicesprs[Random.Range(0, dicesprs.Length)];

            // 骰子滚动时间
            float rollDuration = 2f;
            float switchInterval = 0.2f;
            float elapsed = 0f;

            while (elapsed < rollDuration)
            {
                // 淡出当前精灵
                timer = 0f;
                while (timer < switchInterval / 2)
                {
                    timer += Time.unscaledDeltaTime;
                    float alpha = Mathf.Lerp(1, 0, timer / (switchInterval / 2));
                    diceDis.color = new Color(1, 1, 1, alpha);
                    yield return null;
                }

                // 切换精灵
                diceDis.sprite = dicesprs[Random.Range(0, dicesprs.Length)];

                // 淡入新精灵
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

            // 显示最终结果
            int finalIndex = Random.Range(0, dicesprs.Length);
            diceDis.sprite = dicesprs[finalIndex];
            diceDis.color = Color.white;
            Debug.Log("点数为：" + (finalIndex + 1));
            willMoveIndex = finalIndex + 1;
            //diceBtn.interactable = true;
            CubeMoveByRoll(willMoveIndex);
            //cube?.TargetMove(targetX: 30, targetY: 90, targetAngle: 180);
            //Coroutine item = StartCoroutine(IECubeMoveByRoll(finalIndex + 1));
            //yield return  item;
        }

        private async UniTask Connect()
        {
            // Cube の接続
            var peripheral = await new CubeScanner(connectType).NearestScan();
            cube = await new CubeConnecter(connectType).Connect(peripheral);

            // モーター速度の読み取りをオンにする
            await cube.ConfigMotorRead(true);

            cube.connectionIntervalCallback.AddListener("Sample_Sensor", OnConnectionInterval);  // Connection Interval

            await cube.ConfigIDNotification(500);       // 精度10ms
            await cube.ConfigIDMissedNotification(500); // 精度10ms
            await cube.ConfigConnectionInterval(100, 200, timeOutSec: 2f, callback: OnConfigConnectionInterval); // 125ms ~ 250ms
            await UniTask.Delay(500);
            cube.ObtainConnectionInterval();
        }

        public async void OnBtnConnect() { await Connect(); }

        public void Forward() { cube?.Move(60, 60, durationMs: 0, order: Cube.ORDER_TYPE.Strong); }
        public void Backward() { cube?.Move(-40, -40, durationMs: 0, order: Cube.ORDER_TYPE.Strong); }
        public void TurnRight() { cube?.Move(60, 30, durationMs: 0, order: Cube.ORDER_TYPE.Strong); }
        public void TurnLeft() { cube?.Move(30, 60, durationMs: 0, order: Cube.ORDER_TYPE.Strong); }
        public void Stop() { cube?.Move(0, 0, durationMs: 0, order: Cube.ORDER_TYPE.Strong); }

        public int rotateSpeeds = 40;
        public int moveSpeeds = 40;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                CubeMoveByRoll(1);
                //StartCoroutine(WaittiME());
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                CubeMoveByRoll(35);
            }
            //if (Input.GetKeyDown(KeyCode.T))
            //{
            //    var item = GameObject.Find("TestText").GetComponent<TMP_Text>();
            //    item.text = "";
            //    item.text += cube.pos;
            //    item.text += "\n";
            //    item.text += cube.sensorPos;
            //    item.text += "\n";
            //    item.text += sample_ConnectType.cubeData.pos;
            //    item.text += "\n";
            //    item.text += sample_ConnectType.cubeData.sensorPos;
            //    item.text += "\n";
            //    Debug.Log(cube.pos);
            //    Debug.Log(cube.sensorPos);
            //    Debug.Log(sample_ConnectType.cubeData.pos);
            //    Debug.Log(sample_ConnectType.cubeData.sensorPos);
            //}
        }
        public float timer = 0.5f;
        public void CubeMoveByRoll(int index)
        {
            StartCoroutine(IECubeMoveByRoll(index));
        }

        IEnumerator IECubeMoveByRoll(int index)
        {
            yield return new WaitForSeconds(1);
            //  第一格为 380 335  -45
            Debug.Log(index);
            var cube2 = sample_ConnectType.cubeData;
            for (int i = 0; i < index; i++)
            {
                Debug.Log(i);
                moveIndex++;
                switch (moveIndex)
                {
                    case 1:
                        cube?.TargetMove(targetX: 335, targetY: 335, targetAngle: 180);
                        cube2?.TargetMove(targetX: 335, targetY: 335, targetAngle: 180);
                        break;
                    case 2:
                        cube?.TargetMove(targetX: 290, targetY: 335, targetAngle: 180);
                        cube2?.TargetMove(targetX: 290, targetY: 335, targetAngle: 180);
                        break;
                    case 3:
                        cube?.TargetMove(targetX: 245, targetY: 335, targetAngle: 180);
                        cube2?.TargetMove(targetX: 245, targetY: 335, targetAngle: 180);
                        break;
                    case 4:
                        cube?.TargetMove(targetX: 200, targetY: 335, targetAngle: 180);
                        cube2?.TargetMove(targetX: 200, targetY: 335, targetAngle: 180);
                        break;
                    case 5:
                        cube?.TargetMove(targetX: 155, targetY: 335, targetAngle: 180);
                        cube2?.TargetMove(targetX: 155, targetY: 335, targetAngle: 180);
                        break;
                    case 6:
                        cube?.TargetMove(targetX: 110, targetY: 335, targetAngle: 180);
                        cube2?.TargetMove(targetX: 110, targetY: 335, targetAngle: 180);
                        break;
                    case 7:
                        //sample_ConnectType.cubeData.Move(rotateSpeeds, -rotateSpeeds, 1, order: Cube.ORDER_TYPE.Strong);
                        cube?.TargetMove(targetX: 110, targetY: 335, targetAngle:  270);
                        cube2?.TargetMove(targetX: 110, targetY: 335, targetAngle: 270);
                        yield return new WaitForSeconds(timer);
                        cube?.TargetMove(targetX: 110, targetY: 290, targetAngle: 270);
                        cube2?.TargetMove(targetX: 110, targetY: 290, targetAngle: 270);
                        break;
                    case 8:
                        cube?.TargetMove(targetX: 110, targetY: 290, targetAngle: 0);
                        cube2?.TargetMove(targetX: 110, targetY: 290, targetAngle: 0);
                        yield return new WaitForSeconds(timer);
                        cube?.TargetMove(targetX: 155, targetY: 290, targetAngle: 0);
                        cube2?.TargetMove(targetX: 155, targetY: 290, targetAngle: 0);
                        break;
                    case 9:
                        cube?.TargetMove(targetX: 200, targetY: 290, targetAngle: 0);
                        cube2?.TargetMove(targetX: 200, targetY: 290, targetAngle: 0);
                        break;
                    case 10:
                        cube?.TargetMove(targetX: 245, targetY: 290, targetAngle: 0);
                        cube2?.TargetMove(targetX: 245, targetY: 290, targetAngle: 0);
                        break;
                    case 11:
                        cube?.TargetMove(targetX: 290, targetY: 290, targetAngle: 0);
                        cube2?.TargetMove(targetX: 290, targetY: 290, targetAngle: 0);
                        break;
                    case 12:
                        cube?.TargetMove(targetX:  335, targetY: 290, targetAngle: 0);
                        cube2?.TargetMove(targetX: 335, targetY: 290, targetAngle: 0);
                        break;
                    case 13:
                        cube?.TargetMove(targetX:  380, targetY: 290, targetAngle: 0);
                        cube2?.TargetMove(targetX: 380, targetY: 290, targetAngle: 0);
                        break;
                    case 14:
                        cube?.TargetMove(targetX: 380, targetY:  290, targetAngle: 270);
                        cube2?.TargetMove(targetX: 380, targetY: 290, targetAngle: 270);
                        yield return new WaitForSeconds(timer);
                        cube?.TargetMove(targetX: 380, targetY:  245, targetAngle: 270);
                        cube2?.TargetMove(targetX: 380, targetY: 245, targetAngle: 270);
                        break;
                    case 15:
                        cube?.TargetMove(targetX: 380, targetY: 245, targetAngle:  180);
                        cube2?.TargetMove(targetX: 380, targetY: 245, targetAngle: 180);
                        yield return new WaitForSeconds(timer);
                        cube?.TargetMove(targetX:  335, targetY: 245, targetAngle: 180);
                        cube2?.TargetMove(targetX: 335, targetY: 245, targetAngle: 180);
                        break;
                    case 16:
                        cube?.TargetMove(targetX:  290, targetY: 245, targetAngle: 180);
                        cube2?.TargetMove(targetX: 290, targetY: 245, targetAngle: 180);
                        break;
                    case 17:
                        cube?.TargetMove(targetX:  245, targetY: 245, targetAngle: 180);
                        cube2?.TargetMove(targetX: 245, targetY: 245, targetAngle: 180);
                        break;
                    case 18:
                        cube?.TargetMove(targetX:  200, targetY: 245, targetAngle: 180);
                        cube2?.TargetMove(targetX: 200, targetY: 245, targetAngle: 180);
                        break;
                    case 19:
                        cube?.TargetMove(targetX:  155, targetY: 245, targetAngle: 180);
                        cube2?.TargetMove(targetX: 155, targetY: 245, targetAngle: 180);
                        break;
                    case 20:
                        cube?.TargetMove(targetX:  110, targetY: 245, targetAngle: 180);
                        cube2?.TargetMove(targetX: 110, targetY: 245, targetAngle: 180);
                        break;
                    case 21:
                        cube?.TargetMove(targetX: 110, targetY: 245, targetAngle:  270);
                        cube2?.TargetMove(targetX: 110, targetY: 245, targetAngle: 270);
                        yield return new WaitForSeconds(timer);
                        cube?.TargetMove(targetX: 110, targetY:  200, targetAngle:  270);
                        cube2?.TargetMove(targetX: 110, targetY: 200, targetAngle: 270);
                        break;
                    case 22:
                        cube?.TargetMove(targetX: 110, targetY: 200, targetAngle: 0);
                        cube2?.TargetMove(targetX: 110, targetY: 200, targetAngle:0);
                        yield return new WaitForSeconds(timer);
                        cube?.TargetMove(targetX:  155, targetY: 200, targetAngle: 0);
                        cube2?.TargetMove(targetX: 155, targetY: 200, targetAngle: 0);
                        break;
                    case 23:
                        cube?.TargetMove(targetX:  200, targetY: 200, targetAngle: 0);
                        cube2?.TargetMove(targetX: 200, targetY: 200, targetAngle: 0);
                        break;
                    case 24:
                        cube?.TargetMove(targetX:  245, targetY: 200, targetAngle: 0);
                        cube2?.TargetMove(targetX: 245, targetY: 200, targetAngle: 0);
                        break;
                    case 25:
                        cube?.TargetMove(targetX:  290, targetY: 200, targetAngle: 0);
                        cube2?.TargetMove(targetX: 290, targetY: 200, targetAngle: 0);
                        break;
                    case 26:
                        cube?.TargetMove(targetX:  335, targetY: 200, targetAngle: 0);
                        cube2?.TargetMove(targetX: 335, targetY: 200, targetAngle: 0);
                        break;
                    case 27:
                        cube?.TargetMove(targetX:  380, targetY: 200, targetAngle: 0);
                        cube2?.TargetMove(targetX: 380, targetY: 200, targetAngle: 0);
                        break;
                    case 28:
                        cube?.TargetMove(targetX: 380, targetY: 200, targetAngle:  270);
                        cube2?.TargetMove(targetX: 380, targetY: 200, targetAngle: 270);
                        yield return new WaitForSeconds(timer);
                        cube?.TargetMove(targetX:  380, targetY: 155, targetAngle: 270);
                        cube2?.TargetMove(targetX: 380, targetY: 155, targetAngle: 270);
                        break;
                    case 29:
                        cube?.TargetMove(targetX: 380, targetY: 155, targetAngle:  180);
                        cube2?.TargetMove(targetX: 380, targetY: 155, targetAngle: 180);
                        yield return new WaitForSeconds(timer);
                        cube?.TargetMove(targetX:  335, targetY: 155, targetAngle: 180);
                        cube2?.TargetMove(targetX: 335, targetY: 155, targetAngle: 180);
                        break;
                    case 30:
                        cube?.TargetMove(targetX:  290, targetY: 155, targetAngle: 180);
                        cube2?.TargetMove(targetX: 290, targetY: 155, targetAngle: 180);
                        break;
                    case 31:
                        cube?.TargetMove(targetX:  245, targetY: 155, targetAngle: 180);
                        cube2?.TargetMove(targetX: 245, targetY: 155, targetAngle: 180);
                        break;
                    case 32:
                        cube?.TargetMove(targetX:  200, targetY: 155, targetAngle: 180);
                        cube2?.TargetMove(targetX: 200, targetY: 155, targetAngle: 180);
                        break;
                    case 33:
                        cube?.TargetMove(targetX:  155, targetY: 155, targetAngle: 180);
                        cube2?.TargetMove(targetX: 155, targetY: 155, targetAngle: 180);
                        break;
                    case 34:
                        cube?.TargetMove(targetX:  110, targetY: 155, targetAngle: 180);
                        cube2?.TargetMove(targetX: 110, targetY: 155, targetAngle: 180);
                        break;
                    default:
                        cube?.Move(0, 0, durationMs: 0, order: Cube.ORDER_TYPE.Strong);
                        cube2?.TargetMove(targetX: 380, targetY: 335, targetAngle: 180);
                        break;
                }

                //if (cube != null && sample_ConnectType.cubeData != null && moveIndex < 35)
                //{
                //    //sample_ConnectType.cubeData.Move(moveSpeeds, moveSpeeds, 1, order: Cube.ORDER_TYPE.Strong);
                //    sample_ConnectType.cubeData.Move(moveSpeeds, moveSpeeds, 1, order: Cube.ORDER_TYPE.Strong);
                    
                //}
                yield return new WaitForSeconds(timer);
                //sample_ConnectType.cubeData?.Move(0, 0, durationMs: 0, order: Cube.ORDER_TYPE.Strong);
                yield return new WaitForSeconds(0.2f);
                if (moveIndex == 7 || moveIndex == 14 || moveIndex == 21 || moveIndex == 28)
                {
                    ChangeGameMode.Instance.OpenBtn();
                    ChangeGameMode.Instance.ChangeMode2();
                    break;
                }
                else
                {
                    ChangeGameMode.Instance.CloseBtn();
                }
                if (moveIndex >= 34)//胜利面板
                {
                    overPlan.gameObject.SetActive(true);
                    overPlan.GetChild(0).gameObject.SetActive(true);
                }
            }
            Debug.Log(123);
            CalculationCorrect(moveIndex);
            diceBtn.interactable = true;
            //sample_ConnectType.cubeData?.Move(0, 0, durationMs: 0, order: Cube.ORDER_TYPE.Strong);
        }

        void CalculationCorrect(int index)
        {
            if (index == 0|| moveIndex == 7 || moveIndex == 14 || moveIndex == 21 || moveIndex == 28) return;
            //pokemonDataWithinGrid
            //    pokemonAIModelWeightsGrid
            Debug.Log(pokemonDataWithinGrid.Count + " : " + pokemonAIModelWeightsGrid.Count + " : " + index);

            AIManager.Instance.UpdateWeightsFromCards();
            

            PokemonType itemPokemonType = AIManager.Instance.PredictPokemonType(pokemonDataWithinGrid[index], AIManager.Instance.currentWeights);
            if (string.Equals(itemPokemonType.ToString(), pokemonDataWithinGrid[index].CorrectType, StringComparison.OrdinalIgnoreCase))
            {
                //预测正确
                cube.TurnLedOn(0, 255, 0, 2000);
                GetComponent<Sample_ConnectType>().cubeData.TurnLedOn(0, 255, 0, 2000);
                GetComponent<Sample_ConnectType>().cubeData.PlayPresetSound(1);
                StartCoroutine(WaitResultTextRight("extract Pokémon information \n Predict Pokémon type \n right Prediction"));
            }
            else
            {
                //预测错误
                cube.TurnLedOn(255, 0, 0, 2000);
                GetComponent<Sample_ConnectType>().cubeData.TurnLedOn(255, 0, 0, 2000);
                GetComponent<Sample_ConnectType>().cubeData.PlayPresetSound(2);
                StartCoroutine(WaitResultTextRight("extract Pokémon information \n Predict Pokémon type \n wrong Prediction"));

                bool res = HealthController.Instance.GetHurt();
                if (!res)
                {
                    Debug.Log(123);
                    overPlan.gameObject.SetActive(true);
                    overPlan.GetChild(1).gameObject.SetActive(true);
                    //死
                    //RefreshPage();
                    //StartCoroutine(WaitGameOverTextTextRight());
                    //HealthController.Instance.HelathsInit();
                }
            }
        }
        IEnumerator WaitResultTextLeft(string str)
        {
            hintTextLeft.gameObject.SetActive(true);
            hintTextLeft.text = str;
            yield return new WaitForSeconds(2);
            hintTextLeft.text = "";
            hintTextLeft.gameObject.SetActive(false) ;
        }
        IEnumerator WaitResultTextRight(string str)
        {
            hintTextRight.text = str;
            hintTextRight.gameObject.SetActive(true);
            yield return new WaitForSeconds(8);
            hintTextRight.text = "";
            hintTextRight.gameObject.SetActive(false) ;
        }
        IEnumerator WaitGameOverTextTextRight()
        {
            gameOverText.gameObject.SetActive(true);
            yield return new WaitForSeconds(2);
            gameOverText.gameObject.SetActive(false) ;
        }

        public void RefreshPage()
        {
            HealthController.Instance.HelathsInit();

            ChangeGameMode.Instance.OpenBtn();
            cube?.TargetMove(targetX: 380, targetY: 335, targetAngle: 180);
            sample_ConnectType.cubeData?.TargetMove(targetX: 380, targetY: 335, targetAngle: 180);
            moveIndex = 0;//移动过的格子数
            willMoveIndex = 0;//将要移动的格子数
        }

        IEnumerator WaittiME()
        {
            yield return new WaitForSeconds(timer);
            sample_ConnectType.cubeData?.Move(0, 0, durationMs: 0, order: Cube.ORDER_TYPE.Strong);
        }

        Cube.MagneticMode magMode = Cube.MagneticMode.Off;
        public async void OnSwitchMag()
        {
            this.magMode = (Cube.MagneticMode)(((int)this.magMode + 1) % 3);
            await cube.ConfigMagneticSensor(
                this.magMode,
                intervalMs: 500,    // 精度20msなの注意
                notificationType: Cube.MagneticNotificationType.OnChanged
            );
            //if (this.magMode == Cube.MagneticMode.Off)
            //    //this.textMag.text = "MagneticSensor Off";
            //else
            //    cube.RequestMagneticSensor();
        }

        int attitudeMode = 0;
        public async void OnSwitchAttitude()
        {
            this.attitudeMode = ((int)this.attitudeMode + 1) % 4;
            if (attitudeMode == 0)
            {
                // The only way to Disable attitude notifications is to set interval to 0
                await cube.ConfigAttitudeSensor(
                    Cube.AttitudeFormat.Eulers, intervalMs: 0,
                    notificationType: Cube.AttitudeNotificationType.OnChanged
                );
                //this.textAttitude.text = "AttitudeSensor Off";
            }
            else
            {
                var format = (Cube.AttitudeFormat)this.attitudeMode;
                await cube.ConfigAttitudeSensor(
                    format, intervalMs: 500,                // 精度10ms
                    notificationType: Cube.AttitudeNotificationType.OnChanged
                );
                cube.RequestAttitudeSensor(format);
            }
        }

        public void OnConfigConnectionInterval(bool success, Cube c)
        {
            Debug.Log("Config Connection Interval success: " + success.ToString());
        }

        public void OnConnectionInterval(Cube c)
        {
            Debug.Log("Current Connection Interval: " + (c.connectionInterval * 1.25f).ToString() + "ms");
        }

    }
}