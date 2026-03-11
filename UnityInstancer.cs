using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UnityInstancer : MonoBehaviour
{
    public GameObject duplicateObject; // The prefab to duplicate
    public Transform[] ObjectsCollection;
    public Transform transformParentDups;

    // Adds the given transforms to the ObjectsCollection (keeps duplicates if present)
    public void AddObjects(IEnumerable<Transform> transforms)
    {
        var list = new List<Transform>();
        if (ObjectsCollection != null && ObjectsCollection.Length > 0)
            list.AddRange(ObjectsCollection);

        foreach (var t in transforms)
        {
            if (t == null) continue;
            // skip the host GameObject's transform
            if (t == this.transform) continue;
            list.Add(t);
        }

        ObjectsCollection = list.ToArray();
    }

    // Clears the collection
    public void ClearObjectsCollection()
    {
        ObjectsCollection = Array.Empty<Transform>();
    }

    // Instantiates a duplicate under each transform in the collection and resets it
    public List<GameObject> InstantiateDuplicates()
    {
        var created = new List<GameObject>();
        if (duplicateObject == null || ObjectsCollection == null || ObjectsCollection.Length == 0)
            return created;

        foreach (var parent in ObjectsCollection)
        {
            if (parent == null) continue;

            var instance = CreateDuplicate(parent);
            ResetLocalTransform(instance.transform);
            if (transformParentDups != null)
                instance.transform.SetParent(transformParentDups, true);
            created.Add(instance);
        }

        return created;
    }

    private GameObject CreateDuplicate(Transform parent)
    {
#if UNITY_EDITOR
        // In the editor, preserve prefab links (including model prefabs from FBX)
        if (PrefabUtility.IsPartOfPrefabAsset(duplicateObject))
        {
            var obj = PrefabUtility.InstantiatePrefab(duplicateObject, parent);
            return obj as GameObject;
        }

        // If a scene instance is assigned, try to resolve its source prefab asset
        if (PrefabUtility.IsPartOfPrefabInstance(duplicateObject))
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(duplicateObject);
            if (source != null)
            {
                var obj = PrefabUtility.InstantiatePrefab(source, parent);
                return obj as GameObject;
            }
        }
#endif
        return Instantiate(duplicateObject, parent);
    }

    private static void ResetLocalTransform(Transform t)
    {
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
    }
}
