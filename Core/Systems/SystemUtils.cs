using System;
using System.Collections.Generic;
using System.Linq;

namespace AnotherECS.Core
{
    internal static class SystemUtils
    {
        public static Type[] GetOrder(Type[] types)
        {
            try
            {
                return OrderResolver.GetByOrder(types, OrderResolver.GetOrder(CreateElements(types)));
            }
            catch (Exceptions.InvalideNodeTopologyException e)
            {
                throw new Exceptions.InvalideSystemOrderException(types[e.Id0], e.Message, e);
            }
        }

        private static OrderResolver.Element[] CreateElements(Type[] types)
        {
            var typeTable = types
                .Select((type, index) => (type, index))
                .ToArray();

            var elements = typeTable
                .Select((p) => CreateElement(p.index, typeTable))
                .ToArray();

            Validate(elements, types);

            return elements;
        }

        private static void Validate(OrderResolver.Element[] elements, Type[] types)
        {
            try
            {
                Validate(elements, OrderResolver.Element.FIRST_ID, OrderResolver.Element.Order.Before);
            }
            catch (Exceptions.InvalideNodeTopologyException e)
            {
                throw new Exceptions.InvalideSystemOrderException(types[e.Id0], $"The system '{types[e.Id0]}' cannot be earlier than the '{nameof(SystemOrder.First)}' marked system '{types[e.Id1]}'.");
            }
            try
            {
                Validate(elements, OrderResolver.Element.LAST_ID, OrderResolver.Element.Order.After);
            }
            catch (Exceptions.InvalideNodeTopologyException e)
            {
                throw new Exceptions.InvalideSystemOrderException(types[e.Id0], $"The system '{types[e.Id0]}' cannot be later than the '{nameof(SystemOrder.Last)}' marked system '{types[e.Id1]}'.");
            }
        }

        private static void Validate(OrderResolver.Element[] elements, int valiadateId, OrderResolver.Element.Order validateOrder)
        {
            foreach (var element in elements)
            {
                if (!element.relative.Any(p => p.id == valiadateId && p.order == validateOrder))
                {
                    foreach (var (order, id) in element.relative)
                    {
                        if (id != valiadateId && order == validateOrder)
                        {
                            if (id != OrderResolver.Element.FIRST_ID && id != OrderResolver.Element.LAST_ID)
                            {
                                var other = elements.First(p => p.id == id);
                                if (other.relative.Any(p => p.id == valiadateId && p.order == validateOrder))
                                {
                                    throw new Exceptions.InvalideNodeTopologyException(element.id, other.id, string.Empty);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static OrderResolver.Element CreateElement(int index, (Type type, int index)[] types)
        {
            var attributes = types[index].type
                .GetCustomAttributes(typeof(SystemOrderAttribute), false)
                .Cast<SystemOrderAttribute>()
                .ToArray();

            var order = new List<(OrderResolver.Element.Order order, int id)>();

            if (attributes != null && attributes.Length != 0)
            {
                order = attributes
                    .Where(p => p.OrderRelative != SystemOrderRelative.None)
                    .Where(p => types.Any(p0 => p0.type == p.System))
                    .Select(p => (
                    p.OrderRelative == SystemOrderRelative.After ? OrderResolver.Element.Order.After : OrderResolver.Element.Order.Before,
                    types.First(p0 => p0.type == p.System).index)
                    )
                    .ToList();

                var attr = attributes
                    .FirstOrDefault(p => p.Order != SystemOrder.None);

                if (attr != null)
                {
                    if (attr.Order == SystemOrder.First)
                    {
                        order.Add((OrderResolver.Element.Order.Before, OrderResolver.Element.FIRST_ID));
                    }
                    else if(attr.Order == SystemOrder.Last)
                    {
                        order.Add((OrderResolver.Element.Order.After, OrderResolver.Element.LAST_ID));
                    }
                }
                else
                {
                    order.Add((OrderResolver.Element.Order.After, OrderResolver.Element.FIRST_ID));
                    order.Add((OrderResolver.Element.Order.Before, OrderResolver.Element.LAST_ID));
                }
            }
            else
            {
                order.Add((OrderResolver.Element.Order.After, OrderResolver.Element.FIRST_ID));
                order.Add((OrderResolver.Element.Order.Before, OrderResolver.Element.LAST_ID));
            }

            return new OrderResolver.Element(index, order.ToArray());
        }
    }
}