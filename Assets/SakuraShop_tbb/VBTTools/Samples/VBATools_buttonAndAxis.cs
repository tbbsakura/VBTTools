// Original: VMT Sample, modified by tbbsakura

// 右手左手両方を同時に同じ画面に置けるようにしてある
// 他のスクリプト等で初期化する場合は needEnable を　falseにすることで、最初の送信を省略できる

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace SakuraScript.ModifiedVMTSample
{
    public class VBATools_buttonAndAxis : MonoBehaviour
    {
        public bool isLeft = true;
        public int index = 1;
        public int enable = 5;
        public bool needEnable = false; 

        const float timeoffset = 0f;

        float Trigger0 = 0;
        float Trigger1 = 0;

        float JoystickX = 0;
        float JoystickY = 0;

        uOSC.uOscClient client;
        void Start()
        {
            Application.targetFrameRate = 30;
            client = GetComponent<uOSC.uOscClient>();

            if (needEnable) { 
                client.Send("/VMT/Room/Unity", (int)index, (int)enable, (float)timeoffset,
                    (float)transform.position.x,
                    (float)transform.position.y,
                    (float)transform.position.z,
                    (float)transform.rotation.x,
                    (float)transform.rotation.y,
                    (float)transform.rotation.z,
                    (float)transform.rotation.w
                );
            }
        }


        private void OnGUI()
        {
            int guiX = isLeft ? 50 : 850;
            int guiYNum = 16;

            GUIStyle styleWhite = new GUIStyle(); // gskin.label;
            styleWhite.normal.textColor = Color.white; // GUI.skin.label.normal.textColor;
            GUIStyle styleTitle = new GUIStyle(); // gskin.label;
            styleTitle.normal.textColor  = Color.yellow; // new Color(1,0.1f,0.1f,0.8f);       
            styleTitle.fontStyle = FontStyle.Bold;        

            GUI.Label(new Rect(guiX, 20 * guiYNum++ , 100, 20), isLeft ? "[LEFT]" : "[RIGHT]", styleTitle ); 

            bool Button0 = GUI.RepeatButton(new Rect(guiX, 20 * guiYNum++, 100, 20), "System");
            client.Send("/VMT/Input/Button", (int)index, (int)0, (float)timeoffset, Button0 ? 1 : 0);

            bool Button1 = GUI.RepeatButton(new Rect(guiX, 20 * guiYNum++, 100, 20), "A");
            client.Send("/VMT/Input/Button", (int)index, (int)1, (float)timeoffset, Button1 ? 1 : 0);

            bool Button3 = GUI.RepeatButton(new Rect(guiX, 20 * guiYNum++, 100, 20), "B");
            client.Send("/VMT/Input/Button", (int)index, (int)3, (float)timeoffset, Button3 ? 1 : 0);

            GUI.Label(new Rect(guiX, 20 * guiYNum++, 100, 20), "Trigger", styleWhite);
            Trigger0 = GUI.HorizontalSlider(new Rect(guiX, 20 * guiYNum++, 100, 20), Trigger0, 0, 1);
            client.Send("/VMT/Input/Trigger", (int)index, (int)0, (float)timeoffset, (float)Trigger0);

            GUI.Label(new Rect(guiX, 20 * guiYNum++, 100, 20), "Grip", styleWhite);
            Trigger1 = GUI.HorizontalSlider(new Rect(guiX, 20 * guiYNum++, 100, 20), Trigger1, 0, 1);
            client.Send("/VMT/Input/Trigger", (int)index, (int)1, (float)timeoffset, (float)Trigger1);

            GUI.Label(new Rect(guiX, 20 * guiYNum++, 100, 20), "Joystick", styleWhite);
            JoystickX = GUI.HorizontalSlider(new Rect(guiX, 20 * guiYNum++, 100, 20), JoystickX, -1, 1);
            JoystickY = GUI.HorizontalSlider(new Rect(guiX, 20 * guiYNum++, 100, 20), JoystickY, -1, 1);
            client.Send("/VMT/Input/Joystick", (int)index, (int)0, (float)timeoffset, (float)JoystickX, (float)JoystickY);

            bool JoystickTouch = GUI.RepeatButton(new Rect(guiX, 20 * guiYNum++, 100, 20), "Touch");
            client.Send("/VMT/Input/Joystick/Touch", (int)index, (int)0, (float)timeoffset, JoystickTouch ? 1 : 0);

        }

        void Update()
        {
            
        }
    }

}
