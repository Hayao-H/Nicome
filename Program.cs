using System;
using System.Collections.Generic;
using System.Net.Http;
using Nicome.Utils;
using CLI = Nicome.CLI;
using NicoEnums = Nicome.Enums;
using Store = Nicome.Store;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Linq;
using System.Data;
using System.Threading.Tasks;

namespace Nicome
{
    class Program
    {
        public static HttpClientHandler Handler = new HttpClientHandler() { UseCookies=true};

        public static HttpClient Client = new HttpClient(Program.Handler);

        static int Main(string[] args)
        {

            //ua
            Client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",$"Mozilla/5.0 ({Assembly.GetExecutingAssembly().GetName().Name} V.{Assembly.GetExecutingAssembly().GetName().Version}) Contact 5chan/sofware/1542064043");

            //バージョン出力
            if (args.Length > 0 && Regex.IsMatch(args[0], "--version|-v"))
            {
                Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Version);
                return 0;
            } else if (args.Length > 0 && Regex.IsMatch(args[0], "--help|-h"))
            {
                Console.WriteLine(Program.HelpText);
                return 0;
            }

            //補足されなかった例外のハンドラ
            AppDomain.CurrentDomain.UnhandledException +=Program.CurrentDomain_UnhandledException;


            //ロガー
            var logger = NicoLogger.GetLogger(Program.GetLogLevel(args));

            logger.Log($"{Assembly.GetExecutingAssembly().GetName().Name} V.{Assembly.GetExecutingAssembly().GetName().Version}");


            //引数解析
            var _args = new List<string>();
            _args.AddRange(args);
            var parser = new CLI::Parser();
            var errorlevel = parser.ParseArgs(args);


            //分岐
            switch (errorlevel)
            {
                case NicoEnums::GenelicErrorCode.ERROR:
                    logger.Log("処理を終了します。");
                    return 1;
                case NicoEnums::GenelicErrorCode.EXIT:
                    logger.Log("処理を終了します。");
                    return 0;
                case NicoEnums::GenelicErrorCode.OK:
                    break;
            }

            Store.Store store;
            //ストア初期化
            try
            {
                store = new Store::Store(parser);
            }
            catch (NoNullAllowedException e)
            {
                logger.Error(e.Message);
                return 100;
            }
            catch (Exception e)
            {
                logger.Error($"データの初期化中にエラーが発生しました。{e.Message}");
                logger.Error(e.StackTrace==null?"no stack_trace":e.StackTrace);
                return 100;
            }
            var storeData = store.GetData();

            List<string> IdList;
            try
            {
                IdList = CLI.IDHandler.GetIDLists().Result;
            }
            catch(Exception e)
            {
                logger.Error($"ダウンロードIDの解析中にエラーが発生しました。(詳細: {e.Message})");
                return 102;
            }

            if (IdList.Count > 1)
            {
                logger.Log($"id:{IdList[0]}ほか、{IdList.Count}件の動画をダウンロードします");
            }

            foreach (var id in IdList)
            {

                //実行
                var downloader = new WWW.Comment.Comment();
                errorlevel = downloader.DownloadComment(id).Result;

                switch (errorlevel)
                {
                    case NicoEnums::GenelicErrorCode.ERROR:
                        logger.Log("処理を終了します。");
                        return 101;
                }

                if (IdList.Count > 1&&errorlevel==NicoEnums::GenelicErrorCode.OK)
                {
                    logger.Log("待機中です...(5,000ms)");
                    Task.Delay(5000).Wait();
                }
            }

            logger.Log("処理を終了します)");
            return 0;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {

                Exception ex = (Exception)e.ExceptionObject;

                var logger = NicoLogger.GetLogger();
                logger.Error("アプリケーションがクラッシュしました。");
                logger.Error($"Exception: {ex.Message} Type: {ex.GetType()}");
                logger.Error(ex.StackTrace == null ? "no stacktrace..." : ex.StackTrace);
            }
            catch (Exception exo)
            {
                Console.WriteLine($"深刻なエラーが発生しました。(詳細: {exo.Message})");
                Console.WriteLine(exo.StackTrace);
            }
        }

        /// <summary>
        /// ログレベルの指定
        /// </summary>
        /// <returns></returns>
        private static NicoEnums.LOGLEVEL GetLogLevel(string[] args)
        {
            int index;
            if (args.Contains("-l"))
            {
                index = Array.IndexOf(args, "-l");
            }
            else if (args.Contains("--loglevel"))
            {
                index = Array.IndexOf(args, "--loglevel");
            }
            else
            {
                return NicoEnums.LOGLEVEL.Log;
            }

            if (index >= args.Length)
            {
                return NicoEnums.LOGLEVEL.Log;
            }
            else
            {
                string loglevel = args[index + 1];

                switch (loglevel)
                {
                    case "quiet":
                        return NicoEnums::LOGLEVEL.Quiet;
                    case "error":
                        return NicoEnums::LOGLEVEL.Error;
                    case "warn":
                        return NicoEnums::LOGLEVEL.Warn;
                    case "info":
                        return NicoEnums::LOGLEVEL.Info;
                    case "debug":
                        return NicoEnums::LOGLEVEL.Debug;
                    default:
                        return NicoEnums::LOGLEVEL.Log;
                }
            }
        }

        public static string HelpText { get; } = "実行方法: nicome <オプションキー> <値> <オプションキー> <値>... \n"
                                                + "  例) nicome -u nico@example.com -p 0000 -i sm9 -f 陰陽師\n"
                                                + "  -i | --id => <<必須>>ニコニコ動画におけるID。\n"
                                                + "  -u | --username => <<必須>>niconicoアカウントのユーザー名。\n"
                                                + "  -p | --password => <<必須>>niconicoアカウントのパスワード。\n"
                                                + "  -f | --folder => 保存フォルダー名。実行ファイルからの相対パス\n"
                                                + "  -k | --kako => 過去ログ取得オプション。値は不要。\n"
                                                + "  -l | --loglevel => quiet/error/warn/log(デフォルト)/info/debug\n"
                                                + "  -m | --max-comment => 最大コメント数(数値)\n"
                                                + "  -c | --channel => チャンネル";
    }
}
