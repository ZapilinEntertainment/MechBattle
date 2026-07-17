using UnityEngine;
using System.Collections.Generic;
using VContainer;

namespace ZE.MechBattle
{
    public enum ColouredMaterialType : byte { FactionColour}

    public class ColouredMaterialsDepot
    {

        private readonly Dictionary<(ColouredMaterialType materialType, Color color), Material> _materialsList = new();

        [Inject]
        public ColouredMaterialsDepot()
        {
            var colouredMaterial = Resources.Load<Material>("Materials/coloured_material");
            _materialsList.Add((ColouredMaterialType.FactionColour, Color.white), colouredMaterial);
        }

        public Material GetColouredMaterial(ColouredMaterialType type, Color color)
        {
            if (_materialsList.TryGetValue((type, color), out var material))
                return material;

            var original = _materialsList[(type, Color.white)];
            var colouredCopy = GameObject.Instantiate(original);
            colouredCopy.color = color;
            _materialsList.Add((type,color), colouredCopy);
            return colouredCopy;
        }
    
    }
}
