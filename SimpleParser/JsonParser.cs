using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleParser
{
    public class JsonParser
    {
        public static T Parse<T>(string value) where T : class, new()
        {
            var mainObject = new T();
            var mainType = typeof(T);
            if (value.Count(x => x == '{') == value.Count(x => x == '}') &&
                value.Count(x => x == '\"') % 2 == 0)
            {
                var mainItem = GetItems(value);
                mainObject = Parse(mainItem, mainType) as T;
            }
            return mainObject;
        }

        public static object Parse(HierarchicalItem mainItem, Type type)
        {
            var mainObject = Activator.CreateInstance(type);
            var mainType = type;
            foreach (var item in mainItem.Children)
            {
                var prop = mainType.GetProperty(item.Name.Replace("\"", ""));
                prop.SetValue(mainObject, 
                    item.Type != TypeEnum.Object 
                        ? item.Value 
                        : Parse(item, prop.PropertyType));
            }
            return mainObject;
        }

        public static HierarchicalItem GetItems(string value, HierarchicalItem item = null)
        {
            item ??= new HierarchicalItem();
            var obj = GetValue(value, '{', '}');
            var result = GetValues(obj, ',');
            result = NormalizeSplit(result, '{', '}', ',');

            foreach (var child in result)
            {
                if (!child.Contains('{'))
                {
                    var (name, dataValue, type) = GetDataFromValue(child);
                    item.Children.Add(
                        new HierarchicalItem
                        {
                            Name = name,
                            Value = type == TypeEnum.Integer ? int.Parse(dataValue) : (object)dataValue,
                            Type = type
                        });
                }
                else
                {
                    item.Children.Add(GetItems(string.Join(":", child.Split(':').Skip(1)), new HierarchicalItem
                    {
                        Type = TypeEnum.Object,
                        Name = child.Split(':')[0]
                    }));
                }
            }

            return item;
        }

        public static (string, string, TypeEnum) GetDataFromValue(string value)
        {
            var data = value.Split(':');
            return (data[0], data[1], data[1].Contains('\"') ? TypeEnum.String : TypeEnum.Integer);
        }

        public static string GetValue(string value, char startSymbol, char endSymbol)
        {
            var startIndex = -1;
            var depth = 0;
            for (var i = 0; i < value.Length; i++)
            {
                var sym = value[i];
                if (sym == startSymbol)
                {
                    depth++;
                    if (startIndex == -1)
                        startIndex = i;
                }

                if (sym != endSymbol) continue;
                if (depth == 1)
                {
                    return value.Substring(startIndex + 1, i - startIndex - 1);
                }

                depth--;
            }
            throw new FormatException("Нет закрывающего символа!");
        }

        public static List<string> GetValues(string value, char splitSymbol)
        {
            var result = new List<string>();
            var startIndex = 0;
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] == splitSymbol)
                {
                    result.Add(value.Substring(startIndex, i - startIndex));
                    startIndex = i + 1;
                }

                if (i >= value.Length - 1 && startIndex != i)
                    result.Add(value.Substring(startIndex, i - startIndex + 1));
            }

            return result;
        }

        public static List<string> NormalizeSplit(List<string> values, char openSym, char closeSym, char splitSym)
        {
            var res = new List<string>();
            var startIndex = 0;
            var notClosedCount = 0;

            for (var i = 0; i < values.Count; i++)
            {
                var value = values[i];
                var open = value.Count(x => x == openSym);
                var close = value.Count(x => x == closeSym);

                if (open == close && notClosedCount == 0)
                {
                    res.Add(value);
                    startIndex = i + 1;
                    continue;
                }

                if (open == 0 && close == 0) continue;
                var notClCount = close - open;

                if (notClosedCount + notClCount == 0)
                {
                    notClosedCount = 0;
                    res.Add(string.Join(splitSym, values.Skip(startIndex).Take(i - startIndex + 1)));
                    continue;
                }

                notClosedCount += notClCount;
            }

            return res;
        }
    }
}