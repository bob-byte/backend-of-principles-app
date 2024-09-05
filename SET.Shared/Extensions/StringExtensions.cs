using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SET.Shared.Extensions;

public static class StringExtensions
{
    public const char TABULATION = '\t';
    public const string TABULATION_AS_STR = "\t";

    public static string GetPropsAsStr( this object objectToConvert, string initialTabulation = "", string memberName = "" )
    {
        if (objectToConvert != null)
        {
            if (memberName == "")
            {
                memberName = objectToConvert.GetType().Name;
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.Append( $"{initialTabulation}{memberName}:\n" );

            foreach (PropertyInfo prop in objectToConvert.GetType().
                GetProperties().
                OrderBy( c => c.Name ))
            {
                if ((prop.PropertyType != typeof( string )) && typeof( IEnumerable ).IsAssignableFrom( prop.PropertyType ))
                {
                    if (prop.GetValue( objectToConvert ) is IEnumerable enumerable)
                    {
                        string tab = $"{initialTabulation}{TABULATION}";
                        stringBuilder.Append( enumerable.GetItemPropsAsStr( tab, enumerableName: prop.Name, nameOfEachItem: string.Empty ) );
                    }
                }
                else
                {
                    stringBuilder.Append( $"{initialTabulation}{VariableWithValue( prop.Name, prop.GetValue( objectToConvert, index: null ) )};\n" );
                }
            }

            return stringBuilder.ToString();
        }
        else
        {
            throw new ArgumentNullException( nameof( objectToConvert ) );
        }
    }

    public static string GetItemPropsAsStr( this IEnumerable enumerable, string initialTabulation = "", string enumerableName = "", string nameOfEachItem = "" )
    {
        string checkedEnumarableName = CheckedNameOfVariable( enumerableName, enumerable );

        var stringBuilder = new StringBuilder( value: $"{initialTabulation}{checkedEnumarableName}:\n" );

        foreach (object item in enumerable)
        {
            string itemAsStr = item?.ToString();

            string tab = $"{initialTabulation}{TABULATION}";
            itemAsStr = (itemAsStr != null) && itemAsStr.Equals( item.GetType().FullName, StringComparison.Ordinal ) ?
                item.GetPropsAsStr( tab, nameOfEachItem ) :
                $"{tab}{itemAsStr};";

            stringBuilder.Append( $"{itemAsStr}\n" );
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// With tabulation in start
    /// </summary>
    internal static string VariableWithValue<T>( string nameProp, T value, bool useTab = true )
    {
        string tab = useTab ? TABULATION_AS_STR : string.Empty;

        string propertyWithValue = $"{tab}{nameProp} = {value}";
        return propertyWithValue;
    }

    private static string CheckedNameOfVariable( string nameOfVariable, object variable )
    {
        if (string.IsNullOrWhiteSpace( nameOfVariable ))
        {
            nameOfVariable = variable.GetType().Name;
        }

        return nameOfVariable;
    }
}

