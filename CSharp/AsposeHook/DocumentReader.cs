using Aspose.Words;
using GWCustomerInfoCollection.STD.Hook;
namespace STD.Utils
{
    public static class DocumentReader
    {
        /// <summary>
        /// 读取任意 Word/PDF/RTF/TXT/HTML 等文档的纯文本内容
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文档内的纯文本内容</returns>
        public static string ExtractText(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("文件路径不能为空", nameof(filePath));

            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException($"找不到文件：{filePath}");

            try
            {
                HookManager.StartHook();
                // 让 Aspose 自动识别文档类型
                var doc = new Document(filePath);

                // 获取全文纯文本（不包含格式）
                string text = doc.Range.Text;

                // 去掉多余空行
                text = text?.Trim();

                return text ?? string.Empty;
            }
            catch (Exception ex)
            {
                LogProvider.Error($"读取文档失败：{ex.Message}");
                return string.Empty;
            }
        }
    }
}
