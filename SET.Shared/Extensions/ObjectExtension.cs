using System;
using System.Reflection;

namespace SET.Shared.Extensions;

public static class ObjectExtension
{
    public static T? PropValue<T>(this object obj, string propName )
    {
        ArgumentException.ThrowIfNullOrEmpty( propName );

        PropertyInfo propertyInfo = obj.GetType().GetProperty( propName );
        if(propertyInfo == null)
        {
            throw new ArgumentException( $"Prop {propName} is not found in {obj}", nameof( propName ) );
        }
        else
        {
            var result = (T)propertyInfo.GetValue( obj );
            return result;
        }
    }
}

