﻿namespace Tauron.Application.Master.Commands.Deployment.Build
{
    public sealed class BuildErrorCodes
    {
        public const string GeneralQueryFailed = nameof(GeneralQueryFailed);

        public const string QueryAppNotFound = nameof(QueryAppNotFound);

        public const string QueryFileNotFound = nameof(QueryFileNotFound);

        public const string CommandErrorRegisterRepository = nameof(CommandErrorRegisterRepository);

        public const string CommandDuplicateApp = nameof(CommandDuplicateApp);
    }
}