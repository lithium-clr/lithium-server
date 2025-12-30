using System.Buffers;
using System.Runtime.CompilerServices;

namespace Lithium.Core.ECS;

public partial class World
{
    private readonly Dictionary<ArchetypeKey, Archetype> _archetypes = [];
    private readonly Dictionary<EntityId, Archetype> _entityArchetype = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Archetype GetArchetype(Type t1, Type t2)
    {
        var key = new ArchetypeKey([t1, t2]);
        return _archetypes.GetValueOrDefault(key, Archetype.Empty);
    }

    public WorldQuery<T1, T2> Query<T1, T2>()
        where T1 : struct, IComponent where T2 : struct, IComponent =>
        new(new FilteredQuery(this, GetArchetype(typeof(T1), typeof(T2))));

    internal class FilteredQuery(World world, Archetype archetype, int[]? with = null, int[]? without = null)
    {
        public readonly World World = world;
        public readonly Archetype Archetype = archetype;
        public readonly int[]? With = with;
        public readonly int[]? Without = without;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Matches(Entity entity)
        {
            if (With is not null && !World.HasAllTags(entity, With)) return false;
            if (Without is not null && World.HasAnyTag(entity, Without)) return false;
            
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] Append(int[]? src, int value)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReturnArray(int[]? array)
        {
            if (array is not null)
                ArrayPool<int>.Shared.Return(array, clearArray: false);
        }
    }

    public readonly ref struct QueryResult<T1, T2>
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        private readonly ref readonly Entity _entity;
        private readonly ref T1 _c1;
        private readonly ref T2 _c2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QueryResult(ref readonly Entity entity, ref T1 c1, ref T2 c2)
        {
            _entity = ref entity;
            _c1 = ref c1;
            _c2 = ref c2;
        }

        public ref readonly Entity Entity => ref _entity;
        public ref T1 Component1 => ref _c1;
        public ref T2 Component2 => ref _c2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out Entity entity, out T1 c1, out T2 c2)
        {
            entity = _entity;
            c1 = _c1;
            c2 = _c2;
        }
    }

    public readonly ref struct WorldQuery<T1, T2>
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        private readonly FilteredQuery _base;

        public delegate void QueryAction(ref readonly Entity entity, ref T1 c1, ref T2 c2);

        internal WorldQuery(FilteredQuery b)
        {
            _base = b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WorldQuery<T1, T2> HasTag<T>() where T : struct, ITag
        {
            var newWith = FilteredQuery.Append(_base.With, TagTypeId<T>.Id);
            return new WorldQuery<T1, T2>(new FilteredQuery(_base.World, _base.Archetype, newWith, _base.Without));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WorldQuery<T1, T2> WithoutTag<T>() where T : struct, ITag
        {
            var newWithout = FilteredQuery.Append(_base.Without, TagTypeId<T>.Id);
            return new WorldQuery<T1, T2>(new FilteredQuery(_base.World, _base.Archetype, _base.With, newWithout));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEachEntity(QueryAction action)
        {
            var span = _base.Archetype.AsSpan();

            for (var i = 0; i < span.Length; i++)
            {
                ref var entity = ref span[i];
                if (!_base.Matches(entity)) continue;

                action(
                    ref entity,
                    ref _base.World.GetComponentRef<T1>(entity),
                    ref _base.World.GetComponentRef<T2>(entity)
                );
            }
        }
        
        public Enumerator GetEnumerator()
        {
            return new Enumerator(_base.World, _base.Archetype.AsReadOnlySpan().ToArray());
        }

        public ref struct Enumerator
        {
            private readonly World _world;
            private readonly Entity[] _entities;
            private int _index;

            public QueryResult<T1, T2> Current { get; private set; }

            internal Enumerator(World world, Entity[] entities)
            {
                _world = world;
                _entities = entities;
                _index = -1;

                Current = default;
            }

            public bool MoveNext()
            {
                while (++_index < _entities.Length)
                {
                    ref var entity = ref _entities[_index];

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