using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class FileReadWrite : MonoBehaviour
{
    [SerializeField]
    string _fileName;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SaveAnchorGameObjectDataToPlayerPrefs(Dictionary<System.Guid, int> anchorGameObjectData)
    {
        string wayspotAnchorsJson = JsonUtility.ToJson(anchorGameObjectData);
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream stream = new MemoryStream();
        formatter.Serialize(stream, anchorGameObjectData);
        PlayerPrefs.SetString("AnchorGameObjectData", Convert.ToBase64String( stream.ToArray()));
    }
    public void SaveAnchorRotationsToPlayerPrefs(Dictionary<System.Guid, SerializableTypes.SQuaternion> anchorRotationData)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream stream = new MemoryStream();
        formatter.Serialize(stream, anchorRotationData);
        PlayerPrefs.SetString("AnchorRotationData", Convert.ToBase64String(stream.ToArray()));
    }

    public void SaveAnchorGameObjectData(Dictionary<System.Guid, int> anchorGameObjectData)
    {
        using (Stream stream = File.OpenWrite(Application.streamingAssetsPath + Path.DirectorySeparatorChar + _fileName))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream,anchorGameObjectData);
            stream.Close();
        }
    }
    public void LoadAnchorGameObjectData(out Dictionary<System.Guid, int> anchorGameObjectData)
    {
        anchorGameObjectData = new Dictionary<System.Guid, int>();
        if (File.Exists(Application.streamingAssetsPath + Path.DirectorySeparatorChar + _fileName))
        {            
                using (Stream stream = File.OpenRead(Application.streamingAssetsPath + Path.DirectorySeparatorChar + _fileName))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    anchorGameObjectData = formatter.Deserialize(stream) as Dictionary<System.Guid, int>;
                    stream.Close();
                }            
        }
    }
    public void LoadAnchorGameObjectDataFromPlayerPrefs(out Dictionary<System.Guid, int> anchorRotationData)
    {
        string wayspotAnchorsJson;
        if(PlayerPrefs.HasKey("AnchorGameObjectData"))
        {
            wayspotAnchorsJson = PlayerPrefs.GetString("AnchorGameObjectData");
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(Convert.FromBase64String(wayspotAnchorsJson));
            //anchorGameObjectData = JsonUtility.FromJson<Dictionary<System.Guid, int>>(wayspotAnchorsJson);
            anchorRotationData = (Dictionary<System.Guid, int>)formatter.Deserialize(stream);
        }
        else
        {
            anchorRotationData = new Dictionary<Guid, int>();
        }
    }
    public void LoadAnchorRotationDataFromPlayerPrefs(out Dictionary<System.Guid, SerializableTypes.SQuaternion> anchorGameObjectData)
    {
        string wayspotAnchorsJson;
        if (PlayerPrefs.HasKey("AnchorRotationData"))
        {
            wayspotAnchorsJson = PlayerPrefs.GetString("AnchorRotationData");
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(Convert.FromBase64String(wayspotAnchorsJson));
            //anchorGameObjectData = JsonUtility.FromJson<Dictionary<System.Guid, int>>(wayspotAnchorsJson);
            anchorGameObjectData = (Dictionary<System.Guid, SerializableTypes.SQuaternion>)formatter.Deserialize(stream);
        }
        else
        {
            anchorGameObjectData = new Dictionary<Guid, SerializableTypes.SQuaternion>();
        }
    }
}
