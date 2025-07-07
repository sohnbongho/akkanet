using Library.DTO;
using System.Globalization;

namespace Library.Helper
{
    public static class ConvertHelper
    {
        /// <summary>
        /// Enum형으로 변환해주는 함수
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ToEnum<T>(int value) where T : struct, Enum
        {
            //default 키워드는 주어진 타입의 기본값을 반환합니다.
            //열거형의 경우, 기본값은 그 열거형의 첫 번째 정의된 값(None=0)입니다.
            T result = default(T);
            if (Enum.IsDefined(typeof(T), value))
            {
                result = (T)Enum.ToObject(typeof(T), value);
            }
            return result;
        }

        /// <summary>
        /// dictionary -> class 변환하는 함수
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public static T ToClass<T>(Dictionary<string, object> values) where T : class, new()
        {
            var instance = new T();

            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                if (values.ContainsKey(property.Name) && property.CanWrite)
                {
                    property.SetValue(instance, values[property.Name]);
                }
            }

            return instance;
        }

        /// <summary>
        /// class -> dictionary 변환하는 함수
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDictionary<T>(T obj) where T : class
        {
            var dict = new Dictionary<string, object>();
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                if (property == null)
                    continue;
                
                var result = property.GetValue(obj);
                if (result == null)
                    continue;

                dict[property.Name] = result;
            }

            return dict;
        }
        /// <summary>
        /// 시간을 string을 변환
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string TimeToString(DateTime dateTime) 
        {
            return dateTime.ToString(ConstInfo.TimeFormat);
        }

        public static (bool, DateTime) StringToTime(string timeString)
        {
            // 문자열을 다시 DateTime 값으로 변환                        
            if(DateTime.TryParseExact(timeString, ConstInfo.TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDateTime))
            {
                return (true, parsedDateTime);
            }

            return (false, new DateTime());
        }

        public static string GetItemName(int itemId)
        {
            var instance = ItemDBLoaderHelper.Instance;
            return instance.GetItemName(itemId);
        }

        /// <summary>
        /// 시간 변경
        /// </summary>        
        public static int ToYYYYMMDD(DateTime dateTime)
        {
            return (dateTime.Year * 10000) + (dateTime.Month * 100) + (dateTime.Day);
        }

        public static DateTime ToYYYYMMDD(int yyyyMmDd)
        {
            var acquiredYear = yyyyMmDd / 10000;
            yyyyMmDd -= acquiredYear * 10000;

            var acquiredMonth = yyyyMmDd / 100;
            yyyyMmDd -= acquiredMonth * 100;

            var acquiredDay = yyyyMmDd;
            return new DateTime(acquiredYear, acquiredMonth, acquiredDay);
        }

        public static FeatureControlType GameModeToFeatureControl(GameModeType gameModeType)
            => gameModeType switch
            {
                GameModeType.DiggingWarrior => FeatureControlType.DiggingWarrior,
                GameModeType.Cleaning => FeatureControlType.Cleaning,
                GameModeType.BubblePop => FeatureControlType.BubblePop,
                GameModeType.CutShroom => FeatureControlType.CutShroom,
                _ => FeatureControlType.None
            };
        public static CurrencyType ItemSnToCurrencyType(int itemSn)
            => itemSn switch
            {
                (int)ItemIndex.Gold => CurrencyType.Gold,
                (int)ItemIndex.Diamond => CurrencyType.Diamond,
                (int)ItemIndex.Clover => CurrencyType.Clover,
                (int)ItemIndex.Bell => CurrencyType.Bell,
                (int)ItemIndex.PremiumGold => CurrencyType.PremiumGold,
                (int)ItemIndex.PremiumDiamond => CurrencyType.PremiumDiamond,
                (int)ItemIndex.ColorTube => CurrencyType.ColorTube,
                _ => CurrencyType.None
            };
    }
}
