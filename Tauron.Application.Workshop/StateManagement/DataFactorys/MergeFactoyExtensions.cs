﻿using System.Collections.Generic;
using Functional.Maybe;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.DataFactorys
{
    public partial class MergeFactory
    {
        public static AdvancedDataSourceFactory Merge(params Maybe<AdvancedDataSourceFactory>[] factories)
        {
            var foundFac = factories.FindIndex(a => a is MergeFactory);
            MergeFactory factory;

            if (foundFac != -1)
                factory = (MergeFactory) factories[foundFac];
            else
                factory = new MergeFactory();

            for (var i = 0; i < factories.Length; i++)
            {
                if(i == foundFac) continue;

                factory.Register(factories[i]);
            }

            return factory;
        }

        public static AdvancedDataSourceFactory Merge(IEnumerable<Maybe<AdvancedDataSourceFactory>> factorys)
        {
            
        }
    }
}