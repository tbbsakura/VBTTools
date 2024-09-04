// Original: VMT Sample, modified by tbbsakura

// 右手左手両方を同時に同じ画面に置けるようにしてある
// 他のスクリプト等で初期化する場合は needEnable を　falseにすることで、最初の送信を省略できる

using UnityEngine;

namespace SakuraScript.ModifiedVMTSample
{
    public class VBATools_buttonAndAxis : MonoBehaviour
    {
        [SerializeField] bool isLeft = true;
        [SerializeField] int index = 1;
        [SerializeField] int enable = 5;
        [SerializeField] bool needEnable = false; 

        [SerializeField] Canvas parentCanvas;
        [SerializeField] uOSC.uOscClient client;

        const float timeoffset = 0f;

        void Start()
        {
            Application.targetFrameRate = 30;
            if (needEnable) { 
                client.Send("/VMT/Room/Unity", index, enable, timeoffset,
                    transform.position.x, transform.position.y, transform.position.z,
                    transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w
                );
            }
        }

        // OnGUI
        float Trigger0 = 0;
        float Trigger1 = 0;
        const int scrX = 1280;// OnGUIで基準とする解像度X
        const int scrY = 720; // OnGUIで基準とする解像度Y
        const float guiYSpace = 2f;
        float GetGuiY(int num, float height){ return (height+guiYSpace) * num; }

        private void OnGUI()
        {
            RectTransform r =  (RectTransform)parentCanvas.transform;
            Transform pTransform = transform.parent;
            transform.position = Vector3.zero;
            float guiWidth = 150 * r.localScale.x;
            float guiHeight = 23 * r.localScale.y;
            float guiXOffset = 50 * r.localScale.x; // 左はX0からの距離
            float guiX = isLeft ? guiXOffset : (scrX * r.localScale.x - (guiXOffset+guiWidth) ); // 右は右端から逆算
            guiX = pTransform.position.x - guiWidth / 2f;
            float guiY = 330; // guiYNum == 0 の位置
            guiY = pTransform.position.y + 165* r.localScale.y;
            const float guiYGrp1Offset =5f; // 微調整用
            int guiYNum = 3; // 増える都度下に

            GUIStyle styleWhite = new GUIStyle(); // gskin.label;
            styleWhite.normal.textColor = Color.white; // GUI.skin.label.normal.textColor;
            styleWhite.fontSize = (int)(16* r.localScale.x);
            GUIStyle styleLR = new GUIStyle(); // gskin.label;
            styleLR.normal.textColor = Color.white; // GUI.skin.label.normal.textColor;
            styleLR.fontSize = (int)(20 * r.localScale.x);
            styleLR.fontStyle = FontStyle.Bold;

            // Header
            GUI.Label(
                    new Rect(guiX,guiY + GetGuiY(guiYNum++, guiHeight), guiWidth, guiHeight), 
                    isLeft ? "[LEFT]" : "[RIGHT]", styleLR);
            
            // Group1 : Sys/A/B Buttons
            bool Button0 = GUI.RepeatButton(
                    new Rect(guiX,guiY + GetGuiY(guiYNum++, guiHeight)+guiYGrp1Offset, guiWidth, guiHeight),
                    "System");
            client.Send("/VMT/Input/Button", index, 0, timeoffset, Button0 ? 1 : 0);

            bool Button1 = GUI.RepeatButton(
                new Rect(guiX,guiY + GetGuiY(guiYNum++, guiHeight)+guiYGrp1Offset, guiWidth, guiHeight), "A");
            client.Send("/VMT/Input/Button", index, 1, timeoffset, Button1 ? 1 : 0);

            bool Button3 = GUI.RepeatButton(
                new Rect(guiX,guiY + GetGuiY(guiYNum++, guiHeight)+guiYGrp1Offset, guiWidth, guiHeight), "B");
            client.Send("/VMT/Input/Button", index, 3, timeoffset, Button3 ? 1 : 0);

            guiYNum++;

            // Group 2 : Trigger and Grip
            GUI.Label(new Rect(guiX,guiY + GetGuiY(guiYNum  , guiHeight)-5f, guiWidth*0.4f, guiHeight), "Trigger", styleWhite);
            Trigger0 = GUI.HorizontalSlider(new Rect(guiX+guiWidth*0.4f,guiY + GetGuiY(guiYNum++, guiHeight)-2.5f, guiWidth*0.6f, guiHeight), Trigger0, 0, 1);
            client.Send("/VMT/Input/Trigger", index, 0, timeoffset, Trigger0);

            GUI.Label(new Rect(guiX,guiY + GetGuiY(guiYNum, guiHeight), guiWidth*0.4f, guiHeight), "Grip", styleWhite);
            Trigger1 = GUI.HorizontalSlider(new Rect(guiX+guiWidth*0.4f,guiY + GetGuiY(guiYNum++, guiHeight)+2.5f, guiWidth*0.6f, guiHeight), Trigger1, 0, 1);
            client.Send("/VMT/Input/Trigger", index, 1, timeoffset, Trigger1); // Grip
            client.Send("/VMT/Input/Trigger", index, 2, timeoffset, Trigger1 * 0.8f); // Force

            // Group 3 : Stick
            GUI.Label(new Rect(guiX+guiWidth *0.3f    ,guiY + GetGuiY(guiYNum++, guiHeight), guiWidth, guiHeight), "Joy Stick", styleWhite);
            bool ButtonUp = GUI.RepeatButton(new Rect(guiX+guiWidth*0.333f,guiY + GetGuiY(guiYNum++, guiHeight), guiWidth/3, guiHeight), "Up");
            bool ButtonL = GUI.RepeatButton(new Rect(guiX,guiY + GetGuiY(guiYNum, guiHeight), guiWidth/3, guiHeight), "L");
            bool ButtonR = GUI.RepeatButton(new Rect(guiX+guiWidth*0.666f,guiY + GetGuiY(guiYNum++, guiHeight), guiWidth/3, guiHeight), "R");
            bool ButtonDn= GUI.RepeatButton(new Rect(guiX+guiWidth*0.333f,guiY + GetGuiY(guiYNum++, guiHeight), guiWidth/3, guiHeight), "Dn");
            float JoystickX = 0;
            float JoystickY = 0;
            if ( ButtonUp ) JoystickY = 1f;
            if ( ButtonDn ) JoystickY = -1f;
            if ( ButtonL ) JoystickX = -1f;
            if ( ButtonR ) JoystickX = 1f;
            client.Send("/VMT/Input/Joystick", index, 1, timeoffset, JoystickX, JoystickY);

            bool JoystickTouch = GUI.RepeatButton(new Rect(guiX+guiWidth*0.25f,guiY + GetGuiY(guiYNum++, guiHeight)+4f, guiWidth*0.5f, guiHeight), "Touch");
            client.Send("/VMT/Input/Joystick/Touch", index, 0, timeoffset, JoystickTouch ? 1 : 0);
        }

        void Update()
        {
            
        }
    }

}
