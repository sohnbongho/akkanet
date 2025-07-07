using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Library.Helper
{
    public static class RedisConvertHelper
    {
        /// <summary>
        /// Redis에 넣기 위해 변환
        /// </summary>        
        /// <returns></returns>        
        public static RedisValue ConvertObjectToRedisValue(object value)
        {
            RedisValue redisValue;
            if (value is int intNum)
            {
                redisValue = intNum;
                return redisValue;
            }
            else if (value is long longNum)
            {
                redisValue = longNum;
                return redisValue;
            }
            else if (value is ulong ulongNum)
            {
                redisValue = ulongNum;
                return redisValue;
            }
            else if (value is double doubleNum)
            {
                redisValue = doubleNum;
                return redisValue;
            }
            else if (value is bool boolNum)
            {
                redisValue = boolNum;
                return redisValue;
            }

            // 위에 값들이 아니면 string이다
            redisValue = value.ToString();
            return redisValue;
        }

        public static object ConvertRedisValueToObject(RedisValue value)
        {
            if (value.IsNull)
            {
                return string.Empty;
            }

            var valueString = value.ToString();
            if (value.IsInteger)
            {                
                if (value.TryParse(out int intNum))
                {
                    return intNum;
                }
                else if (value.TryParse(out long longNum))
                {
                    return longNum;
                }
                else if (value.TryParse(out double doubleNum))
                {
                    return doubleNum;
                }
                else if (bool.TryParse(valueString, out bool boolValue))
                {
                    return boolValue;
                }
            }                        
            return valueString;
        }

        /// <summary>
        /// Redis에서 읽은 값을 class로 변환
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entries"></param>
        /// <returns></returns>
        public static T HashEntriesToClass<T>(HashEntry[] entries) where T : class, new()
        {
            var instance = new T();
            var properties = typeof(T).GetProperties();

            foreach (var entry in entries)
            {
                foreach (var property in properties)
                {
                    if (property.Name.Equals(entry.Name.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        // 속성 타입에 따라 적절히 변환하고 할당
                        if (property.PropertyType == typeof(int))
                        {
                            property.SetValue(instance, Convert.ToInt32(entry.Value.ToString()));
                        }
                        else if (property.PropertyType == typeof(string))
                        {
                            property.SetValue(instance, entry.Value.ToString());
                        }
                        else if (property.PropertyType == typeof(ulong))
                        {
                            property.SetValue(instance, Convert.ToUInt64(entry.Value.ToString()));
                        }
                        else if (property.PropertyType == typeof(short))
                        {
                            property.SetValue(instance, Convert.ToInt16(entry.Value.ToString()));
                        }
                        // DateTime 타입 처리
                        else if (property.PropertyType == typeof(DateTime))
                        {
                            if (DateTime.TryParse(entry.Value.ToString(), out var dateValue))
                            {
                                property.SetValue(instance, dateValue);
                            }
                        }
                    }
                }
            }

            return instance;
        }
        public static HashEntry[] ToHashEntries<T>(T obj) where T : class
        {
            var properties = typeof(T).GetProperties();
            var hashEntries = new List<HashEntry>();

            foreach (var property in properties)
            {
                var value = property.GetValue(obj);
                if (value != null) // null 값을 무시합니다.
                {
                    // DateTime 타입은 특별한 포맷으로 변환합니다.
                    if (value is DateTime dateTimeValue)
                    {
                        value = dateTimeValue.ToString("o"); // ISO 8601 포맷
                    }

                    hashEntries.Add(new HashEntry(property.Name, value.ToString()));
                }
            }

            return hashEntries.ToArray();
        }
        public static Dictionary<string, string> ToDictionary(HashEntry[] entries)
        {
            var dict = new Dictionary<string, string>(entries.Length);

            foreach (var entry in entries)
            {
                dict[entry.Name.ToString()] = entry.Value.ToString();
            }

            return dict;
        }
    }
}
