﻿using System.Collections.Generic;
using System.ServiceModel;
using System.Web.Mvc;

using Moq;

using NUnit.Framework;

using UberDeployer.Agent.Proxy;
using UberDeployer.Agent.Proxy.Faults;
using UberDeployer.WebApp.Core.ApiControllers;
using UberDeployer.WebApp.Core.Controllers;
using UberDeployer.WebApp.Core.Services;

namespace UberDeployer.Tests.WebApp
{
  [TestFixture]
  public class ApiControllerTests
  {
    private ApiController _apiController;
    
    private Mock<ISessionService> _sessionServiceFake;
    private Mock<IAgentService> _agentServiceFake;
    private Mock<IDeploymentStateProvider> _deploymentStateProviderFake;

    [SetUp]
    public void SetUp()
    {
      _sessionServiceFake = new Mock<ISessionService>();
      _agentServiceFake = new Mock<IAgentService>();
      _deploymentStateProviderFake = new Mock<IDeploymentStateProvider>();

      _apiController = new ApiController(_sessionServiceFake.Object, _agentServiceFake.Object, _deploymentStateProviderFake.Object);
    }

    [Test]
    public void GetWebMachineNames_should_properly_return_machines_names()
    {
      // arrange  
      const string envName = "envName";
      var expectedMachineNames = new List<string> {"name1", "name2"};

      _agentServiceFake.Setup(x => x.GetWebMachineNames(envName)).Returns(expectedMachineNames);

      // act
      var machineNamesJson = _apiController.GetWebMachineNames(envName) as JsonResult;

      // assert
      Assert.NotNull(machineNamesJson);
      
      var machineNames = machineNamesJson.Data as List<string>;

      Assert.NotNull(machineNames);
      Assert.AreEqual(expectedMachineNames.Count, machineNames.Count);

      foreach (string machineName in machineNames)
      {
        Assert.Contains(machineName, expectedMachineNames);
      }
    }

    [Test]
    [Sequential]
    public void GetWebMachineNames_should_return_bad_request_when_invalid_enviroment_name(
      [Values("", null)] string invalidEnvName)
    {
      // act
      var httpStatusCodeResult = _apiController.GetWebMachineNames(string.Empty) as HttpStatusCodeResult;

      // assert
      Assert.IsNotNull(httpStatusCodeResult);
      Assert.AreEqual((int)400, (int)httpStatusCodeResult.StatusCode);
    }

    [Test]
    public void GetWebMachineNames_should_return_bad_request_when_environment_does_not_exist()
    {
      // arrange
      const string environmentName = "env name";
      var faultException =new FaultException<EnvironmentNotFoundFault>(
          new EnvironmentNotFoundFault
            {
              EnvironmentName = environmentName
            });

      _agentServiceFake.Setup(x => x.GetWebMachineNames(environmentName)).Throws(faultException);

      // act
      var httpStatusCodeResult = _apiController.GetWebMachineNames(string.Empty) as HttpStatusCodeResult;

      // assert
      Assert.IsNotNull(httpStatusCodeResult);
      Assert.AreEqual((int)400, (int)httpStatusCodeResult.StatusCode);
    }    
  }
}
