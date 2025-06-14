using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace toio.Samples.Sample_WebGL
{
    public class Sample_WebGL_UI : MonoBehaviour
    {
        CubeManager cubeManager;
        Cube cube;

        void Start()
        {
            cubeManager = new CubeManager(ConnectType.Real);
        }

        public async void Connect()
        {
            if (null == cube || !cube.isConnected)
            {
                cube = await cubeManager.SingleConnect();
            }
        }
        public void Disconnect()
        {
            if (null != cube && cube.isConnected)
            {
                cubeManager.DisconnectAll();
            }
        }
        public async void ReConnect()
        {
            if (null != cube && !cube.isConnected)
            {
                await cubeManager.ReConnectAll();
            }
        }

        // 持続時間(durationMs):0にする事で時間無制限となり、一度呼び出すだけで動作し続ける事が出来る。
        // 命令の優先度(order):Cube.ORDER_TYPE.Strongにすることで、一度きりの命令を安全に送信。
        // 【詳細】:
        // 命令の優先度をStrongにすると、内部で命令を命令キューに追加して命令可能フレーム時に順次命令を送る仕組みになっている。
        // 通常は命令前に cubeManager.IsControllable(cube) を呼ぶことで命令可能フレームの確認を行うが、
        // 今回は命令の優先度をStrongにしているため、cubeManager.IsControllable(cube) を呼ばずにそのまま命令キューに追加する。
        // ※ちなみにcubeManager.IsControllable(cube) を呼んで事前に命令可能フレームの確認を行った場合は、パケロスならぬ命令ロスとなる。
        public void Forward() { cube?.Move(60, 60, durationMs:0, order:Cube.ORDER_TYPE.Strong); }
        public void Backward() { cube?.Move(-40, -40, durationMs:0, order:Cube.ORDER_TYPE.Strong); }
        public void TurnRight() { cube?.Move(45, -45, durationMs:0, order:Cube.ORDER_TYPE.Strong); }
        public void TurnLeft() { cube?.Move(-45, 45, durationMs:0, order:Cube.ORDER_TYPE.Strong); }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                cube?.Move(-45, 45, durationMs: 0, order: Cube.ORDER_TYPE.Strong);
                StartCoroutine(WaittiME());
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                cube?.Move(45, -45, durationMs: 0, order: Cube.ORDER_TYPE.Strong);
                StartCoroutine(WaittiME());
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                Debug.Log(cube?.pos);
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                Debug.Log(cube?.eulers);
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                cube?.TargetMove(1,1,50);
            }

            //if (Input.GetKeyUp(KeyCode.A))
            //{
            //    cube?.Move(0, 0, durationMs: 0, order: Cube.ORDER_TYPE.Strong);
            //}
            //if (Input.GetKeyUp(KeyCode.D))
            //{
            //    cube?.Move(0, 0, durationMs: 0, order: Cube.ORDER_TYPE.Strong);
            //}
        }
        IEnumerator WaittiME()
        {
            yield return new WaitForSeconds(0.2f);
            cube?.Move(0, 0, durationMs: 0, order: Cube.ORDER_TYPE.Strong);
        }

        public void Stop() { cube?.Move(0, 0, durationMs:0, order:Cube.ORDER_TYPE.Strong); }
        public void PlayPresetSound1() { cube?.PlayPresetSound(1); }
        public void PlayPresetSound2() { cube?.PlayPresetSound(2); }
        public void LedOn()
        {
            List<Cube.LightOperation> scenario = new List<Cube.LightOperation>();
            float rad = (Mathf.Deg2Rad * (360.0f / 29.0f));
            for (int i = 0; i < 29; i++)
            {
                byte r = (byte)Mathf.Clamp((128 + (Mathf.Cos(rad * i) * 128)), 0, 255);
                byte g = (byte)Mathf.Clamp((128 + (Mathf.Sin(rad * i) * 128)), 0, 255);
                byte b = (byte)Mathf.Clamp(((Mathf.Abs(Mathf.Cos(rad * i) * 255))), 0, 255);
                scenario.Add(new Cube.LightOperation(100, r, g, b));
            }
            cube?.TurnOnLightWithScenario(0, scenario.ToArray());
        }
        public void LedOff() { cube?.TurnLedOff(); }
    }
}