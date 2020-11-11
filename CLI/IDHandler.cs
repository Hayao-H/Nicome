using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nicome.CLI
{
    class IDHandler
    {
        public static async Task<List<string>> GetIDLists()
        {

            var storeData = new Store.Store().GetData();
            var ids = new List<string>();

            //チャンネルが指定されているかどうか
            if (!storeData.DoDownloadChannel())
            {
                ids.Add(storeData.GetNicoID());
            } else
            {
                var channnelClient = new WWW.Channel();
                ids.AddRange(await channnelClient.GetIdList(storeData.GetChannnelName()));
            }
            return ids;
        }
    }
}
