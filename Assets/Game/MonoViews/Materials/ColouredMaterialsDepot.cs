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
            var idKey = _materialsDepot.GetMaterialIdKey(AssetConstants.FACTION_COLOURING_MATERIAL);
            _materialsList.Add((ColouredMaterialType.FactionColour, Color.white), idKey);
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }

        public Material GetColouredMaterial(ColouredMaterialType type, Color color)
        {
            if (_materialsList.TryGetValue((type, color), out var materialId))
                return _materialsDepot.GetMaterialById(materialId);

            var original = _materialsDepot.GetMaterialById( _materialsList[(type, Color.white)]);
            var colouredCopy = GameObject.Instantiate(original);
            colouredCopy.color = color;
            _materialsDepot
                .RegisterTemporalMaterial(colouredCopy, out var colouredIdKey)
                .AddTo(_compositeDisposable);
            _materialsList.Add((type,color), colouredIdKey);
            return colouredCopy;
        }
    
    }
}
