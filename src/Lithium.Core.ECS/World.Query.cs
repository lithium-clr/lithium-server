using System.Runtime.CompilerServices;

namespace Lithium.Core.ECS;

public partial class World
{
    private readonly Dictionary<ArchetypeKey, Archetype> _archetypes = [];
    private Archetype[] _entityArchetypes = new Archetype[1024];

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Archetype GetArchetype(params Type[] types)
    {
        var key = new ArchetypeKey(types);
        return _archetypes.GetValueOrDefault(key, Archetype.Empty);
    }

    public ArchetypeQuery<T1, T2> Query<T1, T2>()
        where T1 : struct, IComponent where T2 : struct, IComponent =>
        new(new FilteredQuery(this, GetArchetype(typeof(T1), typeof(T2))));

    internal readonly struct FilteredQuery
    {
        public readonly World World;
        public readonly Archetype Archetype;
        public readonly TagBitset WithMask;
        public readonly TagBitset WithoutMask;
        public readonly TagBitset AnyMask;
        public readonly bool HasWith;
        public readonly bool HasWithout;
        public readonly bool HasAny;

        public FilteredQuery(
            World world,
            Archetype archetype,
            TagBitset with = default,
            TagBitset without = default,
            TagBitset any = default,
            bool hasWith = false,
            bool hasWithout = false,
            bool hasAny = false
        )
        {
            World = world;
            Archetype = archetype;
            WithMask = with;
            WithoutMask = without;
            AnyMask = any;
            HasWith = hasWith;
            HasWithout = hasWithout;
            HasAny = hasAny;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Matches(Entity entity)
        {
            if (HasWith && !World.HasAllTags(entity, WithMask))
                return false;

            if (HasWithout && World.HasAnyTag(entity, WithoutMask))
                return false;

            if (HasAny && !World.HasAnyTag(entity, AnyMask))
                return false;

            return true;
        }
    }

    public readonly ref struct QueryResult<T1, T2>
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        private readonly Entity _entity;
        private readonly ref T1 _c1;
        private readonly ref T2 _c2;

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QueryResult(Entity entity, ref T1 c1, ref T2 c2)
        {
            _entity = entity;
            _c1 = ref c1;
            _c2 = ref c2;
        }

        public Entity Entity => _entity;
        public ref T1 Component1 => ref _c1;
        public ref T2 Component2 => ref _c2;

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out Entity entity, out T1 c1, out T2 c2)
        {
            entity = _entity;
            c1 = _c1;
            c2 = _c2;
        }
    }

    public delegate void QueryFunc<T1, T2>(ref readonly Entity entity, ref T1 c1, ref T2 c2)
        where T1 : struct, IComponent
        where T2 : struct, IComponent;

    public readonly ref struct ArchetypeQuery<T1, T2>
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        private readonly FilteredQuery _base;

        internal ArchetypeQuery(FilteredQuery b)
        {
            _base = b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArchetypeQuery<T1, T2> WithAllTags(params Type[] types)
        {
            var mask = _base.WithMask;
            foreach (var t in types) mask.Add(TagTypeId.GetId(t));
            
            return new ArchetypeQuery<T1, T2>(new FilteredQuery(
                _base.World, 
                _base.Archetype, 
                mask,
                _base.WithoutMask,
                _base.AnyMask,
                true,
                _base.HasWithout,
                _base.HasAny
            ));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArchetypeQuery<T1, T2> WithAnyTags(params Type[] types)
        {
            var mask = _base.AnyMask;
            foreach (var t in types) mask.Add(TagTypeId.GetId(t));

            return new ArchetypeQuery<T1, T2>(new FilteredQuery(
                _base.World,
                _base.Archetype,
                _base.WithMask,
                _base.WithoutMask,
                mask,
                _base.HasWith,
                _base.HasWithout,
                true
            ));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArchetypeQuery<T1, T2> WithoutTags(params Type[] types)
        {
            var mask = _base.WithoutMask;
            foreach (var t in types) mask.Add(TagTypeId.GetId(t));

            return new ArchetypeQuery<T1, T2>(new FilteredQuery(
                _base.World,
                _base.Archetype,
                _base.WithMask,
                mask,
                _base.AnyMask,
                _base.HasWith,
                true,
                _base.HasAny
            ));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArchetypeQuery<T1, T2> WithTag<T>() where T : struct, ITag
        {
            var mask = _base.WithMask;
            mask.Add(TagTypeId<T>.Id);

            return new ArchetypeQuery<T1, T2>(new FilteredQuery(
                _base.World, 
                _base.Archetype, 
                mask,
                _base.WithoutMask,
                _base.AnyMask,
                true,
                _base.HasWithout,
                _base.HasAny
            ));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArchetypeQuery<T1, T2> WithoutTag<T>() where T : struct, ITag
        {
            var mask = _base.WithoutMask;
            mask.Add(TagTypeId<T>.Id);

            return new ArchetypeQuery<T1, T2>(new FilteredQuery(
                _base.World, 
                _base.Archetype, 
                _base.WithMask,
                mask,
                _base.AnyMask,
                _base.HasWith,
                true,
                _base.HasAny
            ));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEachEntity(QueryFunc<T1, T2> action)
        {
            var set1 = _base.World.GetComponentSet<T1>();
            var set2 = _base.World.GetComponentSet<T2>();
            
            // Optimization: Iterate over the smaller set
            if (set1.Count <= set2.Count)
            {
                var entities = set1.Entities;
                var dense1 = set1.GetDenseArray();
                var sparse2 = set2.GetSparseArray();
                var dense2 = set2.GetDenseArray();
                
                for (int i = 0; i < set1.Count; i++)
                {
                    var entityId = entities[i];
                    var id = (int)entityId;
                    
                    if (id >= sparse2.Length) continue;
                    var idx2 = sparse2[id];
                    if (idx2 == -1) continue;

                    var entity = new Entity(_base.World, entityId);
                    
                    if (!_base.Matches(entity)) continue;
                    
                    action(in entity, ref dense1[i], ref dense2[idx2]);
                }
            }
            else
            {
                var entities = set2.Entities;
                var dense2 = set2.GetDenseArray();
                var sparse1 = set1.GetSparseArray();
                var dense1 = set1.GetDenseArray();
                
                for (int i = 0; i < set2.Count; i++)
                {
                    var entityId = entities[i];
                    var id = (int)entityId;
                    
                    if (id >= sparse1.Length) continue;
                    var idx1 = sparse1[id];
                    if (idx1 == -1) continue;

                    var entity = new Entity(_base.World, entityId);
                    
                    if (!_base.Matches(entity)) continue;
                    
                    action(in entity, ref dense1[idx1], ref dense2[i]);
                }
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_base.World, _base);
        }

        public ref struct Enumerator
        {
            private readonly World _world;
            private readonly FilteredQuery _filter;
            private readonly SparseSet<T1> _set1;
            private readonly SparseSet<T2> _set2;
            private readonly bool _iterateSet1;
            private int _index;
            private Entity _currentEntity;

            public QueryResult<T1, T2> Current { get; private set; }

            internal Enumerator(World world, FilteredQuery filter)
            {
                _world = world;
                _filter = filter;
                _set1 = world.GetComponentSet<T1>();
                _set2 = world.GetComponentSet<T2>();
                _iterateSet1 = _set1.Count <= _set2.Count;
                _index = -1;
                Current = default;
                _currentEntity = default;
            }

            public bool MoveNext()
            {
                if (_iterateSet1)
                {
                    var entities = _set1.Entities;
                    var dense1 = _set1.GetDenseArray();
                    var sparse2 = _set2.GetSparseArray();
                    var dense2 = _set2.GetDenseArray();
                    
                    while (++_index < _set1.Count)
                    {
                        var entityId = entities[_index];
                        var id = (int)entityId;
                        
                        if (id >= sparse2.Length) continue;
                        var idx2 = sparse2[id];
                        if (idx2 == -1) continue;

                        _currentEntity = new Entity(_world, entityId);

                        if (!_filter.Matches(_currentEntity))
                            continue;

                        Current = new QueryResult<T1, T2>(
                            _currentEntity,
                            ref dense1[_index],
                            ref dense2[idx2]
                        );
                        return true;
                    }
                }
                else
                {
                    var entities = _set2.Entities;
                    var dense2 = _set2.GetDenseArray();
                    var sparse1 = _set1.GetSparseArray();
                    var dense1 = _set1.GetDenseArray();
                    
                    while (++_index < _set2.Count)
                    {
                        var entityId = entities[_index];
                        var id = (int)entityId;
                        
                        if (id >= sparse1.Length) continue;
                        var idx1 = sparse1[id];
                        if (idx1 == -1) continue;

                        _currentEntity = new Entity(_world, entityId);

                        if (!_filter.Matches(_currentEntity))
                            continue;

                        Current = new QueryResult<T1, T2>(
                            _currentEntity,
                            ref dense1[idx1],
                            ref dense2[_index]
                        );
                        return true;
                    }
                }

                return false;
            }
        }
    }
}