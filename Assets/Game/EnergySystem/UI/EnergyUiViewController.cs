using System;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public class EnergyUiViewController : IDisposable
    {
        public struct UpdateProtocol
        {
            public float Charge;
            public float MaxCharge;
            public float Hp;
            public float MaxHp;
            public bool IsRepairing;
            public bool ReceivedDamageAtThisFrame;
        }

        private readonly EnergyCellUiView _view;

        public EnergyUiViewController(EnergyCellUiView view)
        {
            _view = view;
        }

        public void Update(UpdateProtocol protocol)
        {
            var hpPc = protocol.Hp / protocol.MaxHp;
            if (protocol.IsRepairing)
            {
                _view.UpdateValues(new()
                {
                    EnableDamageLine = false,
                    EnableRepairMarker = true,
                    EnergyLineColor = _view.RepairColor,
                    EnergyLineFillPc = hpPc,
                    HasReceivedDamageOnThisFrame = protocol.ReceivedDamageAtThisFrame
                });
            }
            else
            {
                _view.UpdateValues(new()
                {
                    EnableDamageLine = true,
                    EnergyLineColor = _view.EnergyColor,
                    EnergyLineFillPc = protocol.Charge / protocol.MaxCharge,
                    DamageLineFillPc = math.clamp(1f - hpPc, 0f, 1f),
                    HasReceivedDamageOnThisFrame = protocol.ReceivedDamageAtThisFrame
                });
            }
        }

        public void Dispose() => _view.Dispose();

    }
}
