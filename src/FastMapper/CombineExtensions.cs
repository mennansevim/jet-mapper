using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FastMapper
{
    /// <summary>
    /// Extension methods providing combine operations for FastMapper
    /// </summary>
    public static class CombineExtensions
    {
        /// <summary>
        /// Updates a specific property of the target object with a combine function after mapping
        /// </summary>
        /// <typeparam name="TTarget">Target object type</typeparam>
        /// <param name="target">Target object</param>
        /// <param name="propertyName">Name of property to update</param>
        /// <param name="value">Value to assign</param>
        /// <returns>Updated target object</returns>
        public static TTarget WithCombine<TTarget>(
            this TTarget target,
            string propertyName,
            string value)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            var property = typeof(TTarget).GetProperty(propertyName);
            if (property == null)
                throw new ArgumentException($"Property '{propertyName}' not found in class {typeof(TTarget).Name}", nameof(propertyName));

            if (property.PropertyType == typeof(string))
            {
                property.SetValue(target, value);
            }
            else
            {
                try
                {
                    var convertedValue = Convert.ChangeType(value, property.PropertyType);
                    property.SetValue(target, convertedValue);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Value '{value}' could not be converted to property '{propertyName}' type ({property.PropertyType.Name})", ex);
                }
            }

            return target;
        }

        /// <summary>
        /// Updates a specific property of the target object with a value calculated by a lambda function
        /// </summary>
        /// <typeparam name="TSource">Source object type</typeparam>
        /// <typeparam name="TTarget">Target object type</typeparam>
        /// <param name="target">Target object</param>
        /// <param name="propertyName">Name of property to update</param>
        /// <param name="source">Source object</param>
        /// <param name="valueFunc">Value calculation function</param>
        /// <returns>Updated target object</returns>
        public static TTarget WithCombine<TSource, TTarget>(
            this TTarget target,
            string propertyName,
            TSource source,
            Func<TSource, string> valueFunc)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException(nameof(propertyName));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (valueFunc == null)
                throw new ArgumentNullException(nameof(valueFunc));

            string value = valueFunc(source);
            return WithCombine(target, propertyName, value);
        }

        /// <summary>
        /// Updates multiple properties with different lambda functions after mapping
        /// </summary>
        /// <typeparam name="TSource">Source object type</typeparam>
        /// <typeparam name="TTarget">Target object type</typeparam>
        /// <param name="target">Target object</param>
        /// <param name="source">Source object</param>
        /// <param name="combineMappings">Property name and lambda function pairs</param>
        /// <returns>Updated target object</returns>
        public static TTarget WithMultipleCombines<TSource, TTarget>(
            this TTarget target,
            TSource source,
            params (string PropertyName, Func<TSource, string> ValueFunc)[] combineMappings)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (combineMappings == null || combineMappings.Length == 0)
                throw new ArgumentException("At least one combine mapping is required", nameof(combineMappings));

            foreach (var (propertyName, valueFunc) in combineMappings)
            {
                WithCombine(target, propertyName, source, valueFunc);
            }

            return target;
        }
    }
} 