
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace MappingValidator;

/// <summary>
/// Validates mappings between source and destination types to ensure required fields are populated.
/// </summary>
public static class MapValidator
{
    private static readonly ConcurrentDictionary<(Type Source, Type Destination), MappingValidationConfig> _configurations =
        new ConcurrentDictionary<(Type, Type), MappingValidationConfig>();

    /// <summary>
    /// Configuration for mapping validation rules.
    /// </summary>
    public class MappingValidationConfig
    {
        private readonly HashSet<string> _ignoredProperties;

        internal MappingValidationConfig(IEnumerable<string> ignoredProperties)
        {
            _ignoredProperties = new HashSet<string>(ignoredProperties, StringComparer.Ordinal);
        }

        public bool ShouldIgnore(string propertyName) => _ignoredProperties.Contains(propertyName);
    }

    /// <summary>
    /// Builder for configuring mapping validation rules.
    /// </summary>
    public sealed class ValidationConfigBuilder<TSource, TDestination>
    {
        private readonly HashSet<string> _ignoredProperties = new HashSet<string>();

        public ValidationConfigBuilder<TSource, TDestination> Ignore<TProperty>(
            Expression<Func<TDestination, TProperty>> propertyExpression)
        {
            if (propertyExpression.Body is MemberExpression memberExpression)
            {
                _ignoredProperties.Add(memberExpression.Member.Name);
            }
            else
            {
                throw new ArgumentException("Expression must be a property access", nameof(propertyExpression));
            }
            return this;
        }

        public MappingValidationConfig Build() => new MappingValidationConfig(_ignoredProperties);
    }

    /// <summary>
    /// Registers validation configuration for a source-destination pair.
    /// </summary>
    public static void Configure<TSource, TDestination>(
        Action<ValidationConfigBuilder<TSource, TDestination>> configureAction)
    {
        var builder = new ValidationConfigBuilder<TSource, TDestination>();
        configureAction(builder);
        _configurations[(typeof(TSource), typeof(TDestination))] = builder.Build();
    }

    /// <summary>
    /// Registers an empty validation configuration (no ignored properties).
    /// </summary>
    public static void Configure<TSource, TDestination>()
    {
        _configurations[(typeof(TSource), typeof(TDestination))] =
            new MappingValidationConfig(Enumerable.Empty<string>());
    }

    /// <summary>
    /// Validates that all non-ignored destination properties without source counterparts are properly set.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if configuration is missing.</exception>
    /// <exception cref="ValidationException">Thrown if required fields are unset.</exception>
    public static void Validate<TSource, TDestination>(TDestination destination)
    {
        if (!_configurations.TryGetValue((typeof(TSource), typeof(TDestination)), out var config))
        {
            throw new InvalidOperationException(
                $"No validation configuration registered for {typeof(TSource).Name} -> {typeof(TDestination).Name}. " +
                $"Call MapValidator.Configure<{typeof(TSource).Name}, {typeof(TDestination).Name}>() first.");
        }

        var sourceProperties = typeof(TSource)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToHashSet();

        var unsetProperties = typeof(TDestination)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .Where(p => !sourceProperties.Contains(p.Name))
            .Where(p => !config.ShouldIgnore(p.Name))
            .Where(p => IsUnset(p, destination))
            .Select(p => p.Name)
            .ToList();

        if (unsetProperties.Any())
        {
            throw new ValidationException(
                $"Required fields not set on {typeof(TDestination).Name}: {string.Join(", ", unsetProperties)}");
        }
    }

    private static bool IsUnset(PropertyInfo property, object obj)
    {
        var value = property.GetValue(obj);
        return value == null || Equals(value, GetDefault(property.PropertyType));
    }

    private static object? GetDefault(Type type) =>
        type.IsValueType ? Activator.CreateInstance(type) : null;
}
