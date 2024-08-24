using System.Collections;
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

    public EVMC4U.ExternalReceiver m_exrec;
    private UniHumanoid.HumanPoseTransfer m_target = null; // ロードされたVRMモデルのHumanPoseTransfer
    public VBTToolsSample m_sampleProject;

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

        if ( m_target != null ) m_exrec.Model = m_target.gameObject;

        // must be installed on the main thread to get the right thread id.
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;
        m_topText = GameObject.Find("TopText").GetComponent<Text>();

        if ( m_sampleProject != null && m_sampleProject._animationTarget == null ) {
#if UNITY_EDITOR
            const char separatorChar = '/';
            string modelFilepath = "Assets/SakuraShop_tbb/VRM_CC0/HairSample_Male.vrm"; //CC0 model
            modelFilepath = modelFilepath.Replace( separatorChar, System.IO.Path.DirectorySeparatorChar );
            //modelFilepath = "Z:\\VR\\_VRM\\fumifumi\\3c6.0_noshoe_.vrm";
#else
            string modelFilepath = "HairSample_Male.vrm"; //CC0 model
#endif
            LoadModel(modelFilepath);
        }
    }
    void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    void OnFiles(List<string> aFiles, POINT aPos)
    {
        if ( m_loading ) return; // ignore
        LoadModel(aFiles[0].ToString()); // load only 1st file
    }
    
    void OnLoaded(RuntimeGltfInstance loaded)
    {
        var root = loaded.gameObject;

        root.transform.SetParent(transform, false);// このcomponentがattachされているオブジェクトを親に設定する
        root.transform.position.Set(-1,1,3); // 位置調整
        m_topText.text = "Showing Meshes...";
        foreach (var spring in root.GetComponentsInChildren<VRMSpringBone>())
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

        m_target = root.AddComponent<UniHumanoid.HumanPoseTransfer>();
        if ( m_target != null ) 
        {
            
            Animator animator = m_target.GetComponent<Animator>();
            if (animator != null)
            {
                if ( m_exrec is not null && m_target is not null && m_target.gameObject is not null ) m_exrec.Model = m_target.gameObject;

                if ( m_sampleProject is not null ) {
                    m_sampleProject.OnVRMLoaded(animator);
                }
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
            if (m_target != null) // unload
            {
                GameObject.Destroy(m_target.gameObject);
                m_target = null;
            }
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
                    OnLoaded(loaded);
                }
            }
        }
        else {
            if (m_topText != null) m_topText.text = "Dropped is not .vrm file.";
        }
        m_loading = false;
    }
}

