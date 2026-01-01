using System.Runtime.CompilerServices;

namespace Lithium.Core.ECS;

public partial class World
{
    private readonly Dictionary<ArchetypeKey, Archetype> _archetypes = [];
    private readonly Dictionary<EntityId, Archetype> _entityArchetype = new();

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Archetype GetArchetype(params Type[] types)
    {
        var key = new ArchetypeKey(types);
        return _archetypes.GetValueOrDefault(key, Archetype.Empty);
    }

    public ArchetypeQuery<T1, T2> Query<T1, T2>()
        where T1 : struct, IComponent where T2 : struct, IComponent =>
        new(new FilteredQuery(this, GetArchetype(typeof(T1), typeof(T2))));

    internal readonly struct FilteredQuery(
        World world,
        Archetype archetype,
        int[]? with = null,
        int[]? without = null,
        int[]? hasAnyOf = null
    )
    {
        public readonly World World = world;
        public readonly Archetype Archetype = archetype;
        public readonly int[]? HasAnyOf = hasAnyOf;

        public ReadOnlySpan<int> With => with ?? [];
        public ReadOnlySpan<int> Without => without ?? [];

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Matches(Entity entity)
        {
            // Vérifier si l'entité a tous les tags requis
            if (with is { Length: > 0 } && !World.HasAllTags(entity, with))
                return false;

            // Vérifier si l'entité a un des tags à exclure
            if (without is { Length: > 0 } && World.HasAnyTag(entity, without))
                return false;

            // Si HasAnyOf est défini, vérifier que l'entité a au moins un des tags
            if (HasAnyOf is { Length: > 0 } && !World.HasAnyTag(entity, HasAnyOf))
                return false;

            return true;
        }
    }

    public readonly ref struct QueryResult<T1, T2>
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        private readonly ref readonly Entity _entity;
        private readonly ref T1 _c1;
        private readonly ref T2 _c2;

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QueryResult(ref readonly Entity entity, ref T1 c1, ref T2 c2)
        {
            _entity = ref entity;
            _c1 = ref c1;
            _c2 = ref c2;
        }

        public ref readonly Entity Entity => ref _entity;
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

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArchetypeQuery<T1, T2> WithAllTags(params Type[] types)
        {
            var with = _base.With.ToArray();
            var newWith = new int[with.Length + types.Length];

            Array.Copy(with, newWith, with.Length);

            for (var i = 0; i < types.Length; i++)
                newWith[with.Length + i] = TagTypeId.GetId(types[i]);

            return new ArchetypeQuery<T1, T2>(new FilteredQuery(_base.World, _base.Archetype, newWith,
                _base.Without.ToArray()));
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArchetypeQuery<T1, T2> WithAnyTags(params Type[] types)
        {
            var tagIds = new int[types.Length];

            for (var i = 0; i < types.Length; i++)
                tagIds[i] = TagTypeId.GetId(types[i]);

            return new ArchetypeQuery<T1, T2>(
                new FilteredQuery(
                    _base.World,
                    _base.Archetype,
                    _base.With.ToArray(),
                    _base.Without.ToArray(),
                    tagIds
                )
            );
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArchetypeQuery<T1, T2> WithoutTags(params Type[] types)
        {
            var without = _base.Without.ToArray();
            var newWithout = new int[without.Length + types.Length];

            Array.Copy(without, newWithout, without.Length);

            for (var i = 0; i < types.Length; i++)
                newWithout[without.Length + i] = TagTypeId.GetId(types[i]);

            return new ArchetypeQuery<T1, T2>(
                new FilteredQuery(
                    _base.World,
                    _base.Archetype,
                    _base.With.ToArray(),
                    newWithout,
                    _base.HasAnyOf
                )
            );
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArchetypeQuery<T1, T2> WithTag<T>() where T : struct, ITag
        {
            var with = _base.With.ToArray();
            var newWith = new int[with.Length + 1];

            Array.Copy(with, newWith, with.Length);
            newWith[with.Length] = TagTypeId<T>.Id;

            return new ArchetypeQuery<T1, T2>(new FilteredQuery(_base.World, _base.Archetype, newWith,
                _base.Without.ToArray()));
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArchetypeQuery<T1, T2> WithoutTag<T>() where T : struct, ITag
        {
            var without = _base.Without.ToArray();
            var newWithout = new int[without.Length + 1];

            Array.Copy(without, newWithout, without.Length);
            newWithout[without.Length] = TagTypeId<T>.Id;

            return new ArchetypeQuery<T1, T2>(new FilteredQuery(_base.World, _base.Archetype, _base.With.ToArray(),
                newWithout));
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEachEntity(QueryFunc<T1, T2> action)
        {
            foreach (var item in this)
            {
                action(
                    in item.Entity,
                    ref item.Component1,
                    ref item.Component2
                );
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_base.World, _base.Archetype.AsReadOnlySpan().ToArray(), _base);
        }

        public ref struct Enumerator
        {
            private readonly World _world;
            private readonly Entity[] _entities;
            private readonly FilteredQuery _filter;
            private int _index;

            public QueryResult<T1, T2> Current { get; private set; }

            internal Enumerator(World world, Entity[] entities, FilteredQuery filter)
            {
                _world = world;
                _entities = entities;
                _filter = filter;
                _index = -1;
                Current = default;
            }

            public bool MoveNext()
            {
                while (++_index < _entities.Length)
                {
                    ref var entity = ref _entities[_index];

                    if (!_filter.Matches(entity))
                        continue;

                    Current = new QueryResult<T1, T2>(
                        ref entity,
                        ref _world.GetComponentRef<T1>(entity),
                        ref _world.GetComponentRef<T2>(entity)
                    );
                    return true;
                }

                return false;
            }
        }
    }
}