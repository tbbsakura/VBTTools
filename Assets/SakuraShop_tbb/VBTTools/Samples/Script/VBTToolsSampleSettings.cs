// Copyright (c) 2023-2024 Sakura(さくら) / tbbsakura
// MIT License. See "LICENSE" file.

using UnityEngine;
using System.IO;
using SFB;
using System;
using JetBrains.Annotations;
using TMPro;

namespace SakuraScript.VBTTool
{
    public class VBTSetting<T>  
    {
        protected T _data;
        public T Data { get => _data; set => _data = value; }
        /// <summary>
        /// ファイル選択ダイアログを出してからロードする(nullは返さない、必ずインスタンスを返す)
        /// </summary>
        public bool LoadFromFile()  // no path specified
        {
            var extensions = new[] {
                new ExtensionFilter("Json Files", "json" ),
            };

            var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);
            if (paths.Length > 0 && paths[0].Length > 0)
            {
                return LoadFromFile(paths[0]);
            }
            return false;
        }

        public virtual bool LoadFromFile(string path)
        {
            string  json = LoadJsonFromFile(path);
            if (json == "") return false;
            _data = JsonUtility.FromJson<T>(json);
            return true;
        }

        public void SaveToFile()  // no path specified
        {
            var json = JsonUtility.ToJson(_data, true);
            SaveJsonToFile(json);
        }

        public void SaveToFile(string path)
        {
            var json = JsonUtility.ToJson(_data, true);
            StreamWriter sw = new StreamWriter(path,false); 
            sw.Write(json);
            sw.Flush();
            sw.Close();
        }

        // static json load/save functions
        public static string LoadJsonFromFile(string path)
        {
            StreamReader sr = new StreamReader(path, false);
            string json = "";
            while(!sr.EndOfStream) {
                json += sr.ReadLine ();
            }
            sr.Close();
            return json;
        }

        static public void SaveJsonToFile(string json, string nameDefault = "default.json" )
        {
            var extensions = new[] {
                new ExtensionFilter("Json Files", "json" ),
            };

            var path = StandaloneFileBrowser.SaveFilePanel("Save File", "", nameDefault, extensions);

            if (path.Length > 0)
            {
                StreamWriter sw = new StreamWriter(path,false); 
                sw.Write(json);
                sw.Flush();
                sw.Close();
            }
        }
    }

    public class VBTMainSetting : VBTSetting<VBTToolsSetting> 
    {
        public override bool LoadFromFile(string path)  
        {
            string json = LoadJsonFromFile(path);
            _data = JsonUtility.FromJson<VBTToolsSetting>(json);
            _data.SetVersionAsCurrent();
            return true;
        }
    } 


    /// <summary>
    /// v0.2.0 からファイルを分けた
    /// </summary>
    [System.Serializable]
    public class VBTToolsAdjustSetting
    {
        public Vector3 PosL;
        public Vector3 RotEuL;
        public Vector3 PosR;
        public Vector3 RotEuR;
        public Vector3 HandPosL;
        public Vector3 HandPosR;

        public Vector3 RootPosL;
        public Vector3 RootRotL;
        public Vector3 WristPosL;
        public Vector3 WristRotL;
        public Vector3 RootPosR;
        public Vector3 RootRotR;
        public Vector3 WristPosR;
        public Vector3 WristRotR;
    }

    [System.Serializable]
    public class VBTToolsNetworkSetting
    {
        public int _vmcpListenPort = 39544;

        public string _vmtSendAddress = "127.0.0.1";
        public int _vmtSendPort = 39570;
        public int _vmtListenPort = 39571;

        public string _opentrackSendAddress = "127.0.0.1";
        public int _opentrackSendPort = 4242;
    }

    /// <summary>
    /// v0.2.0 以降用の設定保存クラス
    /// </summary>
    [System.Serializable]
    public class VBTToolsSetting
    {
        readonly string _currentVersion = "0.2.0";
        [SerializeField] string _version = "0.0.0";

        public ExRecSetting _exrecSetting;
        public VBTToolsNetworkSetting _networkSetting;

        public string Version => _version;
        public string CurrentVersion => _currentVersion;

        void SetVersion( int a, int b, int c ) {
            _version = $"{a}.{b}.{c}";
        }

        public void SetVersionAsCurrent() {
            _version = _currentVersion;
        }
    }
}
