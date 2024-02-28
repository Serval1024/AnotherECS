using System;
using System.Collections.Generic;
using System.Linq;

namespace AnotherECS.Core
{
    public static class OrderResolver
    {
        public static T[] GetByOrder<T>(T[] elements, int[] orders)
        {
            var result = new T[elements.Length];
            int index = 0;
            for (int i = 0; i < orders.Length; ++i)
            {
                if (orders[i] >= 0)
                {
                    result[index++] = elements[orders[i]];
                }
            }
            return result;
        }

        public static int[] GetOrder(IEnumerable<Element> options)
        {
            var state = new State(options.Append(Element.First).Append(Element.Last));

            while (state.dataToProcessing.Count != 0)
            {
                BuildNode(state.dataToProcessing.First().Value, state);
            }

            ValidateNode(state);

            return ConvertToArray(state);
        }

        private static void ValidateNode(State state)
        {
            foreach(var node in state.nodes)
            {
                if (node.Value.after.Any(p => p.id == node.Key))
                {
                    throw new Exceptions.InvalidNodeTopologyException(node.Value.id, "Node refers to itself in the 'after' modifier.");
                }
                if (node.Value.before.Any(p => p.id == node.Key))
                {
                    throw new Exceptions.InvalidNodeTopologyException(node.Value.id, "Node refers to itself in the 'before' modifier");
                }
                var nodeWithCrossRequirements = node.Value.after.FirstOrDefault(p => node.Value.before.Any(p0 => p0.id == p.id));
                if (nodeWithCrossRequirements != null)
                {
                    throw new Exceptions.InvalidNodeTopologyException(nodeWithCrossRequirements.id, "Node requires to be 'after' and 'before' at the same time");
                }
            }
        }

        private static int[] ConvertToArray(State state)
        {
            
            var nodes = state.nodes;
            var result = nodes.Keys.ToList();
            int interationCount = result.Count * 8;
            bool isIterationContinue = true;
            for (int interation = 0; interation < interationCount; ++interation)
            {
                if (isIterationContinue)
                {
                    isIterationContinue = false;
                    for (int i = 0; i < result.Count; ++i)
                    {
                        var after = nodes[result[i]].after;
                        for (int j = 0; j < after.Count; ++j)
                        {
                            for (int i0 = i + 1; i0 < result.Count; ++i0)
                            {
                                if (result[i0] == after[j].id)
                                {
                                    isIterationContinue = true;
                                    result.Insert(i0 + 1, result[i]);
                                    result.RemoveAt(i);
                                }
                            }
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            
            return result.ToArray();
        }

        private static Node BuildNode(Element element, State state)
        {
            var node = new Node(element.id);
            state.nodes.Add(node.id, node);
            state.dataToProcessing.Remove(element.id);

            node.before.AddRange(
                                element.relative
                                .Where(p => p.order == Element.Order.Before)
                                .Select(p => CreateNode(p, node, Element.Order.Before, state))
                                );

            node.after.AddRange(
                                element.relative
                                .Where(p => p.order == Element.Order.After)
                                .Select(p => CreateNode(p, node, Element.Order.After, state))
                                );

            return node;
        }

        private static Node CreateNode((Element.Order order, int id) element, Node current, Element.Order nodeMode, State state)
        {
            if (!state.nodes.TryGetValue(element.id, out Node node))
            {
                node = BuildNode(state.dataToProcessing[element.id], state);
            }
            if (nodeMode == Element.Order.Before)
            {
                node.after.Add(current);
            }
            else if (nodeMode == Element.Order.After)
            {
                node.before.Add(current);
            }
            return node;
        }

        public readonly struct Element
        {
            public static readonly Element First = new(FIRST_ID, new (Order, int id)[] { (Order.Before, LAST_ID) });
            public static readonly Element Last = new(LAST_ID, new (Order, int id)[] { (Order.After, FIRST_ID) });

            public const int FIRST_ID = -1;
            public const int LAST_ID = -2;

            public readonly int id;
            public readonly (Order order, int id)[] relative;

            public Element(int id)
                : this(id, Array.Empty<(Order, int)>())
            { }

            public Element(int id, (Order, int id)[] relative)
            {
                this.id = id;
                this.relative = relative;
            }

            public enum Order
            {
                Before,
                After,
            }
        }

        private struct State
        {
            public readonly Dictionary<int, Element> dataToProcessing;
            public readonly Dictionary<int, Node> nodes;

            public State(IEnumerable<Element> options)
            {
                dataToProcessing = options.ToDictionary(k => k.id, v => v);
                nodes = new();
            }
        }

        private class Node
        {
            public int id;
            public List<Node> before;
            public List<Node> after;

            public Node(int id)
            {
                this.id = id;
                this.before = new();
                this.after = new();
            }
        }
    }
}
