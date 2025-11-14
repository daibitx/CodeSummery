using Crane.MethodHook;
using GWCustomerInfoCollection.STD.Utils;
using System.Reflection;
using System.Xml;

namespace STD.Hook
{
    public class NewHookMethods
    {
        //将过期日期修改为
        private static readonly string DATE_CHANGED_TO = (DateTime.Today.Year + 1).ToString() + "1230";
        #region New hook methods
        public static object NewMethodInvoke(MethodBase method, object obj, object[] parameters)
        {
            if (Assembly.GetCallingAssembly() != null && Assembly.GetCallingAssembly().FullName.StartsWith("Aspose.") && method.Name == "ParseExact" && parameters.Length > 0 && parameters[0].ToString().Contains("0827"))
            {
                var ret = DateTime.ParseExact(DATE_CHANGED_TO, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                ShowLog(method, ret, obj, parameters, true);
                return ret;
            }
            else if (Assembly.GetCallingAssembly() != null && Assembly.GetCallingAssembly().FullName.StartsWith("Aspose.") && method.Name == "ParseExact" && parameters.Length > 0 && System.Text.RegularExpressions.Regex.Match(parameters[0].ToString(), @"^\d{4}\.\d{2}\.\d{2}$").Success)
            {
                var ret = DateTime.ParseExact("20200501", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                ShowLog(method, ret, obj, parameters, true);
                return ret;
            }
            else if (Assembly.GetCallingAssembly() != null && Assembly.GetCallingAssembly().FullName.StartsWith("Aspose.") && method.Name == "get_InnerText" && obj is XmlElement && (obj as XmlElement).Name == "SubscriptionExpiry")
            {
                ////这里不能用，否则会出错。
                //ShowLog(method, DATE_CHANGED_TO, obj, parameters, true);
                //return DATE_CHANGED_TO;
            }
            else if (Assembly.GetCallingAssembly() != null && Assembly.GetCallingAssembly().FullName.StartsWith("Aspose.") && method.Name == "get_Ticks" && obj is DateTime && ((DateTime)obj).ToString("MMdd") == "0827")
            {
                var ret = DateTime.ParseExact(DATE_CHANGED_TO, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture).Ticks;
                ShowLog(method, ret, obj, parameters, true);
                return ret;
            }
            else if (Assembly.GetCallingAssembly() != null && Assembly.GetCallingAssembly().FullName.StartsWith("Aspose.") && method.DeclaringType != null && method.DeclaringType.Name == "String" && method.Name == "Compare")
            {
                if (parameters.Length == 2)
                {
                    if (parameters[0].ToString() == "20200827")
                    {
                        var ret = 1;
                        ShowLog(method, ret, obj, parameters, true);
                        return ret;
                    }
                    else if (parameters[1].ToString() == "20200827")
                    {
                        var ret = -1;
                        ShowLog(method, ret, obj, parameters, true);
                        return ret;
                    }
                }
            }
            else if (Assembly.GetCallingAssembly() != null && Assembly.GetCallingAssembly().FullName.StartsWith("Aspose.") && method.Name == "op_GreaterThan" && parameters.Length == 2 && parameters[1] is DateTime && ((DateTime)parameters[1]).ToString("MMdd") == "0827")
            {
                ShowLog(method, false, obj, parameters, true);
                return false;
            }
            else if (Assembly.GetCallingAssembly() != null && Assembly.GetCallingAssembly().FullName.StartsWith("Aspose.") && method.Name == "IndexOf" && parameters.Length > 0 && parameters[0].ToString().Contains("0827"))
            {
                ShowLog(method, 580, obj, parameters, true);
                return 580;
            }
            else if (Assembly.GetCallingAssembly() != null && Assembly.GetCallingAssembly().FullName.StartsWith("Aspose.") && method.Name == "Split" && System.Text.RegularExpressions.Regex.Match(obj.ToString(), @"^\d{4}\.\d{2}\.\d{2}$").Success
                     && obj != null && obj.ToString().Substring(0, 4) == DateTime.Now.Year.ToString())
            {
                var ret = new string[] { "2019", "08", "27" };
                ShowLog(method, ret, obj, parameters, true);
                return ret;
            }
            else if (Assembly.GetCallingAssembly() != null && Assembly.GetCallingAssembly().FullName.StartsWith("Aspose.") && method.Name == "get_Now")
            {
                var ret = DateTime.ParseExact("20200518", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                ShowLog(method, ret, obj, parameters, true);
                return ret;
            }
            var hook = MethodHookManager.Instance.GetHook(System.Reflection.MethodBase.GetCurrentMethod());
            var result = hook.InvokeOriginal<object>(method, obj, parameters?.ToArray());
            ShowLog(method, result, obj, parameters);
            return result;
        }
        /// <summary>
        /// 获取类型
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string Serialize(object obj)
        {
            if (obj is string) return obj?.ToString();
            if (obj is byte[])
            {
                return "byte[]";
            }
            if (obj is Stream)
            {
                return "<STREAM>...";
            }
            if (obj is IEnumerable<object>)
            {
                var arr = obj as IEnumerable<object>;
                if (arr.Any())
                {
                    return "[" + (arr.Aggregate("", (a, b) => (a == "" ? "" : Serialize(a) + ",") + Serialize(b))) + "]";
                }
                else
                {
                    return "[]";
                }
            }
            else if (obj is char[])
            {
                return string.Join(",", obj as char[]);
            }
            else
            {
                return obj?.ToString();
            }
        }
        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="method"></param>
        /// <param name="ret"></param>
        /// <param name="obj"></param>
        /// <param name="parameters"></param>
        /// <param name="isImportant"></param>
        public static void ShowLog(MethodBase method, object ret, object obj, object[] parameters, bool isImportant = false)
        {
            try
            {
                if (isImportant == false)
                {
                    // 只拦截字符串类型的和时间类型的元素
                    if ((method.DeclaringType?.Name.Contains("DateTime") == false
                         && method.DeclaringType?.Name.Contains("String.") == false)
                        || method.Name.Contains("ToCharArray")
                        || method.Name.Contains("IndexOf")
                        || method.Name.Contains("get_Length")
                        || method.Name.Contains("op_Equality")
                        || method.Name.Contains("ToLower"))
                    {
                        //Utils.LogWriteLine($"Bob Is Not Important {method.DeclaringType?.Name}.{method.Name}", ConsoleColor.White);
                        return;
                    }
                }
                var paras = string.Empty;
                try
                {
                    paras = Serialize(obj) + "," + Serialize(parameters);
                }
                catch (Exception)
                {
                    paras = obj + "," + parameters;
                }
                var returns = string.Empty;
                try
                {
                    returns = Serialize(ret);
                }
                catch (Exception)
                {
                    returns = ret?.ToString();
                }
                LogProvider.Info($"INVOKE method {method.DeclaringType?.Name}.{method.Name}({paras}) RETURN=> {returns}");
            }
            catch (Exception e)
            {
                LogProvider.Error("Error:" + e.Message);
            }
        }
        public static int NewCompare(string s1, string s2)
        {
            if (Assembly.GetCallingAssembly() != null && Assembly.GetCallingAssembly().FullName.StartsWith("Aspose.") && s2 == "20200827")
            {
                LogProvider.Info($"HOOK SUCCESS: From {Assembly.GetCallingAssembly().GetName().Name} String.Compare({s1},{s2}) return -1;");
                return -1;
            }
            else
            {
                var hook = MethodHookManager.Instance.GetHook(MethodBase.GetCurrentMethod());
                var ret = hook.InvokeOriginal<int>(null, s1, s2);
                LogProvider.Info($"NOT Aspose Call: From {Assembly.GetCallingAssembly().GetName().Name} String.Compare({s1},{s2}) return {ret};");
                return ret;
            }
        }
        public static bool NewGreaterThan(DateTime t1, DateTime t2)
        {
            if (Assembly.GetCallingAssembly() != null && Assembly.GetCallingAssembly().FullName.StartsWith("Aspose.") && t2.ToString("yyyyMMdd") == "20200827")
            {
                LogProvider.Info($"HOOK SUCCESS: From {Assembly.GetCallingAssembly().GetName().Name} DateTime ({t1}>{t2}) return false;");
                return false;
            }
            else
            {
                var hook = MethodHookManager.Instance.GetHook(MethodBase.GetCurrentMethod());
                var ret = hook.InvokeOriginal<bool>(null, t1, t2);
                return ret;
            }
        }
        public static DateTime NewParseExact(string s, string format, IFormatProvider provider)
        {
            if (Assembly.GetCallingAssembly() != null && Assembly.GetCallingAssembly().FullName.StartsWith("Aspose.") && s == "20200827")
            {
                LogProvider.Info($"HOOK SUCCESS: From {Assembly.GetCallingAssembly().GetName().Name} DateTime.ParseExact({s},{format},{provider}) return {DATE_CHANGED_TO};");
                var hook = MethodHookManager.Instance.GetHook(System.Reflection.MethodBase.GetCurrentMethod());
                return hook.InvokeOriginal<DateTime>(null, DATE_CHANGED_TO, format, provider);
            }
            else
            {
                var hook = MethodHookManager.Instance.GetHook(System.Reflection.MethodBase.GetCurrentMethod());
                return hook.InvokeOriginal<DateTime>(null, s, format, provider);
            }
        }
        public static string NewInnerText(XmlElement element)
        {
            if (Assembly.GetCallingAssembly() != null && Assembly.GetCallingAssembly().FullName.StartsWith("Aspose.") && Assembly.GetCallingAssembly().FullName.StartsWith("Aspose.Words") == false && Assembly.GetCallingAssembly().FullName.StartsWith("Aspose.Hook") == false && element.Name == "SubscriptionExpiry")
            {
                LogProvider.Info($"HOOK SUCCESS: From {Assembly.GetCallingAssembly().GetName().Name} XmlElement.InnerText ({element.Name},{element.InnerXml}) return {DATE_CHANGED_TO};");
                return DATE_CHANGED_TO;
            }
            else
            {
                var hook = MethodHookManager.Instance.GetHook(System.Reflection.MethodBase.GetCurrentMethod());
                return hook.InvokeOriginal<string>(element);
            }
        }
        public static int NewIndexOf(string v1, string v2)
        {
            if (Assembly.GetCallingAssembly() != null && Assembly.GetCallingAssembly().FullName.StartsWith("Aspose.") && v2 == DATE_CHANGED_TO)
            {
                LogProvider.Info($"HOOK SUCCESS: From {Assembly.GetCallingAssembly().GetName().Name} {v1.ToString().Substring(0, 9) + "..."}.IndexOf({v2}) return 580;");
                return 580;
            }
            else
            {
                var hook = MethodHookManager.Instance.GetHook(System.Reflection.MethodBase.GetCurrentMethod());
                var ret = hook.InvokeOriginal<int>(v1, v2);
                return ret;
            }
        }
        #endregion
    }

}
