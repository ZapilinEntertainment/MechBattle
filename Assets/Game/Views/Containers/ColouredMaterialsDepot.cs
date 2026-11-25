using UnityEngine;
using System.Collections.Generic;
using VContainer;
using System;
using R3;

namespace ZE.MechBattle
{
    public enum ColouredMaterialType : byte { FactionColour}

    public class ColouredMaterialsDepot : IDisposable
    {
        private readonly MaterialsDepot _materialsDepot;
        private readonly CompositeDisposable _compositeDisposable = new();
        private readonly Dictionary<(ColouredMaterialType materialType, Color color), int> _materialsList = new();

        [Inject]
        public ColouredMaterialsDepot(MaterialsDepot depot)
        {
            _materialsDepot = depot;
            _materialsDepot.TryGetElementIndex(ViewConstants.FACTION_COLOURING_MATERIAL, out var idKey);
            _materialsList.Add((ColouredMaterialType.FactionColour, Color.white), idKey);
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }

        public Material GetColouredMaterial(ColouredMaterialType type, Color color)
        {
            if (_materialsList.TryGetValue((type, color), out var materialId))
                return _materialsDepot[materialId];

            CreateNewColouredMaterial(type, color, out _, out var colouredMaterial);
            return colouredMaterial;
        }

        public int GetColouredMaterialId(ColouredMaterialType type, Color color)
        {
            if (_materialsList.TryGetValue((type, color), out var materialId))
                return materialId;

            CreateNewColouredMaterial(type, color, out var colouredMaterialId, out _);
            return colouredMaterialId;
        }

        private void CreateNewColouredMaterial(ColouredMaterialType type, Color color, out int colouredIdKey, out Material colouredMaterial)
        {
            var original = _materialsDepot[_materialsList[(type, Color.white)]];
            colouredMaterial = GameObject.Instantiate(original);
            colouredMaterial.color = color;

            _materialsDepot
               .RegisterTemporalMaterial(colouredMaterial, out colouredIdKey)
               .AddTo(_compositeDisposable);
            _materialsList.Add((type, color), colouredIdKey);
        }
    
    }
}
