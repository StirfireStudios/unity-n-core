using System;
using System.Reflection;
using System.Collections.Generic;

namespace N.Package.Core.Reflect
{
  /// Type related reflection functions
  public static class Type
  {
    /// Return a type instance for a fully qualified type name
    /// eg. N.Option
    public static Option<System.Type> Resolve(string qualifiedName)
    {
      var type = System.Type.GetType(qualifiedName);
      if (type != null) return Option.Some(type);
      foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
      {
        type = a.GetType(qualifiedName);
        if (type != null)
        {
          return Option.Some(type);
        }
      }
      return Option.None<System.Type>();
    }

    /// Return the fully qualified name of a type
    public static string Name<T>()
    {
      return typeof(T).FullName;
    }

    /// Return the fully qualified name of a type
    public static string Name<T>(T instance)
    {
      return instance.GetType().FullName;
    }

    /// Return the fully qualified namespace of a type
    public static string Namespace<T>()
    {
      return typeof(T).Namespace;
    }

    /// Return the fully qualified namespace of a type
    public static string Namespace<T>(T instance)
    {
      return typeof(T).Namespace;
    }

    /// Get all the public properties and field of a type
    /// @param getters Return properties which are readable
    /// @param setters Return properties which are writable
    public static string[] Fields<T>(T instance, bool getters = true, bool setters = true)
    {
      System.Type type;
      if (typeof(T) == typeof(System.Type))
      {
        type = instance as System.Type;
      }
      else
      {
        type = instance.GetType();
      }
      var rtn = new List<string>();
      foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
      {
        rtn.Add(field.Name);
      }
      foreach (var field in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
      {
        // 'Item' is magical property on List in unity, and causes problems?
        if (field.Name == "Item")
        {
          continue;
        }
        var getter = field.GetGetMethod();
        var setter = field.GetSetMethod();
        if ((getters && (getter != null)) && (setters && (setter != null)))
        {
          rtn.Add(field.Name);
        }
        else if ((getters && (getter != null)) && (!setters && (setter == null)))
        {
          rtn.Add(field.Name);
        }
        else if ((!getters && (getter == null)) && (setters && (setter != null)))
        {
          rtn.Add(field.Name);
        }
      }
      return rtn.ToArray();
    }

    /// Get all the public properties of a type by generic type
    /// @param getters Return properties which are readable
    /// @param setters Return properties which are writable
    public static string[] Fields<T>(bool getters = true, bool setters = true)
    {
      return Fields(typeof(T), getters, setters);
    }

    /// Resolve a key into a field info value on a type
    public static Option<Prop> Field(string typeName, string fieldName)
    {
      var type = Type.Resolve(typeName);
      if (type)
      {
        return Field(type.Unwrap(), fieldName);
      }
      return Option.None<Prop>();
    }

    /// Resolve a key into a field info value on a type
    public static Option<Prop> Field(System.Type type, string fieldName)
    {
      if (type != null)
      {
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
          if (field.Name == fieldName)
          {
            return Option.Some(new Prop(type, field));
          }
        }
        foreach (var field in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
          if (field.Name == fieldName)
          {
            return Option.Some(new Prop(type, field));
          }
        }
      }
      return Option.None<Prop>();
    }

    /// Check if a base class is an instance of a parent class
    public static bool Is<T>(object instance)
    {
      return instance is T;
    }

    /// Find all classes with the interface T
    public static IEnumerable<System.Type> Find<T>() where T : class
    {
      foreach (var currentAssembly in System.AppDomain.CurrentDomain.GetAssemblies())
      {
        var types = currentAssembly.GetTypes();
        foreach (var type in types)
        {
          if (Type.Implements<T>(type))
          {
            yield return type;
          }
        }
      }
    }

    /// Check if a type implements an interface
    /// Also requires IsPublic and the implementer is not an interface.
    public static bool Implements<T>(System.Type type)
    {
      return typeof(T).IsAssignableFrom(type) && type.IsPublic && !type.IsInterface && !type.IsAbstract;
    }
  }
}