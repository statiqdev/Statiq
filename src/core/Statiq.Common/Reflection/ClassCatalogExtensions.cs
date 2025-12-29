using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Statiq.Common
{
    public static class ClassCatalogExtensions
    {
        /// <summary>
        /// Gets all types assignable from a specified type.
        /// </summary>
        /// <typeparam name="T">The type of classes to get.</typeparam>
        /// <param name="classCatalog">The class catalog.</param>
        /// <param name="includeAbstract"><c>true</c> to include abstract class types, <c>false</c> otherwise.</param>
        /// <returns>All classes of type <c>T</c>.</returns>
        public static IEnumerable<Type> GetTypesAssignableTo<T>(this ClassCatalog classCatalog, bool includeAbstract = false) =>
            classCatalog.GetTypesAssignableTo(typeof(T), includeAbstract);

        /// <summary>
        /// Gets instances for all classes of a specified assignable type..
        /// </summary>
        /// <typeparam name="T">The type of instances to get.</typeparam>
        /// <param name="classCatalog">The class catalog.</param>
        /// <returns>Instances for all classes of type <c>T</c>.</returns>
        public static IEnumerable<T> GetInstances<T>(this ClassCatalog classCatalog) =>
            classCatalog.GetInstances(typeof(T)).Cast<T>();

        /// <summary>
        /// Gets an instance of a specified assignable type and name.
        /// </summary>
        /// <typeparam name="T">The assignable type of the instance to get.</typeparam>
        /// <param name="classCatalog">The class catalog.</param>
        /// <param name="typeName">The name of the type.</param>
        /// <param name="ignoreCase">if set to <c>true</c> ignore the case of the type name.</param>
        /// <returns>
        /// An instance of the first class that matches the specified type and name.
        /// </returns>
        public static T GetInstance<T>(this ClassCatalog classCatalog, string typeName, bool ignoreCase = false) =>
            (T)classCatalog.GetInstance(typeof(T), typeName, ignoreCase);

        /// <summary>
        /// Gets an instance of a specified type.
        /// </summary>
        /// <typeparam name="T">The type of the instance to get.</typeparam>
        /// <param name="classCatalog">The class catalog.</param>
        /// <returns>
        /// An instance of the specified type, or the first class that can be assigned to the specified type, or <c>null</c>.
        /// </returns>
        public static T GetInstance<T>(this ClassCatalog classCatalog) =>
            (T)classCatalog.GetInstance(typeof(T));
    }
}
