using UnityEngine;

/// <summary>
/// Project-local data foundation.
/// Replaces the small BaseData dependency previously supplied by UCExtension.
/// </summary>
public class BaseData : ScriptableObject
{
    public string ID;
    public string Name;
    public string Description;
    public int SortPriority;
    public Sprite Avatar;

    public bool HasSameID(BaseData data)
    {
        return data != null && ID == data.ID;
    }

    public bool HasSameID(string compareID)
    {
        return ID == compareID;
    }
}
