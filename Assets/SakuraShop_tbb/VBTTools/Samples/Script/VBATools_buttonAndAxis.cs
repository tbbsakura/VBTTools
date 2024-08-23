// Original: VMT Sample, modified by tbbsakura

// 右手左手両方を同時に同じ画面に置けるようにしてある
// 他のスクリプト等で初期化する場合は needEnable を　falseにすることで、最初の送信を省略できる

using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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

        public uOSC.uOscClient client;

        void Start()
        {
            Application.targetFrameRate = 30;

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

        void GetGuiXWH(ref float posx, ref float posy, ref float width, ref float height) 
        {
            var bt = this.transform.Find("ButtonTemplate");
            RectTransform rectTransform = bt.gameObject.GetComponent<RectTransform>();
            Vector3[] v = new Vector3[4];
            rectTransform.GetWorldCorners(v);

            //Debug.Log( $"GetGuiX: {bt.position.x}");
            width = Math.Abs(v[3].x - v[0].x);
            height = Math.Abs(v[3].y - v[2].y);
            posx = (float)bt.position.x - width/2f;
            posy = (float)bt.position.y;
            return ;
        }

        float GetGuiY(int num, float height){
            return height * (float)num;
        }

        private void OnGUI()
        {
            float guiWidth = 0;
            float guiHeight = 0;
            float guiX = 0;
            float guiY = 0;
            GetGuiXWH(ref guiX, ref guiY, ref guiWidth, ref guiHeight);
            int guiYNum = 3;

            GUIStyle styleWhite = new GUIStyle(); // gskin.label;
            styleWhite.normal.textColor = Color.white; // GUI.skin.label.normal.textColor;
            GUIStyle styleTitle = new GUIStyle(); // gskin.label;
            styleTitle.normal.textColor  = Color.yellow; // new Color(1,0.1f,0.1f,0.8f);       
            styleTitle.fontStyle = FontStyle.Bold;        

            bool Button0 = GUI.RepeatButton(new Rect(guiX,guiY + GetGuiY(guiYNum++, guiHeight), guiWidth, guiHeight), "System");
            client.Send("/VMT/Input/Button", (int)index, (int)0, (float)timeoffset, Button0 ? 1 : 0);

            bool Button1 = GUI.RepeatButton(new Rect(guiX,guiY + GetGuiY(guiYNum++, guiHeight), guiWidth, guiHeight), "A");
            client.Send("/VMT/Input/Button", (int)index, (int)1, (float)timeoffset, Button1 ? 1 : 0);

            bool Button3 = GUI.RepeatButton(new Rect(guiX,guiY + GetGuiY(guiYNum++, guiHeight), guiWidth, guiHeight), "B");
            client.Send("/VMT/Input/Button", (int)index, (int)3, (float)timeoffset, Button3 ? 1 : 0);

            GUI.Label(new Rect(guiX,guiY + GetGuiY(guiYNum++, guiHeight), guiWidth, guiHeight), "Trigger", styleWhite);
            Trigger0 = GUI.HorizontalSlider(new Rect(guiX,guiY + GetGuiY(guiYNum++, guiHeight), guiWidth, guiHeight), Trigger0, 0, 1);
            client.Send("/VMT/Input/Trigger", (int)index, (int)0, (float)timeoffset, (float)Trigger0);

            GUI.Label(new Rect(guiX,guiY + GetGuiY(guiYNum++, guiHeight), guiWidth, guiHeight), "Grip", styleWhite);
            Trigger1 = GUI.HorizontalSlider(new Rect(guiX,guiY + GetGuiY(guiYNum++, guiHeight), guiWidth, guiHeight), Trigger1, 0, 1);
            client.Send("/VMT/Input/Trigger", (int)index, (int)1, (float)timeoffset, (float)Trigger1); // Grip
            client.Send("/VMT/Input/Trigger", (int)index, (int)2, (float)timeoffset, (float)Trigger1/2.0f); // Force

            GUI.Label(new Rect(guiX,guiY + GetGuiY(guiYNum++, guiHeight), guiWidth, guiHeight), "Joystick", styleWhite);
            JoystickX = GUI.HorizontalSlider(new Rect(guiX,guiY + GetGuiY(guiYNum++, guiHeight), guiWidth, guiHeight), JoystickX, -1, 1);
            JoystickY = GUI.HorizontalSlider(new Rect(guiX,guiY + GetGuiY(guiYNum++, guiHeight), guiWidth, guiHeight), JoystickY, -1, 1);
            client.Send("/VMT/Input/Joystick", (int)index, (int)0, (float)timeoffset, (float)JoystickX, (float)JoystickY);

            bool JoystickTouch = GUI.RepeatButton(new Rect(guiX,guiY + GetGuiY(guiYNum++, guiHeight), guiWidth, guiHeight), "Touch");
            client.Send("/VMT/Input/Joystick/Touch", (int)index, (int)0, (float)timeoffset, JoystickTouch ? 1 : 0);

        }

        void Update()
        {
            
        }
    }

}
