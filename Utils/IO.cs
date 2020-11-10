using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Nicome.Utils
{
    class IO
    {
        private string moduleName = "Nicome.Utils.IO";

        /// <summary>
        /// ファイルの存在を確認
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool Exists(string id)
        {
            var data = new Store.Store().GetData();
           return  Directory.GetFiles(data.GetVideoFilePath(), $"[{id}]*").Length > 0;
        }

        /// <summary>
        /// フォルダー作成
        /// </summary>
        public void CreateFolderIfNotExist(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                var logger = NicoLogger.GetLogger();
                try
                {
                    Directory.CreateDirectory(folderPath);
                }
                catch (Exception e)
                {
                    logger.Error($"保存フォルダーの作成に失敗しました。(詳細: {e.Message}, パス: {folderPath})");
                }

                logger.Log("保存先のフォルダーが存在しなかった為、新規作成しました。");
                logger.Debug($"パス: {folderPath}", moduleName);
            }
        }

        /// <summary>
        /// 書き込む
        /// </summary>
        /// <param name="content"></param>
        /// <param name="path"></param>
        public void Write(string content,string path)
        {
            using (var fs = new StreamWriter(path))
            {
                fs.Write(content);
            }
        }
    }
}
