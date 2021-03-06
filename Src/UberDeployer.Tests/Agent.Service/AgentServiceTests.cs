﻿using System;
using System.Collections.Generic;
using System.ServiceModel;

using Moq;

using NUnit.Framework;

using UberDeployer.Agent.Proxy.Faults;
using UberDeployer.Agent.Service;
using UberDeployer.Agent.Service.Diagnostics;
using UberDeployer.Core.Configuration;
using UberDeployer.Core.Deployment;
using UberDeployer.Core.Deployment.Pipeline;
using UberDeployer.Core.Deployment.Pipeline.Modules;
using UberDeployer.Core.Domain;
using UberDeployer.Core.Management.Metadata;
using UberDeployer.Core.TeamCity;
using UberDeployer.Tests.Core;

namespace UberDeployer.Tests.Agent.Service
{
  [TestFixture]
  public class AgentServiceTests
  {
    private AgentService _agentService;

    private Mock<IDeploymentPipeline> _deploymentPipelineFake;

    private Mock<IDiagnosticMessagesLogger> _diagnositcMessagesLoggerFake;

    private Mock<IProjectInfoRepository> _projectInfoRepositoryFake;

    private Mock<IEnvironmentInfoRepository> _environmentInfoRepositoryFake;
    private Mock<IEnvironmentDeployInfoRepository> _environmentDeployInfoRepositoryFake;
    private Mock<ITeamCityRestClient> _teamCityClientFake;

    private Mock<IDeploymentRequestRepository> _deploymentRequestRepositoryFake;

    private Mock<IProjectMetadataExplorer> _projectMetadataExplorerFake;

    private Mock<IDirPathParamsResolver> _dirPathParamsResolver;
    private Mock<IEnvDeploymentPipeline> _envDeploymentPipeline;

    private Mock<IApplicationConfiguration> _applicationConfigurationMock;

    [SetUp]
    public void SetUp()
    {
      _deploymentPipelineFake = new Mock<IDeploymentPipeline>();
      _diagnositcMessagesLoggerFake = new Mock<IDiagnosticMessagesLogger>();
      _projectInfoRepositoryFake = new Mock<IProjectInfoRepository>();
      _environmentInfoRepositoryFake = new Mock<IEnvironmentInfoRepository>();
      _teamCityClientFake = new Mock<ITeamCityRestClient>();
      _deploymentRequestRepositoryFake = new Mock<IDeploymentRequestRepository>();
      _projectMetadataExplorerFake = new Mock<IProjectMetadataExplorer>();
      _dirPathParamsResolver = new Mock<IDirPathParamsResolver>();
      _applicationConfigurationMock = new Mock<IApplicationConfiguration>();
      _environmentDeployInfoRepositoryFake = new Mock<IEnvironmentDeployInfoRepository>();
      _envDeploymentPipeline = new Mock<IEnvDeploymentPipeline>();

      _agentService =
        new AgentService(
          _deploymentPipelineFake.Object,
          _projectInfoRepositoryFake.Object,
          _environmentInfoRepositoryFake.Object,
          _teamCityClientFake.Object,
          _deploymentRequestRepositoryFake.Object,
          _diagnositcMessagesLoggerFake.Object,
          _projectMetadataExplorerFake.Object,
          _dirPathParamsResolver.Object,
          _applicationConfigurationMock.Object,
          _environmentDeployInfoRepositoryFake.Object,
          _envDeploymentPipeline.Object);
    }

    [Test]
    public void GetWebMachineNames_fails_when_environment_does_not_exists()
    {
      // arrange  
      const string notExistingEnvName = "env name";

      _environmentInfoRepositoryFake
        .Setup(x => x.FindByName(notExistingEnvName))
        .Returns((EnvironmentInfo) null);

      // act assert
      Assert.Throws<FaultException<EnvironmentNotFoundFault>>(
        () => _agentService.GetWebMachineNames(notExistingEnvName));
    }

    [Test]
    public void GetWebMachineNames_properly_gets_machines_names()
    {
      // arrange  
      var expectedWebMachineNames = new List<string> { "machine1", "machine2" };
      const string environmentName = "env name";

      var environmentInfo = GetEnvironmentInfo(environmentName, expectedWebMachineNames);

      _environmentInfoRepositoryFake
        .Setup(x => x.FindByName(environmentName))
        .Returns(environmentInfo);

      // act      
      List<string> webMachineNames = _agentService.GetWebMachineNames(environmentName);

      // assert
      Assert.AreEqual(expectedWebMachineNames.Count, webMachineNames.Count);

      foreach (var webMachineName in expectedWebMachineNames)
      {
        Assert.Contains(webMachineName, webMachineNames);
      }
    }

    [Test]
    public void GetWebMachineNames_fails_on_null_or_empty_environment_name()
    {
      Assert.Throws<ArgumentException>(() => _agentService.GetWebMachineNames(null));
      Assert.Throws<ArgumentException>(() => _agentService.GetWebMachineNames(string.Empty));
    }

    private static EnvironmentInfo GetEnvironmentInfo(string environmentName,
      IEnumerable<string> expectedWebMachineNames)
    {
      return new EnvironmentInfo(
        environmentName,
        true,
        "configurationTemplateName",
        "appServerMachineName",
        "failOverMachineName",
        expectedWebMachineNames,
        "terminalServerMachineName",
        new[] { "schedulerServerTasksMachineName1", "schedulerServerTasksMachineName2", },
        new[] { "schedulerServerBinariesMachineName1", "schedulerServerBinariesMachineName2", },
        "ntServiceDirPath",
        "webAppsBaseDirPath",
        "schedulerAppsBaseDirPath",
        "terminalAppsBaseDirPath",
        false,
        new[] { new EnvironmentUser("id", "user") },
        new[] { new IisAppPoolInfo("apppool", IisAppPoolVersion.V4_0, IisAppPoolMode.Integrated), },
        new[] { new DatabaseServer("db_server_id", "db_server"), },
        new[] { new ProjectToFailoverClusterGroupMapping("projectName", "groupName") },
        new[] { new WebAppProjectConfigurationOverride("webappprj", "website", "apppool", "dir", "webapp"), },
        new[] { new DbProjectConfigurationOverride("dbprj", "db_server"), },
        "terminalAppsShortcutFolder",
        "artifactsDeploymentDirPath",
        "domain-name",
        TestData.CustomEnvMachines);
    }
  }
}
