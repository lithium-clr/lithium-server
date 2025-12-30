using System.Buffers;
using System.Runtime.CompilerServices;

namespace Lithium.Core.ECS;

public partial class World
{
    private readonly Dictionary<ArchetypeKey, Archetype> _archetypes = [];
    private readonly Dictionary<EntityId, Archetype> _entityArchetype = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Archetype GetArchetype(Type t1, Type t2)
        => _archetypes.TryGetValue(new ArchetypeKey([t1, t2]), out var a)
            ? a
            : Archetype.Empty;

    public WorldQuery<T1, T2> Query<T1, T2>()
        where T1 : struct, IComponent where T2 : struct, IComponent =>
        new(this, GetArchetype(typeof(T1), typeof(T2)));

    public readonly ref struct QueryResult<T1, T2>
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        private readonly ref readonly Entity _entity;
        private readonly ref T1 _c1;
        private readonly ref T2 _c2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QueryResult(
            ref readonly Entity entity,
            ref T1 c1,
            ref T2 c2
        )
        {
            _entity = ref entity;
            _c1 = ref c1;
            _c2 = ref c2;
        }

        public ref readonly Entity Entity => ref _entity;
        public ref readonly T1 Component1 => ref _c1;
        public ref readonly T2 Component2 => ref _c2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(
            out Entity entity,
            out T1 c1,
            out T2 c2
        )
        {
            entity = _entity;
            c1 = _c1;
            c2 = _c2;
        }
    }

    public readonly struct WorldQuery<T1, T2> : IDisposable
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        private readonly World _world;
        private readonly Archetype _archetype;
        private readonly int[]? _with;
        private readonly int[]? _without;

        public delegate void QueryAction(
            ref readonly Entity entity,
            ref T1 c1,
            ref T2 c2);

        internal WorldQuery(World world, Archetype archetype, int[]? with = null, int[]? without = null)
        {
            _world = world;
            _archetype = archetype;
            _with = with;
            _without = without;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WorldQuery<T1, T2> HasTag<T>() where T : struct, ITag
        {
            var id = TagTypeId<T>.Id;
            var newWith = Append(_with, id);

            return new WorldQuery<T1, T2>(_world, _archetype, newWith, _without);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WorldQuery<T1, T2> WithoutTag<T>() where T : struct, ITag
        {
            var id = TagTypeId<T>.Id;
            var newWithout = Append(_without, id);

            return new WorldQuery<T1, T2>(_world, _archetype, _with, newWithout);
        }

        public void ForEachEntity(QueryAction action)
        {
            var entities = _archetype.AsSpan();

            foreach (var ent in entities)
            {
                ref readonly var entity = ref ent;

                if (_with is not null && !_world.HasAllTags(entity, _with)) continue;
                if (_without is not null && _world.HasAnyTag(entity, _without)) continue;

                action(in entity, ref _world.GetComponentRef<T1>(entity), ref _world.GetComponentRef<T2>(entity));
            }
        }

        public Enumerator GetEnumerator() => new(_world, _archetype, _with, _without);

        private static int[] Append(int[]? src, int value)
        {
            var pool = ArrayPool<int>.Shared;

            if (src is null)
            {
                var arr = pool.Rent(4);
                arr[0] = value;

                return arr[..1];
            }

            var dst = pool.Rent(src.Length + 1);
            Array.Copy(src, dst, src.Length);
            dst[src.Length] = value;

            return dst[..(src.Length + 1)];
        }

        public void Dispose()
        {
            if (_with is not null) ArrayPool<int>.Shared.Return(_with, clearArray: false);
            if (_without is not null) ArrayPool<int>.Shared.Return(_without, clearArray: false);
        }

        public ref struct Enumerator
        {
            private readonly World _world;
            private readonly Archetype _archetype;
            private readonly int[]? _with;
            private readonly int[]? _without;
            private int _index;

            public QueryResult<T1, T2> Current { get; private set; }

            internal Enumerator(World world, Archetype archetype, int[]? with, int[]? without)
            {
                _world = world;
                _archetype = archetype;
                _with = with;
                _without = without;
                _index = -1;

                Current = default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var entities = _archetype.AsSpan();

                while (++_index < entities.Length)
                {
                    ref readonly var entity = ref entities[_index];

                    if (_with != null && !_world.HasAllTags(entity, _with)) continue;
                    if (_without != null && _world.HasAnyTag(entity, _without)) continue;

                    Current = new QueryResult<T1, T2>(
                        in entity,
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