using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SakuraScript.Utils {
	[System.Serializable]
    public class TransformSlidersUpdateEvent : UnityEvent<Vector3, Vector3> {}; 

    public class TransformSliders : MonoBehaviour
    {
        [SerializeField] private Vector3 _position;
        [SerializeField] private Vector3 _eularRotation;
        // 位置スライダーテキストの先頭につける文字列
        [SerializeField] private string _positionHeaderString = "Pos";
        // 回転スライダーテキストの先頭につける文字列
        [SerializeField] private string _rotationHeaderString = "Rot";

		[SerializeField]
		public TransformSlidersUpdateEvent onUpdate = new TransformSlidersUpdateEvent();

        [Space]
        [SerializeField] Text _textPos;
        [SerializeField] Slider _sliderPosX;
        [SerializeField] Slider _sliderPosY;
        [SerializeField] Slider _sliderPosZ;
        [Space]
        [SerializeField] Text _textRot;
        [SerializeField] Slider _sliderRotX;
        [SerializeField] Slider _sliderRotY;
        [SerializeField] Slider _sliderRotZ;

        void Start()
        {
            SetTextAndSlider(); // Inspector で設定されていた値を反映
        }

        void Update()
        {
        }

        void Invoke()
        {
            onUpdate.Invoke( _position, _eularRotation );
        }

        public void SetPosText( string header = "Pos") {
            if (_textPos != null) _textPos.text = $"{header} {_position}";
        }

        public void SetRotText( string header = "Rot") {
            if (_textRot != null) _textRot.text = $"{header} {_eularRotation}";
        }
        
        public void SetValue( Vector3 pos, Vector3 rot ) {
            _position = pos;
            _eularRotation = rot;
            SetTextAndSlider();
        }

        public void SetTextAndSlider() {
            if (_sliderPosX != null) _sliderPosX.value = _position.x;
            if (_sliderPosY != null) _sliderPosY.value = _position.y;
            if (_sliderPosZ != null) _sliderPosZ.value = _position.z;
            SetPosText(_positionHeaderString);

            if ( _sliderRotX != null ) _sliderRotX.value = _eularRotation.x;
            if ( _sliderRotY != null ) _sliderRotY.value = _eularRotation.y;
            if ( _sliderRotZ != null ) _sliderRotZ.value = _eularRotation.z;
            SetRotText(_rotationHeaderString);
        }

        public void OnSliderPosXChanged( float val ) { _position.x = val; SetPosText(_positionHeaderString); Invoke(); }
        public void OnSliderPosYChanged( float val ) { _position.y = val; SetPosText(_positionHeaderString); Invoke(); }
        public void OnSliderPosZChanged( float val ) { _position.z = val; SetPosText(_positionHeaderString); Invoke(); }
        public void OnSliderRotXChanged( float val ) { _eularRotation.x = val; SetRotText(_rotationHeaderString); Invoke();}
        public void OnSliderRotYChanged( float val ) { _eularRotation.y = val; SetRotText(_rotationHeaderString); Invoke();}
        public void OnSliderRotZChanged( float val ) { _eularRotation.z = val; SetRotText(_rotationHeaderString); Invoke();}
    }
}