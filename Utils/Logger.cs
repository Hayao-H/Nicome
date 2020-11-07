using System;
using System.IO;

namespace Nicome.Utils
{
    interface ILogStream
    {
        void Write(string content);
        void Delete();
        void Move();
        string GetAbsPath();
        string GetFileName();
    }


    interface INicoLogger
    {
        void Log(string content);
        void Log(string content, bool inline);
        void Log(bool content);
        void Log(int content);
        void Info(string content);
        void Warn(string content);
        void Error(string content,bool c);
        void Debug(string content, string module);
    }

    class NicoLogger : INicoLogger
    {
        Enums.LOGLEVEL loglevel;
        private static NicoLogger? logger;

        /// <summary>
        /// Loggerを取得する
        /// </summary>
        /// <param name="_loglevel"></param>
        /// <returns></returns>
        public static NicoLogger GetLogger(Enums.LOGLEVEL _loglevel)
        {
            if (logger == null)
            {
                logger = new NicoLogger(_loglevel);
            }
            return logger;

        }

        public static NicoLogger GetLogger()
        {
            if (logger == null)
            {

                logger = new NicoLogger();
            }
            return logger;
        }

        public NicoLogger(Enums.LOGLEVEL _loglevel = Enums.LOGLEVEL.Log)
        {
            loglevel = _loglevel;
        }

        /// <summary>
        /// 警告
        /// </summary>
        /// <param name="content"></param>
        public void Warn(string content)
        {
            if ((int)loglevel >= (int)Enums.LOGLEVEL.Warn) { }
            {
                SetColor(GetColor("yellow"));
                WriteOnConsole(content, Enums.LOGLEVEL.Warn);
            }
        }

        /// <summary>
        /// 情報
        /// </summary>
        /// <param name="content"></param>
        public void Info(string content)
        {
            if ((int)loglevel >= (int)Enums.LOGLEVEL.Info)
            {
                SetColor(GetColor("green"));
                WriteOnConsole(content, Enums.LOGLEVEL.Info);
            }
        }

        /// <summary>
        /// エラー
        /// </summary>
        /// <param name="content"></param>
        public void Error(string content,bool commit=true)
        {
            if ((int)loglevel >= (int)Enums.LOGLEVEL.Error)
            {
                SetColor(GetColor("red"));
                WriteOnConsole(content, Enums.LOGLEVEL.Error);
            }
        }

        /// <summary>
        /// ログ
        /// </summary>
        /// <param name="content"></param>
        public void Log(string content)
        {
            if ((int)loglevel >= (int)Enums.LOGLEVEL.Log)
            {
                SetColor(GetColor());
                WriteOnConsole(content, Enums.LOGLEVEL.Log);
            }
        }

        /// <summary>
        /// ログ(インライン)
        /// </summary>
        public void Log(string content,bool inline)
        {
            if (!inline) { Log(content); }
            else if ((int)loglevel >= (int)Enums.LOGLEVEL.Log)
            {
                SetColor(GetColor());
                WriteOnConsole(content, Enums.LOGLEVEL.Log,true);
            }
        }

        /// <summary>
        /// ログ(真偽値)
        /// </summary>
        public void Log(bool content)
        {
            Log(content.ToString());
        }

        /// <summary>
        /// ログ(整数値)
        /// </summary>
        public void Log(int content)
        {
            Log(content.ToString());
        }

        /// <summary>
        /// デバッグ
        /// </summary>
        /// <param name="content"></param>
        public void Debug(string content, string module)
        {
            if ((int)loglevel >= (int)Enums.LOGLEVEL.Debug)
            {
                SetColor(GetColor("gray"));
                WriteOnConsole($"[{module}] {content}", Enums.LOGLEVEL.Debug);
            }
        }

        /// <summary>
        /// コンソールに書き込む
        /// </summary>
        /// <param name="content"></param>
        /// <param name="_loglevel"></param>
        private void WriteOnConsole(string content, Enums.LOGLEVEL _loglevel,bool inline=false)
        {
            if (inline)
            {
                Console.CursorLeft = 0;
                Console.Write($"[{_loglevel.ToString()}] {content}");
            }
            else
            {
                Console.WriteLine($"[{_loglevel.ToString()}] {content}");
            }
            SetColor(GetColor());
        }

        /// <summary>
        /// タイムスタンプを取得する
        /// </summary>
        /// <returns></returns>
        private string GetTimeStamp()
        {
            DateTime dt = DateTime.Now;
            return dt.ToString($"[hh:MM:ss.{dt.Millisecond.ToString().PadLeft(3, '0')}]");
        }

        /// <summary>
        /// 色を取得する
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private ConsoleColor GetColor(string color = "white")
        {
            switch (color)
            {
                case "red":
                    return ConsoleColor.Red;
                case "yellow":
                    return ConsoleColor.Yellow;

                case "green":
                    return ConsoleColor.Green;
                case "gray":
                    return ConsoleColor.Gray;
                default:
                    return ConsoleColor.White;
            }
        }

        /// <summary>
        /// 色を設定する
        /// </summary>
        /// <param name="color"></param>
        private void SetColor(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }

    }
}
