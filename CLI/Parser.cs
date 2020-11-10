using Nicome.Enums;
using Nicome.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NicoEnums = Nicome.Enums;

namespace Nicome.CLI
{
    interface IParser
    {
        bool TryGetOption(string n, out CLICommand? o);
        bool Contains(string n);
        NicoEnums::GenelicErrorCode ParseArgs(string[] a);
    }

    class CLICommand
    {
        public CLICommand(string name)
        {
            Name = name;
        }
        /// <summary>
        /// コマンドフラグ
        /// </summary>
        public bool IsCommand;

        /// <summary>
        /// コマンド
        /// </summary>
        public string Name;

        /// <summary>
        /// 引数
        /// </summary>
        public string? Parameter;

    }

    class KnownOption
    {
        public KnownOption(string name, NicoEnums::Option code, string description)
        {
            Code = code;
            Name = name;
            Description = description;
        }
        public NicoEnums::Option Code;
        public string Name;
        public string Description;
    }

    class BaseParser
    {
        protected List<KnownOption> knownOptions;

        /// <summary>
        /// フィルターのリスト
        /// </summary>
        protected List<Func<string, string>> optionFilters = new List<Func<string, string>>();

        public BaseParser()
        {
            knownOptions = new List<KnownOption>();
        }

        /// <summary>
        /// オプションを追加する
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="commandCode"></param>
        protected void AddOption(string optionName, NicoEnums::Option code, string description = "")
        {
            var option = new KnownOption(optionName, code, description);
            knownOptions.Add(option);
        }


        /// <summary>
        /// 適切なオプションであるかどうかを返す
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="commandCode"></param>
        /// <returns></returns>
        protected bool IsValidOption(string commandName)
        {
            if (knownOptions.Any(v => v.Name == commandName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// オプション名を正規化する
        /// </summary>
        /// <param name="argment"></param>
        /// <returns></returns>
        protected string NormalizeOptionName(string argment)
        {
            foreach (var filter in optionFilters)
            {
                argment = filter(argment);
            }

            return argment;


        }

        /// <summary>
        /// オプションフィルターを追加する
        /// </summary>
        /// <param name="function"></param>
        protected void AddOptionFilter(Func<string, string> function)
        {
            optionFilters.Add(function);
        }

    }

    class Parser : BaseParser, IParser
    {
        private string moduleName = "CLI.Parser";
        public List<CLICommand>? Commands;

        public bool IsVersionMode = false;
        public bool IsHelpMode = false;

        public Parser()
        {
            Commands = new List<CLICommand>();

            AddOption("nicoid", NicoEnums::Option.NICOID);
            AddOption("foldername", NicoEnums::Option.FOLDER);
            AddOption("loglevel", NicoEnums::Option.LOGLEVEL);
            AddOption("user", NicoEnums::Option.USER);
            AddOption("pass", NicoEnums::Option.PASS);
            AddOption("comlog", NicoEnums::Option.COM_LOG);
            AddOption("maxcom", NicoEnums::Option.MAX_COM);
            AddOption("overwrite", NicoEnums::Option.OVERWRITE);
            //時間制NG
            AddOption("ngft", NicoEnums::Option.NG_BY_TIME);
            AddOption("ngftvpdt", NicoEnums::Option.NG_FROM_POST_DATETIME);
            AddOption("ngftdelay", NicoEnums::Option.NG_DELAY);
            //コマンドNG
            AddOption("ngmail", NicoEnums::Option.NG_MAIL);
            //ユーザーNG
            AddOption("nguser", NicoEnums::Option.NG_UID);
            //NGワード
            AddOption("ngword", NicoEnums::Option.NG_UID);

            //フィルター追加
            AddOptionFilter((string argment) =>
            {
                if (Regex.IsMatch(argment, "^(-i|--id)$"))
                {
                    argment = "nicoid";
                }
                return argment;
            });

            AddOptionFilter((string argment) =>
            {
                if (Regex.IsMatch(argment, "^(-f|--folder)$"))
                {
                    argment = "foldername";
                }
                return argment;
            });

            AddOptionFilter((string argment) =>
            {
                if (Regex.IsMatch(argment, "^(-l|--loglevel)$"))
                {
                    argment = "loglevel";
                }
                return argment;
            });

            AddOptionFilter((string argment) =>
            {
                if (Regex.IsMatch(argment, "^(-u|--username)$"))
                {
                    argment = "user";
                }
                return argment;
            });

            AddOptionFilter((string argment) =>
            {
                if (Regex.IsMatch(argment, "^(-p|--password)$"))
                {
                    argment = "pass";
                }
                return argment;
            });

            AddOptionFilter((string argment) =>
            {
                if (Regex.IsMatch(argment, "^(-k|--kako)$"))
                {
                    argment = "comlog";
                }
                return argment;
            });

            AddOptionFilter((string argment) =>
            {
                if (Regex.IsMatch(argment, "^(--ng-time-from-to)$"))
                {
                    argment = "ngft";
                }
                return argment;
            });

            AddOptionFilter((string argment) =>
            {
                if (Regex.IsMatch(argment, "^(--ng-time-from-to-postdate)$"))
                {
                    argment = "ngftvpdt";
                }
                return argment;
            });

            AddOptionFilter((string argment) =>
            {
                if (Regex.IsMatch(argment, "^(--ng-time-from-to-delay)$"))
                {
                    argment = "ngftdelay";
                }
                return argment;
            });

            AddOptionFilter((string argment) =>
            {
                if (Regex.IsMatch(argment, "^(-nc|--ng-command)$"))
                {
                    argment = "ngmail";
                }
                return argment;
            });

            AddOptionFilter((string argment) =>
            {
                if (Regex.IsMatch(argment, "^(-nu|--ng-user)$"))
                {
                    argment = "nguser";
                }
                return argment;
            });

            AddOptionFilter((string argment) =>
            {
                if (Regex.IsMatch(argment, "^(-nw|--ng-word)$"))
                {
                    argment = "ngword";
                }
                return argment;
            });

            AddOptionFilter((string argment) =>
            {
                if (Regex.IsMatch(argment, "^(-m|--max-comment)$"))
                {
                    argment = "maxcom";
                }
                return argment;
            });

            AddOptionFilter((string argment) =>
            {
                if (Regex.IsMatch(argment, "^(-y|--allow-overwrite)$"))
                {
                    argment = "overwrite";
                }
                return argment;
            });
        }

        /// <summary>
        /// オプションを取得する
        /// </summary>
        public bool TryGetOption(string name, out CLICommand? option)
        {
            var command = Commands.FirstOrDefault(cd => cd.Name == name);
            if (command != null)
            {
                option = command;
                return true;
            }
            else
            {
                option = null;
                return false;
            }
        }

        /// <summary>
        /// オプションを取得する
        /// </summary>
        public bool Contains(string name)
        {
            var command = Commands.FirstOrDefault(cd => cd.Name == name);
            if (command != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 引数を解析
        /// </summary>
        /// <param name="arglist"></param>
        public NicoEnums::GenelicErrorCode ParseArgs(string[] arglist)
        {
            var args = new List<string>();
            var logger = NicoLogger.GetLogger();

            args.AddRange(arglist);

            if (args.Count == 0)
            {
                logger.Error("ユーザー名・パスワード・コンテンツIDは必須の引数です。");
                return NicoEnums::GenelicErrorCode.ERROR;
            }

            CLICommand? lastCommand = null;


            //特別なモード
            if (Regex.IsMatch(args[0], "$(--help|-h)"))
            {
                IsHelpMode = true;
                return NicoEnums::GenelicErrorCode.EXIT;
            }
            else if (Regex.IsMatch(args[0], "$(--help|-h)"))
            {
                IsVersionMode = true;
                return NicoEnums::GenelicErrorCode.EXIT;
            }

            int i = 1;
            //引数解析
            foreach (string argment in args)
            {
                var command = new CLICommand(argment);

                string name = NormalizeOptionName(argment);
                //オプションの場合
                if (IsValidOption(name))
                {
                    command.Name = name;
                    command.IsCommand = false;
                    lastCommand = command;

                    if (i == args.Count)
                    {
                        logger.Debug($"オプション名：{lastCommand.Name}", moduleName);
                    }
                }
                //パラメーターの場合
                else if (lastCommand != null)
                {
                    //パラメーターを設定
                    lastCommand.Parameter = argment;
                    logger.Debug($"オプション名：{lastCommand.Name} 値：{argment}", moduleName);
                    //参照を切る
                    lastCommand = null;
                }
                else
                {
                    logger.Error($"不正なオプションです。({argment})");
                    return NicoEnums.GenelicErrorCode.ERROR;
                }

                Commands?.Add(command);
                ++i;

            }


            return NicoEnums::GenelicErrorCode.CONTINUE;
        }
    }
}
