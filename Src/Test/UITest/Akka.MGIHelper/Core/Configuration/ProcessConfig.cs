using System;
using System.Collections;
using Microsoft.Extensions.Logging;
using Tauron.TAkka;
using Tauron.Application.Settings;

namespace Akka.MGIHelper.Core.Configuration
{
    public class ProcessConfig : ConfigurationBase
    {
        public ProcessConfig(IDefaultActorRef<SettingsManager> actor, string scope, ILogger<ProcessConfig> logger)
            : base(actor, scope, logger) { }

        public string Kernel => GetValue(s => s, string.Empty)!;

        public string Client => GetValue(s => s, string.Empty)!;

        public int ClientAffinity => GetValue(GetBitMap);

        public int OperatingSystemAffinity => GetValue(GetBitMap);
        
        private int GetBitMap(string arg)
        {
            if (string.IsNullOrWhiteSpace(arg)) return 0;

            try
            {
                var parameter = arg.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                var array = new BitArray(parameter.Length);

                for (var i = 0; i < parameter.Length; i++) 
                    array.Set(i, bool.Parse(parameter[i]));

                int[] data = { 0 };
                array.CopyTo(data, 0);

                return data[0];
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error on Convert {Data} to Bitmap", arg);
            }

            return 0;
        }
    }
}