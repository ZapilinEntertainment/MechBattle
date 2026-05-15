namespace ZE.MechBattle.Navigation
{
    public unsafe struct CombinedExitDistances
    {
        private fixed ushort _exitDistances[6];

        public CombinedExitDistances(in CombinedFlowData flowData)
        {
            for (var i = 0; i < 6; i++)
            {
                _exitDistances[i] = (ushort)flowData[(HexEdge)i].ExitDistance;
            }
        }
        
        public int this[HexEdge edge] => _exitDistances[(int)edge];
    }
}
