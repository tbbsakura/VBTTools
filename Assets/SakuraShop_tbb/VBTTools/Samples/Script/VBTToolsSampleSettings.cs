// Copyright (c) 2023-2024 Sakura(さくら) / tbbsakura
// MIT License. See "LICENSE" file.

using UnityEngine;
using System.IO;
using SFB;
using System;

namespace SakuraScript.VBTTool
{
    /// <summary>
    /// VBTTools v0.1.0 or before で使われていたクラス
    /// v0.2.0 からファイルを分け、保存項目も増えているので VBTToolsSetting クラスの一部となった
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

        public static VBTToolsAdjustSetting LoadFromFile(string path)
        {
            string  json = VBTToolsSetting.LoadJsonFromFile(path);
            var obj = JsonUtility.FromJson<VBTToolsAdjustSetting>(json);
            return obj;
        }
        /*
        public static void SaveToFile(VBTToolsAdjustSetting _adjSetting)
        {
            var json = JsonUtility.ToJson(_adjSetting, true);
            VBTToolsSetting.SaveJsonToFile(json);
        }
        */
    }

    /// <summary>
    /// v0.2.0 以降用の設定保存クラス、旧型との互換確保処理を含む
    /// </summary>
    [System.Serializable]
    public class VBTToolsSetting
    {
        readonly string _currentVersion = "0.2.0";
        [SerializeField] string _version = "0.0.0";

        public VBTToolsAdjustSetting _adjSetting;

        public string Version => _version;

        void SetVersion( int a, int b, int c ) {
            _version = $"{a}.{b}.{c}";
        }

        /// <summary>
        /// Jsonファイルを読み込み文字列で返す(型は特定しないので他クラスも使える)
        /// </summary>
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


        /// <summary>
         /// ファイル選択ダイアログを出して、json文字列をファイルに保存する(型は特定しないので他クラスも使える)
        /// </summary>
        static public void SaveJsonToFile(string json)
        {
            var extensions = new[] {
                new ExtensionFilter("Json Files", "json" ),
            };

            var path = StandaloneFileBrowser.SaveFilePanel("Save File", "", "default.json", extensions);

            if (path.Length > 0)
            {
                StreamWriter sw = new StreamWriter(path,false); 
                sw.Write(json);
                sw.Flush();
                sw.Close();
            }
        }

        /// <summary>
         /// ファイル選択ダイアログを出して、json文字列にした設定内容をファイルに保存する
        /// </summary>
        static public void SaveToFile(VBTToolsSetting _setting)
        {
            var json = JsonUtility.ToJson(_setting, true);
            SaveJsonToFile(json);
        }

        /// <summary>
        /// 指定されたパスのファイルを読み込む。旧形式(0.1.0以前)のものは変換して読み込む
        /// </summary>
        public static VBTToolsSetting LoadFromFile(string path)
        {
            string json = LoadJsonFromFile(path);
            var obj1 = JsonUtility.FromJson<VBTToolsAdjustSetting>(json);
            var obj2 = JsonUtility.FromJson<VBTToolsSetting>(json);
            Debug.Log($"obj2.version : {obj2.Version}");
            if ( obj2.Version == "0.0.0") { // old style setting file, just including _adjustSetting
                obj2._adjSetting = obj1;
                obj2.SetVersion(0,2,0);
            }
            
            return obj2;
        }

        /// <summary>
        /// ファイル選択ダイアログを出してからロードする(nullは返さない、必ずインスタンスを返す)
        /// </summary>
        public static VBTToolsSetting LoadFromFile() // no path specified
        {
            VBTToolsSetting ret;
            var extensions = new[] {
                new ExtensionFilter("Json Files", "json" ),
            };

            var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);
            if (paths.Length > 0 && paths[0].Length > 0)
            {
                ret = LoadFromFile(paths[0]);
            }
            else {
                ret = null;
            }
            return ret;
        }
    }
}
