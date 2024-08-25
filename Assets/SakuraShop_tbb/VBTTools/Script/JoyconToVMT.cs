/*
 Copyright (c) 2024 Sakura(さくら) / tbbsakura
 MIT License. 

/* Required Liblaries 
    uOSC: https://github.com/hecomi/uOSC
        MIT License : Copyright (c) hecomi 2017-2023
            https://github.com/hecomi/uOSC/blob/master/LICENSE.md

    JoyConLib: https://github.com/Looking-Glass/JoyconLib
        MIT License : Copyright (c)  Looking-Glass 2017-2018
            https://github.com/Looking-Glass/JoyconLib/blob/master/LICENSE
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using uOSC;

namespace SakuraScript.VBTTool {
	[System.Serializable]
	public class JoyconButtonDownEvent : UnityEvent<bool, int> {};
	[System.Serializable]
    public class JoyconButtonUpEvent : UnityEvent<bool, int> {}; 
	[System.Serializable]
    public class JoyconStickEvent : UnityEvent<bool, int> {}; 
    
    [RequireComponent(typeof(uOSC.uOscClient))]
    public class JoyconToVMT : MonoBehaviour {
        [Tooltip("VMTに渡すパラメーター/左手のindex")]
        public int _VMTIndexLeft = 1;
        [Tooltip("VMTに渡すパラメーター/右手のindex")]
        public int _VMTIndexRight = 2;
        
		[SerializeField, Tooltip("bool は 左手true/右手false, int は Joycon.Button (0~12) ")]
		public JoyconButtonDownEvent onButtonDown = new JoyconButtonDownEvent();
		[SerializeField, Tooltip("bool は 左手ならtrue/右手ならfalse, int は Joycon.Button (0~12)")]
		public JoyconButtonUpEvent onButtonUp = new JoyconButtonUpEvent();
		[SerializeField, Tooltip("bool は 左手ならtrue/右手ならfalse, int は TenKey Layout (2=down, etc.)")]
		public JoyconStickEvent onStick = new JoyconStickEvent();

/*      defined in Joycon.cs of JoyConLib
        public enum Button : int
        {
            DPAD_DOWN = 0,  DPAD_RIGHT = 1,  DPAD_LEFT = 2, DPAD_UP = 3,
            SL = 4, SR = 5,
            MINUS = 6, HOME = 7,
            PLUS = 8,  CAPTURE = 9,
            STICK = 10,
            SHOULDER_1 = 11, SHOULDER_2 = 12
        };
*/

        private static readonly Joycon.Button[] m_buttons = Enum.GetValues( typeof( Joycon.Button ) ) as Joycon.Button[];
        private Joycon      m_joyconL;
        private Joycon      m_joyconR;
        private uOscClient  m_client;

        // false にすると、一時的にstickをVMTに通知しなくなる(Callbackは処理される)
        public bool enableStickMove = true;

        void Start ()
        {
            m_client = GetComponent<uOscClient>();
        }

        void OnEnable()
        {
            if (JoyconManager.Instance == null ) {
                Debug.Log( "JoyconManager.Instance is null." );
                return;
            }
            List<Joycon> _joycons = JoyconManager.Instance.j;
            if ( _joycons == null || _joycons.Count <= 0 ) return;
            m_joyconL = _joycons.Find( c =>  c.isLeft );
            m_joyconR = _joycons.Find( c => !c.isLeft );
        }

        void OnDisable()
        {
        }

        public int GetJoyconCount()
        {
            return ((m_joyconL != null ? 1 : 0) + (m_joyconR != null ? 1 : 0));
        }

        void Update () {
            if ( m_joyconL != null ) UpdateOneJoycon( m_joyconL );
            if ( m_joyconR != null ) UpdateOneJoycon( m_joyconR );
        }

        void SendTriggerClick( bool isLeft, int vmtTriggerIndex, int stateValue )
        {
            if ( m_client == null ) return;
            m_client.Send("/VMT/Input/Trigger/Click", isLeft? _VMTIndexLeft : _VMTIndexRight, (int)vmtTriggerIndex, 0f, stateValue );
        }

        void SendTriggerFloat( bool isLeft, int vmtTriggerIndex, float floatValue )
        {
            if ( m_client == null ) return;
            m_client.Send("/VMT/Input/Trigger",  isLeft? _VMTIndexLeft : _VMTIndexRight, (int)vmtTriggerIndex, 0f, (float)floatValue );
        }

        void SendStickClick( bool isLeft, int vmtStickIndex, int stateValue )
        {
            if ( m_client == null ) return;
            m_client.Send("/VMT/Input/Stick/Click", isLeft? _VMTIndexLeft : _VMTIndexRight, (int)vmtStickIndex, 0f, stateValue ); 
        }

        void SendButtonState( bool isLeft, int vmtButtonIndex, int stateValue )
        {
            if ( m_client == null ) return;
            m_client.Send("/VMT/Input/Button", isLeft? _VMTIndexLeft : _VMTIndexRight, (int)vmtButtonIndex, 0f, stateValue ); 
        }

        void UpdateOneJoycon( Joycon j ) 
        {
            foreach (Joycon.Button button in Enum.GetValues(typeof( Joycon.Button )))
            {
                bool _down = j.GetButtonDown(button);
                bool _up = j.GetButtonUp(button);
                bool _hold = j.GetButton(button);

                if ( _down ) {  
                    //Debug.Log($"calling onButtonDown.Invoke {button}");
                    onButtonDown.Invoke( j.isLeft, (int)button );
                }
                else if ( _up ) {
                    //Debug.Log($"calling onButtonUp.Invoke {button}");
                    onButtonUp.Invoke( j.isLeft, (int)button );
                }
                else if ( !_hold ) {
                    continue;
                }

                int vmtButtonIndex = -1;
                int vmtStickIndex = -1;
                switch (button) {
                    case Joycon.Button.DPAD_DOWN: vmtButtonIndex = 1; break;
                    case Joycon.Button.DPAD_UP:   vmtButtonIndex = 3; break;

                    // System Button
                    case Joycon.Button.PLUS:  vmtButtonIndex = 0; break;
                    case Joycon.Button.MINUS: vmtButtonIndex = 0; break;
                    case Joycon.Button.HOME:  vmtButtonIndex = 0; break;
                    case Joycon.Button.CAPTURE: vmtButtonIndex = 0; break;

                    case Joycon.Button.STICK: vmtStickIndex = 1; break;

                    case Joycon.Button.SHOULDER_1: {
                        SendTriggerFloat( j.isLeft, 0, (_down||_hold) ? 0.99f: 0.0f );
                        continue;
                    }
                    case Joycon.Button.SHOULDER_2: {
                        SendTriggerFloat( j.isLeft, 1, (_down||_hold) ? 0.99f: 0.0f ); // Grip
                        SendTriggerFloat( j.isLeft, 2, (_down||_hold) ? 0.50f: 0.0f ); // Force
                        continue;
                    }
                    
                    case Joycon.Button.DPAD_LEFT:  
                    case Joycon.Button.DPAD_RIGHT: 
                    case Joycon.Button.SL:
                    case Joycon.Button.SR:
                    default:
                        break;
                }
                if ( vmtButtonIndex != -1 ) SendButtonState( j.isLeft, vmtButtonIndex, (_down||_hold) ? 1: 0 );
                if ( vmtStickIndex  != -1 ) SendStickClick ( j.isLeft, vmtStickIndex , (_down||_hold) ? 1: 0 ); 
            }

            // Thumb Stick XY 
            float[] stick = j.GetStick();
            //Debug.Log( $"Stick: {stick[0]}, {stick[1]}");
            float jx = stick[0];
            float jy = stick[1];
            if ( stick[0] < -0.75f ) jx = -1f;
            if ( stick[0] >  0.75f ) jx =  1f;
            if ( stick[1] < -0.75f ) jy = -1f;
            if ( stick[1] >  0.75f ) jy =  1f;

            // InvokeStickEvent で特殊なことをするため動きたくない場合、enableStickMove = false で Sendを止められる
            if (enableStickMove)
                m_client.Send("/VMT/Input/Joystick", (int)(j.isLeft? _VMTIndexLeft : _VMTIndexRight), (int)1, 0f, jx, jy );
            InvokeStickEvent(j.isLeft, jx, jy);
        }

        void InvokeStickEvent( bool isLeft, float jx, float jy ) {
            int stickNum = 0;
            // 暫定的に4方向で運用
            if ( jx == -1f && (jy != 1f && jy != -1f) ) stickNum = 4;
            if ( jx ==  1f && (jy != 1f && jy != -1f) ) stickNum = 6;
            if ( jy == -1f && (jx != 1f && jx != -1f) ) stickNum = 2;
            if ( jy ==  1f && (jx != 1f && jx != -1f) ) stickNum = 8;
            if ( stickNum != 0 ) {
                //Debug.Log($"calling onStick.Invoke {stickNum}");
                onStick.Invoke( isLeft, stickNum );
            }
        }
    }
}
