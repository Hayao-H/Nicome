using Nicome.Utils;
using Nicome.WWW;
using Nicome.WWW.Comment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JsonComment = Nicome.WWW.API.Types.Comment.CommentBody.Json.JsonComment;


namespace Nicome.IO
{
    class Save
    {
        public string Filename { get; private set; }
        public string FilePath { get; private set; }
        public string FolderPath { get; private set; }
        public Save(VideoInfo video)
        {
            var store = new Store.Store().GetData();
            Filename = GetFilename(video);
            FilePath = GetFilePath(video);
            FolderPath = GetFolderPath();
        }
        private string moduleName = "IO.Save";

        /// <summary>
        /// ファイル名を取得する
        /// </summary>
        private string GetFilename(VideoInfo video)
        {
            var store = new Store.Store().GetData();
            string format = store.GetVideoFileFormat();
            return format.Replace("<id>", video.Id)
                .Replace("<title>", video.Title)
                .Replace("<user>", video.User)
                + ".xml";
        }

        /// <summary>
        /// フォルダーのパスを取得
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        private string GetFolderPath()
        {
            var store = new Store.Store().GetData();
            return store.GetVideoFilePath();
        }

        /// <summary>
        /// ファイルのパスを取得
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        private string GetFilePath(VideoInfo video)
        {
            string folder = GetFolderPath();
            string filename = GetFilename(video);
            return Path.Combine(folder, filename);
        }

        /// <summary>
        /// コメントを書き込む
        /// </summary>
        public bool TryWriteComment(List<JsonComment> comments)
        {
            var io = new Utils.IO();
            string commentString = ConvertToString(comments);
            var logger = NicoLogger.GetLogger();

            io.CreateFolderIfNotExist(FolderPath);

            try
            {
                logger.Debug("コメントファイルへの書き込みを開始", moduleName);
                io.Write(commentString, FilePath);
                logger.Debug("コメントファイルへの書き込みが完了", moduleName);
            }
            catch (Exception e)
            {
                logger.Error($"コメントファイル書き込み中にエラーが発生しました。(詳細: {e.Message})");
                return false;
            }

            return true;

        }

        /// <summary>
        /// コメントを文字列に変換する
        /// </summary>
        private string ConvertToString(List<JsonComment> comments)
        {
            var converter = new CommentConverter();
            return converter.GetXML(comments);
        }

        /// <summary>
        /// フォルダー作成
        /// </summary>
        private void CreateFolderIfNotExist()
        {
            if (!Directory.Exists(FolderPath))
            {
                var logger = NicoLogger.GetLogger();
                try
                {
                    Directory.CreateDirectory(FolderPath);
                }
                catch (Exception e)
                {
                    logger.Error($"保存フォルダーの作成に失敗しました。(詳細: {e.Message}, パス: {FolderPath})");
                }

                logger.Log("保存先のフォルダーが存在しなかった為、新規作成しました。");
                logger.Debug($"パス: {FolderPath}", moduleName);
            }
        }
    }
}
