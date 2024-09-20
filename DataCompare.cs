using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlazorShared;
public static class DataCompare {
    public static readonly Dictionary<Type, PropertyInfo[]> PropertyCache = [];
    public static List<string> JsonCompare(JObject obj1, JObject obj2) {
        var errors = new List<string>();
        CompareObjects(obj1, obj2, "", errors);
        return errors;
    }

    private static void CompareObjects(JObject obj1, JObject obj2, string path, List<string> errors) {
        foreach (var prop in obj1.Properties()) {
            var prop2 = obj2.Property(prop.Name);
            if (prop2 == null) {
                errors.Add($"{path}.{prop.Name}: Property not found in second object.");
                continue;
            }

            if (prop.Value.Type == JTokenType.Object) {
                CompareObjects(prop.Value as JObject, prop2.Value as JObject, $"{path}.{prop.Name}", errors);
            } else if (prop.Value.Type == JTokenType.Array) {
                CompareArrays(prop.Value as JArray, prop2.Value as JArray, $"{path}.{prop.Name}", errors);
            } else {
                if (!JToken.DeepEquals(prop.Value, prop2.Value)) {
                    errors.Add($"{path}.{prop.Name}: Values do not match (obj1: {prop.Value}, obj2: {prop2.Value})");
                }
            }
        }
    }

    private static void CompareArrays(JArray array1, JArray array2, string path, List<string> errors) {
        if (array1.Count != array2.Count) {
            errors.Add($"{path}: Array lengths do not match (obj1: {array1.Count}, obj2: {array2.Count})");
            return;
        }

        for (int i = 0; i < array1.Count; i++) {
            if (array1[i].Type == JTokenType.Object) {
                CompareObjects(array1[i] as JObject, array2[i] as JObject, $"{path}[{i}]", errors);
            } else if (array1[i].Type == JTokenType.Array) {
                CompareArrays(array1[i] as JArray, array2[i] as JArray, $"{path}[{i}]", errors);
            } else {
                if (!JToken.DeepEquals(array1[i], array2[i])) {
                    errors.Add($"{path}[{i}]: Values do not match (obj1: {array1[i]}, obj2: {array2[i]})");
                }
            }
        }
    }

    public static bool AreObjectsEqual(object obj1, object obj2) {
        if (obj1 == obj2) return true;

        var type = obj1.GetType();
        if (!PropertyCache.TryGetValue(type, out var properties)) {
            properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyCache[type] = properties;
        }

        foreach (var property in properties) {
            var value1 = property.GetValue(obj1);
            var value2 = property.GetValue(obj2);

            if (IsSimpleType(property.PropertyType)) {
                if (!Equals(value1, value2))
                    return false;
            } else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType)) {
                if (!AreCollectionsEqual((IEnumerable)value1, (IEnumerable)value2))
                    return false;
            } else if (!AreObjectsEqual(value1, value2)) {
                return false;
            }
        }

        return true;
    }

    private static bool IsSimpleType(Type type) {
        return type.IsPrimitive || type == typeof(string) || type.IsEnum || type == typeof(decimal) || type == typeof(DateTime);
    }

    private static bool AreCollectionsEqual(IEnumerable collection1, IEnumerable collection2) {
        if (collection1 == null || collection2 == null) return collection1 == collection2;

        var enumerator1 = collection1.GetEnumerator();
        var enumerator2 = collection2.GetEnumerator();

        while (enumerator1.MoveNext() && enumerator2.MoveNext()) {
            if (!AreObjectsEqual(enumerator1.Current, enumerator2.Current)) return false;
        }

        return !enumerator1.MoveNext() && !enumerator2.MoveNext();
    }
}

