using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

[CreateAssetMenu(
        fileName = "FeaturesModulesList",
        menuName = "Bootstrap/Features Modules List")]
public sealed class FeaturesModulesList : ScriptableObject
{
    [SerializeReference]
    [ValidateInput(nameof(ValidateNoDuplicateReferences))]
    private List<IFeatureModule> _modules = new();
    //---
    public IReadOnlyList<IFeatureModule> Modules => _modules;

    private TriValidationResult ValidateNoDuplicateReferences()
    {
        var uniqueTypes = new HashSet<Type>();

        foreach (var item in _modules)
        {
            if (item == null) continue;
            var type = item.GetType();

            if (uniqueTypes.Contains(type))
            {
                return TriValidationResult.Error($"Duplicated {type.Name}");
            }
            
            uniqueTypes.Add(type);
        }

        return TriValidationResult.Valid;
    }
}

