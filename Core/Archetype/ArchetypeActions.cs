using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core
{
    internal static unsafe class ArchetypeActions
    {
        private const int FIND_DEEP = 1024;
        private const int ARCHETYPE_COUNT = 1024;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Setup(ref ArrayPtr<Node> archetypes)
        {
            for (uint i = 0, iMax = archetypes.ElementCount; i < iMax; i++)
            {
                ref var archetype = ref archetypes.GetRef(i);
                archetype.archetypeId = i;
                archetype.itemId = (ushort)i;
            }

            //_items = new BacketCollection(backetItemCapacity, (uint)_archetypeCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Add(ref ArrayPtr<Node> archetypes, ref uint archetypeCount, uint archetypeId, uint id, ushort itemId)
           => (archetypeId == 0)
               ? AddInternal(ref archetypes, id, itemId)
               : AddInternal(ref archetypes, ref archetypeCount, archetypeId, id, itemId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Remove(ref ArrayPtr<Node> archetypes, ref uint archetypeCount, uint archetypeId, uint id, ushort itemId)
            => (archetypeId == 0)
                ? RemoveInternal(id, itemId)
                : RemoveInternal(ref archetypes, ref archetypeCount, archetypeId, id, itemId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(ref ArrayPtr<Node> archetypes, uint archetypeId, uint id)
        {
            //_items.Remove(archetypes.GetRef(archetypeId).itemsCollectionId, id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint[] Filter(ref ArrayPtr<Node> archetypes, ushort[] items)
        {
            var archetypeIds = stackalloc uint[ARCHETYPE_COUNT];
            var count = Filter(ref archetypes, items, items.Length, archetypeIds);
            var result = new uint[count];
            for (int i = 0; i < count; ++i)
            {
                result[i] = archetypeIds[i];
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Filter(ref ArrayPtr<Node> archetypes, ushort[] items, int itemCount, uint* result)
        {
            int resultCount = 0;
            var items0 = items[0];
            for (uint i = 1; i <= items0; ++i)
            {
                FindPattern(ref archetypes, ref archetypes.GetRef(i), 0, items, itemCount, result, ref resultCount);
            }

            PatternDownExtend(ref archetypes, result, ref resultCount);

            return resultCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PatternDownExtend(ref ArrayPtr<Node> archetypes, uint* result, ref int resultCount)
        {
            var count = resultCount;
            for (int i = 0; i < count; ++i)
            {
                ref var node = ref archetypes.GetRef(result[i]);
                int jMax = node.childenCount;
                for (int j = 0; j < jMax; ++j)
                {
                    PatternDownExtend(ref archetypes, ref archetypes.GetRef(node.childen[j]), result, ref resultCount);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PatternDownExtend(ref ArrayPtr<Node> archetypes, ref Node node, uint* result, ref int resultCount)
        {
            result[resultCount++] = node.archetypeId;
            int iMax = node.childenCount;
            for (int i = 0; i < iMax; ++i)
            {
                PatternDownExtend(ref archetypes, ref archetypes.GetRef(node.childen[i]), result, ref resultCount);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FindPattern(ref ArrayPtr<Node> archetypes, ref Node node, int itemIndex, ushort[] items, int itemCount, uint* result, ref int resultCount)
        {
            var itemId = items[itemIndex];
            if (node.itemId <= itemId)
            {
                if (node.itemId == itemId)
                {
                    if (itemIndex == itemCount - 1)
                    {
                        result[resultCount++] = node.archetypeId;
                        return;
                    }

                    int iMax = node.childenCount;
                    for (int i = 0; i < iMax; ++i)
                    {
                        FindPattern(ref archetypes, ref archetypes.GetRef(node.childen[i]), itemIndex + 1, items, itemCount, result, ref resultCount);
                    }
                }
                else
                {
                    int iMax = node.childenCount;
                    for (int i = 0; i < iMax; ++i)
                    {
                        FindPattern(ref archetypes, ref archetypes.GetRef(node.childen[i]), itemIndex, items, itemCount, result, ref resultCount);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RemoveInternal(ref ArrayPtr<Node> archetypes, ref uint archetypeCount, uint archetypeId, uint id, ushort itemId)
        {
            ref var node = ref archetypes.GetRef(archetypeId);
            //_items.Remove(node.itemsCollectionId, id);

            ref var parent = ref archetypes.GetRef(node.parent);
            if (node.itemId == itemId)
            {
                //_items.Add(parent.itemsCollectionId, id);
                return parent.archetypeId;
            }
            else
            {
                int deep = 0;
                ushort* itemDeep = stackalloc ushort[FIND_DEEP];

                var itemNode = MoveUpToItemId(ref archetypes, ref node, itemId, itemDeep, ref deep);
                if (itemNode.parent == 0)
                {
                    ref var rootNode = ref archetypes.GetRef(itemDeep[deep - 1]);
                    ref var childNode = ref DeepAttachNewNode(ref archetypes, ref archetypeCount, ref rootNode, itemDeep, deep - 1);
                    //_items.Add(childNode.itemsCollectionId, id);
                    return childNode.archetypeId;
                }
                else
                {
                    ref var rootNode = ref archetypes.GetRef(itemNode.parent);
                    ref var childNode = ref DeepAttachNewNode(ref archetypes, ref archetypeCount, ref rootNode, itemDeep, deep);
                    //_items.Add(childNode.itemsCollectionId, id);
                    return childNode.archetypeId;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RemoveInternal(uint id, ushort itemId)
        {
            //_items.Remove(itemId, id);
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint AddInternal(ref ArrayPtr<Node> archetypes, ref uint archetypeCount, uint archetypeId, uint id, ushort itemId)
        {
#if !ANOTHERECS_RELEASE
            if (itemId == archetypes.GetRef(archetypeId).itemId)
            {
                throw new ArgumentException($"Item already added to {nameof(ArchetypeCollection)} '{itemId}'.");
            }
#endif
            ref var node = ref archetypes.GetRef(archetypeId);
            //_items.Remove(node.itemsCollectionId, id);

            if (itemId > node.itemId)     //Add as node child
            {
                ref var childNode = ref GetChildNode(ref archetypes, ref archetypeCount, ref node, itemId);
                //_items.Add(childNode.itemsCollectionId, id);
                return childNode.archetypeId;
            }
            else     //Finding right node
            {
                int deep = 0;
                ushort* itemDeep = stackalloc ushort[FIND_DEEP];

                ref var rootNode = ref MoveUpToLocalRoot(ref archetypes, ref node, itemId, itemDeep, ref deep);
                ref var childNode = ref DeepAttachNewNode(ref archetypes, ref archetypeCount, ref rootNode, itemDeep, deep);

                //_items.Add(childNode.itemsCollectionId, id);
                return childNode.archetypeId;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref Node MoveUpToItemId(ref ArrayPtr<Node> archetypes, ref Node startNode, ushort itemId, ushort* itemDeep, ref int deep)
        {
            ref var node = ref startNode;

            do
            {
                if (deep == FIND_DEEP)
                {
                    throw new Exception();
                }

                itemDeep[deep++] = node.itemId;

                node = ref archetypes.GetRef(node.parent);
            }
            while (node.itemId != itemId);

            return ref node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref Node MoveUpToLocalRoot(ref ArrayPtr<Node> archetypes, ref Node startNode, ushort itemId, ushort* itemDeep, ref int deep)
        {
            ref var node = ref startNode;

            do
            {
                if (deep == FIND_DEEP)
                {
                    throw new Exception();
                }

                itemDeep[deep++] = node.itemId;

                if (node.parent == 0)
                {
                    return ref archetypes.GetRef(itemId);
                }

                node = ref archetypes.GetRef(node.parent);
            }
            while (node.itemId > itemId);

            itemDeep[deep++] = itemId;

            return ref node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref Node DeepAttachNewNode(ref ArrayPtr<Node> archetypes, ref uint archetypeCount, ref Node startNode, ushort* itemIds, int itemCount)
        {
            ref Node node = ref startNode;
            for (int i = itemCount - 1; i >= 0; --i)
            {
                node = ref GetChildNode(ref archetypes, ref archetypeCount, ref node, itemIds[i]);
            }
            return ref node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint AddInternal(ref ArrayPtr<Node> archetypes, uint id, ushort itemId)
        {
            //_items.Add(archetypes.GetRef(itemId).itemsCollectionId, id);

            return itemId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref Node GetChildNode(ref ArrayPtr<Node> archetypes, ref uint archetypeCount, ref Node node, ushort itemId)
        {
            ref var childNode = ref FindChildNode(ref archetypes, ref node, itemId);
            if (childNode.archetypeId != 0)
            {
                return ref childNode;
            }

            return ref AttachNewNode(ref archetypes, ref archetypeCount, ref node, itemId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref Node FindChildNode(ref ArrayPtr<Node> archetypes, ref Node node, ushort itemId)
        {
            for (int i = 0; i < node.childenCount; ++i)       //TODO SER OPTIMIZATE
            {
                ref var childNode = ref archetypes.GetRef(node.childen[i]);
                if (childNode.itemId == itemId)
                {
                    return ref childNode;
                }
            }

            return ref archetypes.GetRef(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref Node AttachNewNode(ref ArrayPtr<Node> archetypes, ref uint archetypeCount, ref Node parent, ushort itemId)
        {
#if !ANOTHERECS_RELEASE
            if (parent.childenCount == Node.ChildenMax)
            {
                throw new InvalidOperationException();       //TODO SER
            }
#endif
            if (archetypeCount == archetypes.ElementCount)
            {
                archetypes.Resize(archetypeCount << 1);
            }

            var id = archetypeCount;
            ref var newNode = ref archetypes.GetRef(id);
            newNode.archetypeId = id;
            newNode.itemId = itemId;
            newNode.parent = parent.archetypeId;
            //newNode.itemsCollectionId = _items.Allocate();

            parent.childen[parent.childenCount++] = id;
            ++archetypeCount;

            return ref newNode;
        }
    }
}