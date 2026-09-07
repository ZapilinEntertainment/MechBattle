using Scellecs.Morpeh;
using ZE.Workers;
using R3;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class UIMechPartitionViewWorker : Worker
    {
        private Entity _partitionEntity;
        private UIPartitionView _partitionView;
        private UIEnergyCellViewWorker[] _cellControllers;

        private readonly Stash<RepairRequiredTag> _repairRequiredTags;
        private readonly Stash<RepairProcessComponent> _repairProcessComponents;
        private readonly Stash<HealthComponent> _healthComponents;
        private readonly Stash<EnergyCellsGridComponent> _cellsGrid;
        private readonly Stash<NextEnergyCellComponent> _nextCells;

        private readonly ReactiveProperty<UIPartitionView.PartitionViewMode> _partitionViewModeProperty;
        private readonly EnergyCellUiViewPool _cellsPool;
        private readonly World _world;
        

        public UIMechPartitionViewWorker(World world, EnergyCellUiViewPool cellViewPool)
        {
            _world = world;
            _cellsPool = cellViewPool;

            _repairRequiredTags = _world.GetStash<RepairRequiredTag>();
            _repairProcessComponents = _world.GetStash<RepairProcessComponent>();
            _healthComponents = _world.GetStash<HealthComponent>();
            _cellsGrid = _world.GetStash<EnergyCellsGridComponent>();
            _nextCells = _world.GetStash<NextEnergyCellComponent>();

            _partitionViewModeProperty = new ReactiveProperty<UIPartitionView.PartitionViewMode>(UIPartitionView.PartitionViewMode.HealthDisplay).AddTo(CompositeDisposable);
        }

        public void Start(UIPartitionView view, Entity partitionEntity)
        {
            _partitionEntity = partitionEntity;
            _partitionView = view;
            PrepareCellControllers(partitionEntity);
            
            _partitionViewModeProperty.Subscribe(_partitionView.SwitchMode).AddTo(CompositeDisposable);
            Observable.EveryUpdate().Subscribe(Update).AddTo(CompositeDisposable);
        }

        private void PrepareCellControllers(Entity partitionEntity)
        {
            var totalCellsCount = _cellsGrid.Get(partitionEntity).TotalCellsCount;
            _cellControllers = new UIEnergyCellViewWorker[totalCellsCount];
            var cellsHost = _partitionView.CellsHost;

            var firstCell = _cellsGrid.Get(partitionEntity).FirstCellEntity;
            var i = 0;
            foreach (var cellEntity in new EnergyCellsEnumerator(_nextCells, firstCell))
            {
                //UnityEngine.Debug.Log($"{i} / {totalCellsCount}: {cellEntity.Id}");
                var view = _cellsPool.Get();
                view.SetupParent(cellsHost);
                var cellController = AddSubWorker<UIEnergyCellViewWorker>();
                _cellControllers[i++] = cellController;
                cellController.Start(cellEntity, view);
            }
        }

        private void Update(Unit unit)
        {
            if (_world.IsDisposed)
                return;

            if (_repairRequiredTags.Has(_partitionEntity))
            {
                _partitionViewModeProperty.Value = _repairProcessComponents.Has(_partitionEntity) ? UIPartitionView.PartitionViewMode.Repairs : UIPartitionView.PartitionViewMode.HealthDisplay;
                _partitionView.SetHealthPc(_healthComponents.Get(_partitionEntity).HealthPercent);
            }                
            else
            {
                _partitionViewModeProperty.Value = UIPartitionView.PartitionViewMode.CellsCharging;
                foreach (var cellController in _cellControllers)
                    cellController.Update();
            }            
        }

    }
}
