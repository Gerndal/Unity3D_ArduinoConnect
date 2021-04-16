using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;

public class DebugCodeLocation
{
    private const string logMatchKey = "<size=";

    [UnityEditor.Callbacks.OnOpenAsset]
    private static bool OnOpenAsset(int instanceID, int line)
    {
        bool retValue    = false;
        var  stack_trace = GetStackTrace();
        if (string.IsNullOrEmpty(stack_trace)) return retValue;
        
        var  instanceIDName = UnityEditor.EditorUtility.InstanceIDToObject(instanceID).name;
        var  matchStringID  = stack_trace.Substring(0, logMatchKey.Length);
        if (instanceIDName.Equals("Extension") &&
            matchStringID.Equals(logMatchKey))
        {
            var matches  = Regex.Match(stack_trace, @"\(at(.+)\)", RegexOptions.IgnoreCase);
            if (matches.Success == false) return retValue;

                matches = matches.NextMatch();  // Raise another layer up to enter;
            if (matches.Success == false) return retValue;

            var pathline    = matches.Groups[1].Value.Replace(" ", "");
            var split_index = pathline.LastIndexOf(":");
            var fullpath    = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));
            var strPath     = (fullpath + pathline.Substring(0, split_index)).Replace('/', '\\');
                line        = Convert.ToInt32(pathline.Substring(split_index + 1));
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(strPath, line);
            retValue = true;
        }

        return retValue;
    }

    private static string GetStackTrace()
    {
        // Find the assembly of UnityEditor.EditorWindow
        var assembly_unity_editor = Assembly.GetAssembly(typeof(UnityEditor.EditorWindow));
        if (assembly_unity_editor == null) return null;

        // Find the class UnityEditor.ConsoleWindow
        var type_console_window = assembly_unity_editor.GetType("UnityEditor.ConsoleWindow");
        if (type_console_window == null) return null;
        // Find the member ms_ConsoleWindow in UnityEditor.ConsoleWindow
        var field_console_window = type_console_window.GetField("ms_ConsoleWindow", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        if (field_console_window == null) return null;
        // Get the value of ms_ConsoleWindow
        var instance_console_window = field_console_window.GetValue(null);
        if (instance_console_window == null) return null;

        // If the focus window of the console window, get the stacktrace
        if ((object)UnityEditor.EditorWindow.focusedWindow == instance_console_window)
        {
            // Get the class ListViewState through the assembly
            var type_list_view_state = assembly_unity_editor.GetType("UnityEditor.ListViewState");
            if (type_list_view_state == null) return null;

            // Find the member m_ListView in the class UnityEditor.ConsoleWindow
            var field_list_view = type_console_window.GetField("m_ListView", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field_list_view == null) return null;

            // Get the value of m_ListView
            var value_list_view = field_list_view.GetValue(instance_console_window);
            if (value_list_view == null) return null;

            // Find the member m_ActiveText in the class UnityEditor.ConsoleWindow
            var field_active_text = type_console_window.GetField("m_ActiveText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field_active_text == null) return null;

            // Get the value of m_ActiveText, is the stacktrace we need
            string value_active_text = field_active_text.GetValue(instance_console_window).ToString();
            return value_active_text;
        }

        return null;
    }
}
#endif

#if UNITY_EDITOR
public static partial class _
{
    [MenuItem ("Tools/GUID Regen", false, 0)]
    public static void GuidRegen()
    {
        PlayerPrefs.DeleteKey(GuidKeyName);
        GuidView();
    }

    [MenuItem ("Tools/GUID View", false, 0)]
    public static void GuidView() => GuidKeyName.GetGuidString().ToUpper().Log();

    [MenuItem("Tools/Remove Missing Scripts")] // Recursively Visit Prefabs
    private static void FindAndRemoveMissingInSelected()
    {
        // EditorUtility.CollectDeepHierarchy does not include inactive children
        var deeperSelection = Selection.gameObjects
                                .SelectMany(go => go.GetComponentsInChildren<Transform>(true))
                                .Select(t => t.gameObject);
        var prefabs   = new HashSet<UnityEngine.Object>();
        int compCount = 0;
        int goCount   = 0;
        foreach (var go in deeperSelection)
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (count > 0)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(go))
                {
                    RecursivePrefabSource(go, prefabs, ref compCount, ref goCount);
                    count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                    // if count == 0 the missing scripts has been removed from prefabs
                    if (count == 0)
                        continue;
                    // if not the missing scripts must be prefab overrides on this instance
                }

                Undo.RegisterCompleteObjectUndo(go, "Remove missing scripts");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                compCount += count;
                goCount++;
            }
        }

        ($"Found and removed {compCount} missing scripts from {goCount} GameObjects").Log();
    }

    private static void RecursivePrefabSource(GameObject instance, HashSet<UnityEngine.Object> prefabs, ref int compCount, ref int goCount)
    {
        var source = PrefabUtility.GetCorrespondingObjectFromSource(instance);
        // Only visit if source is valid, and hasn't been visited before
        if (source == null || !prefabs.Add(source))
            return;

        // go deep before removing, to differantiate local overrides from missing in source
        RecursivePrefabSource(source, prefabs, ref compCount, ref goCount);

        int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(source);
        if (count > 0)
        {
            Undo.RegisterCompleteObjectUndo(source, "Remove missing scripts");
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(source);
            compCount += count;
            goCount++;
        }
    }

    [MenuItem("Tools/Set Button Sound Script")]
    private static void SetSelectedInButtonSound()
    {
        var deeperSelection = Selection.gameObjects
                                .SelectMany(go => go.GetComponentsInChildren<Button>(true))
                                .Select(t => t.gameObject);
        var prefabs   = new HashSet<GameObject>();
        int compCount = 0;
        int goCount   = 0;
        foreach (var go in deeperSelection)
        {
            var btns = go.GetComponentsInChildren<Button>();
            if (btns.Length > 0)
            {
                btns.ToList().ForEach(b => prefabs.Add(b.gameObject));
                compCount += btns.Length;
                goCount++;
            }
        }

        prefabs.ToList().ForEach(b =>
        {
            //b.EnsureComponent<ButtonSound>();
            b.transform.GetPath().Log();
        });

        ($"Found {compCount} Button scripts from {goCount} GameObjects").Log();
    }

    // Sample
    // [MenuItem("Tools/Find Button Scripts")]
    // private static void FindButtonInSelected()
    // {
    //     var deeperSelection = Selection.gameObjects
    //                             .SelectMany(go => go.GetComponentsInChildren<Button>(true))
    //                             .Select(t => t.gameObject);
    //     var prefabs   = new HashSet<Object>();
    //     int compCount = 0;
    //     int goCount   = 0;
    //     foreach (var go in deeperSelection)
    //     {
    //         var btns = go.GetComponentsInChildren<Button>();
    //         if (btns.Length > 0)
    //         {
    //             btns.ToList().ForEach(b => prefabs.Add(b));
    //             compCount += btns.Length;
    //             goCount++;
    //         }
    //     }

    //     var logStr = "";
    //     prefabs.ToList().ForEach(b => logStr += (b.name + " \n "));
    //     logStr.Log();

    //     ($"Found {compCount} Button scripts from {goCount} GameObjects").Log();
    // }
}
#endif

public static partial class _
{
    public  static string GuidKeyName = "GUID";
    public  static Guid   GetGuid      (this string _guidKeyName) => Guid.Parse(GetGuidString(_guidKeyName));
    public  static string GetGuidString(this string _guidKeyName)
    {
        var guidStr = PlayerPrefs.GetString(_guidKeyName, Guid.NewGuid().ToString());
        PlayerPrefs.SetString(_guidKeyName, guidStr);
        return guidStr;
    }

    public static string TextSize (this object _object, int _fontSize=13)             => $"<size={_fontSize}>{_object}</size>";
    public static string TextColor(this object _object, Color _clr)                   => $"<color=#{ColorUtility.ToHtmlStringRGBA(_clr)}>{_object}</color>";
    public static string Text     (this object _object, Color _clr, int _fontSize=13) => _object.TextColor(_clr).TextSize(_fontSize);

    private  static string StackTransValue(int _fontSize)
    {
        var sf  = new StackTrace(true).GetFrame(2);
        var str =   sf.GetMethod().ReflectedType.Name.TextColor(Color.blue    * 0.8f) +
                    "."                              .TextColor(Color.white         ) +
                    sf.GetMethod().Name              .TextColor(Color.magenta * 0.8f) +
                    ":"                              .TextColor(Color.white         ) +
                    sf.GetFileLineNumber()           .TextColor(Color.green         );
            str = ("[" + str + "]").Text(Color.black, _fontSize);
        return str;
    }

    public  static object Log(this object _string, Color _clr=default, int _fontSize=14)
    {
        _clr = _clr==default ? Color.white : _clr;
        UnityEngine.Debug.Log(StackTransValue(_fontSize) + Text(_string, _clr, _fontSize));
        return _string;
    }

    public  static object WarningLog(this object _string, Color _clr=default, int _fontSize=14)
    {
        _clr = _clr==default ? Color.yellow : _clr;
        UnityEngine.Debug.LogWarning(StackTransValue(_fontSize) + Text(_string, _clr, _fontSize));
        return _string;
    }

    public  static object ErrorLog(this object _string, Color _clr=default, int _fontSize=16)
    {
        _clr = _clr==default ? Color.red : _clr;
        UnityEngine.Debug.LogError(StackTransValue(_fontSize) + Text(_string, _clr, _fontSize));
        return _string;
    }

    public  static string PkStr(this byte[] _bytes)
    {
        return System.Text.Encoding.UTF8.GetString(_bytes).Split('\0')[0];
    }

    public  static Color A(this Color _clr, float _value) {_clr.a = _value; return _clr;}

    public  static Vector2 X   (this Vector2 _v2Pos, float _value) {_v2Pos.x = _value; return _v2Pos;}
    public  static Vector2 Y   (this Vector2 _v2Pos, float _value) {_v2Pos.y = _value; return _v2Pos;}

    public  static Vector3 X   (this Vector3 _v3Pos, float _value) {_v3Pos.x = _value; return _v3Pos;}
    public  static Vector3 Y   (this Vector3 _v3Pos, float _value) {_v3Pos.y = _value; return _v3Pos;}
    public  static Vector3 Z   (this Vector3 _v3Pos, float _value) {_v3Pos.z = _value; return _v3Pos;}
    public  static Vector3 AddX(this Vector3 _v3Pos, float _value) {_v3Pos.x+= _value; return _v3Pos;}
    public  static Vector3 AddY(this Vector3 _v3Pos, float _value) {_v3Pos.y+= _value; return _v3Pos;}
    public  static Vector3 AddZ(this Vector3 _v3Pos, float _value) {_v3Pos.z+= _value; return _v3Pos;}
    
    //public  static TEnum ToEnum<TEnum>(this string _str ) where TEnum : Enum => (TEnum)Convert.ChangeType(_str, typeof(TEnum));
    public  static TEnum ToEnum<TEnum>(this string _str ) where TEnum : Enum => (TEnum)Enum.Parse(typeof(TEnum), _str, true);
    public  static int      Int<TEnum>(this TEnum _value) where TEnum : Enum => (int)(System.ValueType)_value;
    public  static int      Int       (this float _value)    => Mathf.CeilToInt(_value);
    public  static float    Float(this string strFloat, float _defaultValue)
    {
        var retValue = 0.0f;
        if (float.TryParse(strFloat, out retValue) == false) retValue = _defaultValue;
        return retValue;
    }

    public  static string IsNullOrEmpty(this string _str, string _value) => string.IsNullOrEmpty(_str) ? _value : _str;
    public  static string InputValue(this TMP_InputField _InputField) => _InputField.text.IsNullOrEmpty(_InputField.placeholder.GetComponent<TMP_Text>().text);

    // PlayerPrefs IO
    public  static void Save(this Vector3 _value, string _name)
    {
        PlayerPrefs.SetFloat(_name+"X", _value.x);
        PlayerPrefs.SetFloat(_name+"Y", _value.y);
        PlayerPrefs.SetFloat(_name+"Z", _value.z);
    }

    public  static Vector3 Load(this string _name)
    {
        var ret   = Vector3.zero;
            ret.x = PlayerPrefs.GetFloat(_name+"X", 0);
            ret.y = PlayerPrefs.GetFloat(_name+"Y", 0);
            ret.z = PlayerPrefs.GetFloat(_name+"Z", 0);
        return ret;
    }
    // PlayerPrefs IO

    internal class INI
    {
        [DllImport("kernel32", CharSet = CharSet.Auto)]  public static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);
        [DllImport("kernel32", CharSet = CharSet.Auto)]  public static extern int  GetPrivateProfileString  (string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);
    }

    public static string ReadIni(this string filePath, string section, string key)
    {
        var value = new StringBuilder(255);
        INI.GetPrivateProfileString(section, key, "Error", value, 255, Application.persistentDataPath + "/" + filePath + ".ini");
        return value.ToString();
    }

    public static void WriteIni(this string filePath, string section, string key, string value)
    {
        //(Application.persistentDataPath + "/" + filePath).Log();
        INI.WritePrivateProfileString(section, key, value, Application.persistentDataPath + "/" + filePath + ".ini");
    }

    internal class WinFileDialog
    {
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class OpenFileName
        {
            public int    structSize    = 0;
            public IntPtr dlgOwner      = IntPtr.Zero;
            public IntPtr instance      = IntPtr.Zero;
            public String filter        = null;
            public String customFilter  = null;
            public int    maxCustFilter = 40;
            public int    filterIndex   = 0;
            public String file          = null;
            public int    maxFile       = 0;
            public String fileTitle     = null;
            public int    maxFileTitle  = 0;
            public String initialDir    = null;
            public String title         = null;
            public int    flags         = 0;
            public short  fileOffset    = 0;
            public short  fileExtension = 0;
            public String defExt        = "ptd";
            public IntPtr custData      = IntPtr.Zero;
            public IntPtr hook          = IntPtr.Zero;
            public String templateName  = null;
            public IntPtr reservedPtr   = IntPtr.Zero;
            public int    reservedInt   = 0;
            public int    flagsEx       = 0;
        }

        public static OpenFileName NewOpenFileName(string filter="PathData (*.ptd)\0*.ptd\0All (*.*)\0*.*\0", string defExt="ptd", string _title="File Browser")
        {
            var openFileName              = new OpenFileName();
                openFileName.structSize   = Marshal.SizeOf(openFileName);
                openFileName.filter       = filter;
                openFileName.file         = new string(new char[1024]);
                openFileName.maxFile      = openFileName.file.Length;
                openFileName.fileTitle    = new string(new char[64]);
                openFileName.maxFileTitle = openFileName.fileTitle.Length;
                openFileName.title        = _title;
                openFileName.defExt       = defExt;
                openFileName.flags        = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;  
                                        //OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_ALLOWMULTISELECT | OFN_NOCHANGEDIR    
                //openFileName.initialDir = "C:\\";
            return openFileName;
        }
    }

    /// File IO
    public  static string PathDataFileOpen(this string _fileName, string filter, string defExt)
    {
        var ret          = "";
        var openFileName = WinFileDialog.NewOpenFileName(filter, defExt, Application.productName);

        if (WinFileDialog.GetOpenFileName(openFileName))
            ret = openFileName.file;

        return ret;
    }

    public  static string PathDataFileSave(this string _fileName, string filter, string defExt)
    {
        var ret                = "";
        var openFileName       = WinFileDialog.NewOpenFileName(filter, defExt, Application.productName);
            openFileName.file  = _fileName;
            openFileName.flags|= 0x00000002;

        if (WinFileDialog.GetSaveFileName(openFileName))
            ret = openFileName.file;

        return ret;
    }

    public static string GetProjectName()
    {
        var s = Application.dataPath.Split('/');
        return s[s.Length - 2];
    }
    
    public static void Save(this string Value , string ID)
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+"\\"+ID;
        //DirectoryInfo info = Directory.CreateDirectory (System.Environment.GetFolderPath (System.Environment.SpecialFolder.Desktop) + ID);
        File.WriteAllText(path, Value);        
        //(Application.dataPath + "/" + ID).Log();
        //File.WriteAllText(Application.dataPath + "/" + ID , Value);
    }

    public  static void   FileSave(this string _fullPath, string _value) => File.WriteAllText(_fullPath, _value);
    public  static string FileLoad(this string _fullPath)                => File.ReadAllText (_fullPath);
    /// File IO

    public static T EnsureComponent<T>(this GameObject _go) where T : Component
    {
        T comp = _go.GetComponent<T>();
        if (comp == null) comp = _go.AddComponent<T>();
        return comp;
    }
    public  static Transform  TransformFind    (this GameObject _go, string _name)                     => _go.transform.Find(_name);
    public  static T          TransformFind <T>(this GameObject _go, string _name) where T : Component => _go.TransformFind(_name).GetComponent<T>();
    
    public  static GameObject GameObjectFind      (this string _name)                               => GameObject.Find(_name);
    public  static T          GameObjectFind   <T>(this string _name) where T : Component           => GameObjectFind(_name)?.GetComponent<T>();
    public  static T          ResourcesLoad    <T>(this string _name) where T : UnityEngine.Object  => Resources.Load<T>(_name);
    public  static T          LangResourcesLoad<T>(this string _name, string _middleName) where T : UnityEngine.Object  => ResourcesLoad<T>("Language/" + _middleName + "Game.I.language" + _name);

    public  static void SetRecursivelyLayer(this MonoBehaviour _mb, int newLayer) => SetRecursivelyLayer(_mb.gameObject, newLayer);
    public  static void SetRecursivelyLayer(this GameObject _obj, int newLayer)
    {
        _obj.layer = newLayer;
        for (int i = 0; i < _obj.transform.childCount; ++i)
            SetRecursivelyLayer(_obj.transform.GetChild(i).gameObject, newLayer);
    }

    public  static void RecursivelyDestroyImmediate(this Transform _trans, int _startIdx)
    {
        while (_trans.childCount > _startIdx)
            GameObject.DestroyImmediate(_trans.GetChild(_startIdx).gameObject);
    }

    public  static string GetPath(this Transform current) 
    {
        if (current.parent == null) return "/" + current.name;
        return current.parent.GetPath() + "/" + current.name;
    }

    public static void Swap<T>(ref T lhs, ref T rhs) 
    {
        T temp = lhs;
          lhs  = rhs;
          rhs  = temp;
    }
}

[Serializable]
public class Serialization<T>
{
    [SerializeField] List<T> target;
    public List<T> ToList() { return target; }

    public Serialization(List<T> source)
    {
        this.target = source;
    }
}

public static class RandomWell512
{
    private static uint[] m_State = new uint[16];
    private static uint m_Index;

    static RandomWell512()
    {
        uint _Seed = (uint)DateTime.Now.Millisecond;
        uint _UniqueID = 73;
        for (int i = 0; i < 16; i++)
        {
            m_State[i] = _Seed;
            _Seed += _Seed + _UniqueID;
        }
    }

    public static uint Random()
    {
        uint a, b, c, d;

        a = m_State[m_Index];
        c = m_State[(m_Index + 13) & 15];
        b = a ^ c ^ (a << 16) ^ (c << 15);
        c = m_State[(m_Index + 9) & 15];
        c ^= (c >> 11);
        a = m_State[m_Index] = b ^ c;
        d = a ^ ((a << 5) & 0xda442d24U);
        m_Index = (m_Index + 15) & 15;
        a = m_State[m_Index];
        m_State[m_Index] = a ^ b ^ d ^ (a << 2) ^ (b << 18) ^ (c << 28);

        return m_State[m_Index];
    }

    public static int Range(int _min, int _max) => (int)((Random() % (_max - _min)) + _min);
}

public static class GameUniqueData
{
    private static int store_id;                                    // 매장 정보 ID( 매장이 변경될경우 id 변경 )
    private static string content_name;                              // 게임 고유 이름

    public static int Store_ID
    {
        get { return store_id; }
        set { store_id = value; }
    }

    public static string Content_Name
    {
        get { return content_name; }
        set { content_name = value; }
    }
}
