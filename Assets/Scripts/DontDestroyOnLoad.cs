using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-10000)]
public sealed class DontDestroyOnLoad : MonoBehaviour
{
    private static readonly Dictionary<Type, Component> disallowMultipleInstancesOfTypes = new Dictionary<Type, Component>();
 
    [SerializeField]
    private Component disallowMultipleInstancesOf;
 
    private void Awake()
    {
        if(transform.parent != null)
        {
            Debug.LogError("DontDestroyOnLoad can only be attached to GameObjects in the root of the scene hierarchy.", this);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }

        if(disallowMultipleInstancesOf != null)
        {
            var type = disallowMultipleInstancesOf.GetType();
            if(disallowMultipleInstancesOfTypes.TryGetValue(type, out var firstInstance) && firstInstance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                disallowMultipleInstancesOfTypes[type] = disallowMultipleInstancesOf;
            }
        }
    }

    private void Reset() => disallowMultipleInstancesOf = GetComponents<Component>().Where(c => c != transform && c != this).FirstOrDefault();

    private void OnValidate()
    {
        if(transform.parent != null)
        {
            Debug.LogError("DontDestroyOnLoad can only be attached to GameObjects in the root of the scene hierarchy.", this);
        }
    }
}