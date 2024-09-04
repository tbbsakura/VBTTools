//
// This code is modified version of FileDragAndDrop.cs by Markus Göbel (Bunny83)
// The modified code has additional functions mainly to load VRM file.

/*  
The MIT License (MIT)

Original Code: Copyright (c) 2018 Markus Göbel (Bunny83)
Modified Code: Copyright (c) 2023-2024 Sakura(さくら) / tbbsakura

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using B83.Win32;

using System.IO;
using VRM;
using UniGLTF;
using SakuraScript.VBTTool;

public class VBTTools_FileDragAndDrop : MonoBehaviour
{
    private bool m_loadAsync = true;
    private bool m_loading = false;
    private Text m_topText;

    private string m_lastLoadedFile;
    public string LastLoadedFile => m_lastLoadedFile;

    [SerializeField] EVMC4U.ExternalReceiver m_exrec;
    RuntimeGltfInstance _lastLoaded = null;
    [SerializeField]  VBTToolsSample m_sampleProject;

    void OnEnable ()
    {
        if ( m_exrec == null ) { 
            Debug.LogError( "EVMC4U.ExternalReceiver not speficied" );
            return;
        }
        if ( m_sampleProject == null ) { 
            Debug.LogError( "VBTToolsSample not speficied" );
            return;
        }

        if ( _lastLoaded != null ) m_exrec.Model = _lastLoaded.Root;

        // must be installed on the main thread to get the right thread id.
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;
        m_topText = GameObject.Find("TopText").GetComponent<Text>();
    }

    void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    public void OpenVRM(string path)
    {
        if ( m_loading ) return; // ignore
        LoadModel(path);
    }

    void OnFiles(List<string> aFiles, POINT aPos)
    {
        if ( m_loading ) return; // ignore
        LoadModel(aFiles[0].ToString()); // load only 1st file
    }
    
    void OnLoaded(RuntimeGltfInstance loaded, string path)
    {
        if ( loaded == null || loaded.Root == null )  return;
        if ( _lastLoaded != null ) Destroy(_lastLoaded.Root);
        _lastLoaded = loaded;
        loaded.Root.transform.SetParent(transform, false);// このcomponentがattachされているオブジェクトを親に設定する
        loaded.Root.transform.position.Set(-1,1,3); // 位置調整
        m_topText.text = "Showing Meshes...";
        foreach (var spring in loaded.Root.GetComponentsInChildren<VRMSpringBone>())
        {
            spring.Setup();
            spring.gameObject.SetActive(false);
        }

        loaded.ShowMeshes(); // メッシュ表示
        var lah = loaded.GetComponent<VRMLookAtHead>();
        if (lah !=null)
        {
            lah.UpdateType = UpdateType.LateUpdate;
        }

        UniHumanoid.HumanPoseTransfer target = loaded.Root.AddComponent<UniHumanoid.HumanPoseTransfer>();
        if ( target != null ) 
        {
            Animator animator = target.GetComponent<Animator>();
            if (animator != null)
            {
                if ( m_exrec != null && target != null ) m_exrec.Model = loaded.Root;
                m_lastLoadedFile = path; // load と show が成功してから更新する
                m_sampleProject?.OnVRMLoaded(animator);
            }
        }
        if (m_topText != null) m_topText.text = "VRM loaded";
    }

    private async void LoadModel(string path)
    {
        var ext = Path.GetExtension(path).ToLower();
        if ( ext == ".vrm" ) 
        {
            if ( m_topText != null ) m_topText.text = "Loading VRM...";
            using ( var data = new GlbFileParser(path).Parse() ) 
            {
                var vrm = new VRMData(data);
                using ( var context = new VRMImporterContext(vrm) ) 
                {
                    var loaded = default(RuntimeGltfInstance);
                    m_loading = true;
                    if (m_loadAsync)
                    {
                        try {
                            loaded = await context.LoadAsync(new VRMShaders.RuntimeOnlyAwaitCaller());
                        } catch(System.Exception e) {
                            System.Type exceptionType = e.GetType();
                            if (m_topText != null) m_topText.text = "Load failed. : " + e.Message;
                            m_loading = false;
                            return;
                        }
                    }
                    else
                    {
                        try {
                            loaded = context.Load();
                        } catch(System.AggregateException ae1) { // 2層Aggregateが帰ってきたりするので
                            foreach ( var eInner in ae1.InnerExceptions ) 
                            {
                                if (eInner.GetType() == typeof(System.AggregateException) ) 
                                {
                                    System.AggregateException ae2 = (System.AggregateException)eInner;
                                    foreach ( var eInner2 in ae2.InnerExceptions ) 
                                    {
                                        if (m_topText != null) m_topText.text = "Load failed. : " + eInner2.Message;
                                        m_loading = false;
                                        return;
                                    }
                                }
                                else {
                                    if (m_topText != null) m_topText.text = "Load failed. : " + eInner.Message;
                                    m_loading = false;
                                    return;
                                }
                            }
                            m_loading = false;
                            return;
                        } catch (System.Exception e) {
                            if (m_topText != null) m_topText.text = "Load failed. : " + e.Message;
                            m_loading = false;
                            return;
                        }
                    }
                    OnLoaded(loaded, path);
                }
            }
        }
        else {
            if (m_topText != null) m_topText.text = "Please open .vrm file.";
        }
        m_loading = false;
    }
}

