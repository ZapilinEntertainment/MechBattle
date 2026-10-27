using Scellecs.Morpeh;
using System.Collections.Generic;
using VContainer;

namespace ZE.MechBattle
{
    public abstract class NavigationDataEntitiesFactoryBase<T> where T : ICreationApplier
    {
        protected readonly World _world;
        protected readonly List<T> _creationAppliersList;

        [Inject]
        public NavigationDataEntitiesFactoryBase(World world, IEnumerable<T> creationAppliers) : this(world)
        {
            
            if (creationAppliers != null)
            {
                _creationAppliersList = new(8);
                foreach (var applier in creationAppliers)
                {
                    _creationAppliersList.Add(applier);
                }
            }  
            else
            {
                _creationAppliersList = new(0);
            }
        }


        // manual creation method - for tests
        public NavigationDataEntitiesFactoryBase(World world, List<T> creationAppliers) : this(world)
        {
            _creationAppliersList = creationAppliers ?? new();
        }

        private NavigationDataEntitiesFactoryBase(World world)
        {
            _world = world;
            Init();
        }

        protected abstract void Init();

        protected Entity BuildWithAppliers()
        {
            var entity = _world.CreateEntity();
            foreach (var applier in _creationAppliersList)
            {
                applier.ApplyOnCreate(entity);
            }
            return entity;
        }

    }
}
