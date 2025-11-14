using Crane.MethodHook;
using GWCustomerInfoCollection.STD.Utils;
using System.Reflection;
using System.Xml;

namespace STD.Hook
{
    public static class HookManager
    {
        private class HookItem
        {
            public HookMethods Method { get; set; }
            public MethodHook Hook { get; set; }
            public bool Enabled { get; set; }
        }
        //所有的license都只需要设置一次
        private static List<string> mAssembliesLicenseSetted = new List<string>();
        //将于20200827过期的Aspose.Total开发版序列号
        private const string LICENSE_STRING = "PExpY2Vuc2U+CiAgPERhdGE+CiAgICA8TGljZW5zZWRUbz5TdXpob3UgQXVuYm94IFNvZnR3YXJlIENvLiwgT" +
                                              "HRkLjwvTGljZW5zZWRUbz4KICAgIDxFbWFpbFRvPnNhbGVzQGF1bnRlYy5jb208L0VtYWlsVG8+CiAgICA8TG" +
                                              "ljZW5zZVR5cGU+RGV2ZWxvcGVyIE9FTTwvTGljZW5zZVR5cGU+CiAgICA8TGljZW5zZU5vdGU+TGltaXRlZCB" +
                                              "0byAxIGRldmVsb3BlciwgdW5saW1pdGVkIHBoeXNpY2FsIGxvY2F0aW9uczwvTGljZW5zZU5vdGU+CiAgICA8" +
                                              "T3JkZXJJRD4xOTA4MjYwODA3NTM8L09yZGVySUQ+CiAgICA8VXNlcklEPjEzNDk3NjAwNjwvVXNlcklEPgogI" +
                                              "CAgPE9FTT5UaGlzIGlzIGEgcmVkaXN0cmlidXRhYmxlIGxpY2Vuc2U8L09FTT4KICAgIDxQcm9kdWN0cz4KIC" +
                                              "AgICAgPFByb2R1Y3Q+QXNwb3NlLlRvdGFsIGZvciAuTkVUPC9Qcm9kdWN0PgogICAgPC9Qcm9kdWN0cz4KICA" +
                                              "gIDxFZGl0aW9uVHlwZT5FbnRlcnByaXNlPC9FZGl0aW9uVHlwZT4KICAgIDxTZXJpYWxOdW1iZXI+M2U0NGRl" +
                                              "MzAtZmNkMi00MTA2LWIzNWQtNDZjNmEzNzE1ZmMyPC9TZXJpYWxOdW1iZXI+CiAgICA8U3Vic2NyaXB0aW9uR" +
                                              "XhwaXJ5PjIwMjAwODI3PC9TdWJzY3JpcHRpb25FeHBpcnk+CiAgICA8TGljZW5zZVZlcnNpb24+My4wPC9MaW" +
                                              "NlbnNlVmVyc2lvbj4KICAgIDxMaWNlbnNlSW5zdHJ1Y3Rpb25zPmh0dHBzOi8vcHVyY2hhc2UuYXNwb3NlLmN" +
                                              "vbS9wb2xpY2llcy91c2UtbGljZW5zZTwvTGljZW5zZUluc3RydWN0aW9ucz4KICA8L0RhdGE+CiAgPFNpZ25h" +
                                              "dHVyZT53UGJtNUt3ZTYvRFZXWFNIY1o4d2FiVEFQQXlSR0pEOGI3L00zVkV4YWZpQnd5U2h3YWtrNGI5N2c2e" +
                                              "GtnTjhtbUFGY3J0c0cwd1ZDcnp6MytVYk9iQjRYUndTZWxsTFdXeXNDL0haTDNpN01SMC9jZUFxaVZFOU0rWn" +
                                              "dOQkR4RnlRbE9uYTFQajhQMzhzR1grQ3ZsemJLZFZPZXk1S3A2dDN5c0dqYWtaL1E9PC9TaWduYXR1cmU+CjwvTGljZW5zZT4=";
        private static List<HookItem> mHookStatus = new List<HookItem>();
        private static bool mHookStarted = false;
        static HookManager()
        {
        }
        static void InitializeHookList()
        {
            if (mHookStatus.Count == 0)
            {
                mHookStatus.Add(new HookItem
                {
                    Method = HookMethods.Invoke,
                    Hook = new MethodHook(
                        typeof(MethodBase).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(object), typeof(object[]) }, null),
                        typeof(NewHookMethods).GetMethod(nameof(NewHookMethods.NewMethodInvoke), BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(MethodBase), typeof(object), typeof(object[]) }, null)
                    ),
                    Enabled = true
                });
                mHookStatus.Add(new HookItem
                {
                    Method = HookMethods.ParseExact,
                    Hook = new MethodHook(
                        typeof(DateTime).GetMethod("ParseExact", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string), typeof(string), typeof(IFormatProvider) }, null),
                        typeof(NewHookMethods).GetMethod(nameof(NewHookMethods.NewParseExact), BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string), typeof(string), typeof(IFormatProvider) }, null)
                    ),
                    Enabled = true
                });
                mHookStatus.Add(new HookItem
                {
                    Method = HookMethods.DateTimeOpGreaterThan,
                    Hook = new MethodHook(
                        typeof(DateTime).GetMethod("op_GreaterThan", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(DateTime), typeof(DateTime) }, null),
                        typeof(NewHookMethods).GetMethod(nameof(NewHookMethods.NewGreaterThan), BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(DateTime), typeof(DateTime) }, null)
                    ),
                    Enabled = true
                });
                mHookStatus.Add(new HookItem
                {
                    Method = HookMethods.StringCompare,
                    Hook = new MethodHook(
                        typeof(string).GetMethod("Compare", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string), typeof(string) }, null),
                        typeof(NewHookMethods).GetMethod(nameof(NewHookMethods.NewCompare), BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string), typeof(string) }, null)
                    ),
                    Enabled = true
                });
                mHookStatus.Add(new HookItem
                {
                    Method = HookMethods.StringIndexOf,
                    Hook = new MethodHook(
                        typeof(string).GetMethod("IndexOf", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(string) }, null),
                        typeof(NewHookMethods).GetMethod(nameof(NewHookMethods.NewIndexOf), BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string), typeof(string) }, null)
                    ),
                    Enabled = true
                });
                mHookStatus.Add(new HookItem
                {
                    Method = HookMethods.XmlElementInnerText,
#if NET40
                    Hook = new MethodHook(
                        typeof(XmlElement).GetProperty("InnerText", BindingFlags.Public | BindingFlags.Instance).GetGetMethod(true),
                        typeof(NewHookMethods).GetMethod(nameof(NewHookMethods.NewInnerText), BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(XmlElement) }, null)
                    ),
#else
                    Hook = new MethodHook(
                        typeof(XmlElement).GetProperty("InnerText", BindingFlags.Public | BindingFlags.Instance).GetMethod,
                        typeof(NewHookMethods).GetMethod(nameof(NewHookMethods.NewInnerText), BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(XmlElement) }, null)
                    ),
#endif
                    Enabled = true
                });
                foreach (var item in mHookStatus)
                {
                    if (item.Enabled)
                    {
                        MethodHookManager.Instance.AddHook(item.Hook);
                    }
                }
            }
        }
        /// <summary>
        /// 设置需要启用的方法清单（默认已启用了所有）
        /// </summary>
        /// <param name="methods"></param>
        public static void SetHookMethods(HookMethods methods)
        {
            InitializeHookList();
            MethodHookManager.Instance.StopHook();
            MethodHookManager.Instance.RemoveAllHook();
            foreach (var item in mHookStatus)
            {
                if (methods.HasFlag(item.Method))
                {
                    item.Enabled = true;
                    MethodHookManager.Instance.AddHook(item.Hook);
                }
                else
                {
                    item.Enabled = false;
                }
            }
            if (mHookStarted)
            {
                MethodHookManager.Instance.StartHook();
            }
        }
        /// <summary>
        /// 启用hook
        /// </summary>
        public static void StartHook()
        {
            if (mHookStarted)
            {
                return;
            }
            try
            {
                InitializeHookList();
                MethodHookManager.Instance.StartHook();
                var assemblies = Assembly.GetCallingAssembly()?.GetReferencedAssemblies().Union(Assembly.GetEntryAssembly()?.GetReferencedAssemblies() ?? new AssemblyName[] { })
                    .GroupBy(assembly => assembly.Name).Select(item => item.FirstOrDefault())
                    .Where(assembly => assembly.Name.StartsWith("Aspose") && assembly.Name.StartsWith("Aspose.Hook") == false);
                if (assemblies != null)
                {
                    foreach (var assembly in assemblies)
                    {
                        if (mAssembliesLicenseSetted.Contains(assembly.FullName))
                        {
                            continue;
                        }
                        else
                        {
                            var type = Assembly.Load(assembly).GetType(assembly.Name + ".License");
                            if (type == null)
                            {
                                type = Assembly.Load(assembly).GetType(System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(assembly.Name.ToLower()) + ".License");
                            }
                            if (type != null)
                            {
                                LogProvider.Info($"\nSETTING...{type.FullName}");
                                var instance = Activator.CreateInstance(type);
                                type.GetMethod("SetLicense", new[] { typeof(Stream) }).Invoke(instance, BindingFlags.Public | BindingFlags.Instance, null, new[] { new MemoryStream(Convert.FromBase64String(LICENSE_STRING)) }, null);
                                LogProvider.Info($"{type.FullName} SET SUCCESSFULLY.");
                                mAssembliesLicenseSetted.Add(assembly.FullName);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var exception = e;
                while (exception.InnerException != null)
                {
                    exception = exception.InnerException;
                }
                LogProvider.Info($"start hook failed because of {exception.Message}.");
            }
            mHookStarted = true;
        }
        /// <summary>
        /// 停用Hook
        /// </summary>
        public static void StopHook()
        {
            if (mHookStarted == false)
            {
                return;
            }
            MethodHookManager.Instance.StopHook();
            mHookStarted = false;
        }
    }
}
