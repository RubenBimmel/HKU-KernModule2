using System.Runtime.Serialization;
using UnityEngine;
using UnityEditor;

sealed class Vector3SerializationSurrogate : ISerializationSurrogate {

    // Method called to serialize a Vector3 object: https://forum.unity.com/threads/vector3-not-serializable.7766/
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context) {

        Vector3 v3 = (Vector3)obj;
        info.AddValue("x", v3.x);
        info.AddValue("y", v3.y);
        info.AddValue("z", v3.z);
        //Debug.Log(v3);
    }

    // Method called to deserialize a Vector3 object
    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector) {

        Vector3 v3 = (Vector3)obj;
        v3.x = (float)info.GetValue("x", typeof(float));
        v3.y = (float)info.GetValue("y", typeof(float));
        v3.z = (float)info.GetValue("z", typeof(float));
        obj = v3;
        return obj;   // Formatters ignore this return value //Seems to have been fixed!
    }
}

sealed class SplineSettingsSerializationSurrogate : ISerializationSurrogate {

    // Method called to serialize a Spline Settings object
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context) {

        SplineSettings settings = (SplineSettings)obj;
        info.AddValue("name", settings.name);
    }

    // Method called to deserialize a Spline Settings object
    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector) {

        SplineSettings settings = (SplineSettings)obj;
        string name = (string)info.GetString("name");
        string[] assets = AssetDatabase.FindAssets(string.Concat(name, " t:SplineSettings"));
        if (assets.Length > 0) {
            settings = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[0]), typeof(SplineSettings)) as SplineSettings;
            obj = settings;
        }
        return obj;   // Formatters ignore this return value //Seems to have been fixed!
    }
}