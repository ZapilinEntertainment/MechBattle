using Scellecs.Morpeh;
using ZE.UiService;
using ZE.Workers;

namespace ZE.MechBattle
{
    public class UIMechPartitionsViewWorker : Worker
    {
        private PartitionsListManager _partitionsManager;

        public UIMechPartitionsViewWorker(PartitionsListManager partitionsManager)
        {
            _partitionsManager = partitionsManager;
        }

        public void Start(UIPartitionsWindow partitionsWindow, Entity mechEntity)
        {
            var mechPartitions = _partitionsManager.GetPartitionsList(mechEntity);
            foreach (var kvp in partitionsWindow.Partitions)
            {
                if (mechPartitions.TryGet(kvp.Key, out var partitionEntity))
                {
                    // todo: define which mode it is on
                    kvp.Value.gameObject.SetActive(true);
                }
                else
                {
                    kvp.Value.gameObject.SetActive(false);
                }
            }
        }

    }
}
