﻿using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Querys;

public sealed record QueryApp(AppName AppName) : DeploymentQueryBase<QueryApp, AppInfo>(AppName);