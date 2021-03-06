﻿using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class DuplicateResourceServiceTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void HandlesType_GivenServiceIsCreated_ShouldHandleCorrectly()
        {
            //---------------Set up test pack-------------------
            var resourceCatalog = new Mock<IResourceCatalog>();
            var serverExploer = new Mock<IExplorerServerResourceRepository>();
            DuplicateResourceService resourceService = new DuplicateResourceService(resourceCatalog.Object, serverExploer.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(resourceService);
            //---------------Execute Test ----------------------
            var handlesType = resourceService.HandlesType();
            //---------------Test Result -----------------------
            Assert.AreEqual("DuplicateResourceService", handlesType);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateServiceEntry_GivenServiceIsCreated_ShouldCreateCorrectDynamicService()
        {
            //---------------Set up test pack-------------------
            var resourceCatalog = new Mock<IResourceCatalog>();
            var serverExploer = new Mock<IExplorerServerResourceRepository>();
            DuplicateResourceService resourceService = new DuplicateResourceService(resourceCatalog.Object, serverExploer.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(resourceService);
            //---------------Execute Test ----------------------
            var handlesType = resourceService.CreateServiceEntry();
            //---------------Test Result -----------------------
            Assert.AreEqual(1, handlesType.Actions.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenResourcePayLoad_ShouldExctactPayLoad()
        {
            //---------------Set up test pack-------------------
            var resourceCatalog = new Mock<IResourceCatalog>();
            var serverExploer = new Mock<IExplorerServerResourceRepository>();
            var workScpace = new Mock<IWorkspace>();
            var xElement = XML.XmlResource.Fetch("PluginService");
            var pluginSource = new PluginSource(xElement);
            var guid = "7B71D6B8-3E11-4726-A7A0-AC924977D6E5";
            resourceCatalog.Setup(catalog => catalog.GetResource(GlobalConstants.ServerWorkspaceID, Guid.Parse(guid))).Returns(pluginSource);
            resourceCatalog.Setup(catalog => catalog.SaveResource(GlobalConstants.ServerWorkspaceID, It.IsAny<IResource>(), It.IsAny<string>(), It.IsAny<string>()));
            DuplicateResourceService resourceService = new DuplicateResourceService(resourceCatalog.Object, serverExploer.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(resourceService);
            //---------------Execute Test ----------------------

            resourceService.Execute(new Dictionary<string, StringBuilder>
            {
                {"ResourceID", new StringBuilder(guid) },
                {"NewResourceName", new StringBuilder("NewName") },
            }, workScpace.Object);
            //---------------Test Result -----------------------
            resourceCatalog.Verify(catalog => catalog.GetResource(GlobalConstants.ServerWorkspaceID, Guid.Parse(guid)));
            resourceCatalog.Setup(catalog => catalog.SaveResource(GlobalConstants.ServerWorkspaceID, It.IsAny<IResource>(), It.IsAny<string>(), It.IsAny<string>()));
        }
        Guid guid = "7B71D6B8-3E11-4726-A7A0-AC924977D6E5".ToGuid();

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenResourcePayLoadIFolder_ShouldSaveFolderWithNewName()
        {
            //---------------Set up test pack-------------------
            var resourceCatalog = new Mock<IResourceCatalog>();
            var serverExploer = new Mock<IExplorerServerResourceRepository>();
            var explorerItem = new Mock<IExplorerItem>();
            explorerItem.Setup(item => item.IsFolder).Returns(true);
            explorerItem.Setup(item => item.ResourceId).Returns(guid);
            var childXplorerItem = new Mock<IExplorerItem>();
            childXplorerItem.Setup(item => item.IsFolder).Returns(true);
            childXplorerItem.Setup(item => item.ResourceId).Returns(guid);
            explorerItem.Setup(item => item.Children).Returns(new List<IExplorerItem>() { childXplorerItem.Object });
            serverExploer.Setup(repository => repository.Find(It.IsAny<Guid>())).Returns(explorerItem.Object);
            var workScpace = new Mock<IWorkspace>();
            var folderResource = new Mock<IResource>();
            folderResource.SetupGet(resource => resource.IsFolder).Returns(true);

            resourceCatalog.Setup(catalog => catalog.GetResource(GlobalConstants.ServerWorkspaceID, guid)).Returns(folderResource.Object);
            resourceCatalog.Setup(catalog => catalog.SaveResource(GlobalConstants.ServerWorkspaceID, It.IsAny<IResource>(), It.IsAny<string>(), It.IsAny<string>()));
            resourceCatalog.Setup(catalog => catalog.GetResourceList(GlobalConstants.ServerWorkspaceID, It.IsAny<Dictionary<string, string>>()))
                .Returns(new List<Resource>());
            DuplicateResourceService resourceService = new DuplicateResourceService(resourceCatalog.Object, serverExploer.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(resourceService);
            //---------------Execute Test ----------------------

            var stringBuilder = resourceService.Execute(new Dictionary<string, StringBuilder>
            {
                {"ResourceID", new StringBuilder(guid.ToString()) },
                {"NewResourceName", new StringBuilder("NewName") },
            }, workScpace.Object);
            //---------------Test Result -----------------------
            resourceCatalog.Verify(catalog => catalog.GetResource(GlobalConstants.ServerWorkspaceID, guid));
            resourceCatalog.Verify(catalog => catalog.GetResourceList(GlobalConstants.ServerWorkspaceID, It.IsAny<Dictionary<string, string>>()));
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var executeMessage = serializer.Deserialize<ExecuteMessage>(stringBuilder);
            Assert.IsNotNull(executeMessage);
            Assert.IsFalse(executeMessage.HasError);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenResourcePayLoadIFolder_ShouldSaveClones()
        {
            //---------------Set up test pack-------------------
            var resourceCatalog = new Mock<IResourceCatalog>();
            var serverExploer = new Mock<IExplorerServerResourceRepository>();
            var explorerItem = new Mock<IExplorerItem>();
            explorerItem.Setup(item => item.IsFolder).Returns(true);
            explorerItem.Setup(item => item.ResourceId).Returns(guid);
            var childXplorerItem = new Mock<IExplorerItem>();
            childXplorerItem.Setup(item => item.IsFolder).Returns(true);
            childXplorerItem.Setup(item => item.ResourceId).Returns(guid);
            explorerItem.Setup(item => item.Children).Returns(new List<IExplorerItem>() { childXplorerItem.Object });
            serverExploer.Setup(repository => repository.Find(It.IsAny<Guid>())).Returns(explorerItem.Object);
            var workScpace = new Mock<IWorkspace>();
            var folderResource = new Mock<IResource>();
            folderResource.SetupGet(resource => resource.IsFolder).Returns(true);

            resourceCatalog.Setup(catalog => catalog.GetResource(GlobalConstants.ServerWorkspaceID, guid)).Returns(folderResource.Object);
            resourceCatalog.Setup(catalog => catalog.SaveResource(GlobalConstants.ServerWorkspaceID, It.IsAny<IResource>(), It.IsAny<string>(), It.IsAny<string>()));
            resourceCatalog.Setup(catalog => catalog.GetResourceList(GlobalConstants.ServerWorkspaceID, It.IsAny<Dictionary<string, string>>()))
                .Returns(new List<Resource>()
                {
                    new ComPluginService() {ResourceID = Guid.NewGuid(), ResourceName = "COM"},
                    new ComPluginService() {ResourceID = Guid.NewGuid(), ResourceName = "COM1"},
                });
            DuplicateResourceService resourceService = new DuplicateResourceService(resourceCatalog.Object, serverExploer.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(resourceService);
            //---------------Execute Test ----------------------

            var stringBuilder = resourceService.Execute(new Dictionary<string, StringBuilder>
            {
                {"ResourceID", new StringBuilder(guid.ToString()) },
                {"NewResourceName", new StringBuilder("NewName") },
            }, workScpace.Object);
            //---------------Test Result -----------------------
            resourceCatalog.Verify(catalog => catalog.GetResource(GlobalConstants.ServerWorkspaceID, guid));
            resourceCatalog.Verify(catalog => catalog.CopyResource(It.IsAny<IResource>(), GlobalConstants.ServerWorkspaceID, It.IsAny<string>()), Times.Once);
            resourceCatalog.Verify(catalog => catalog.SaveResource(GlobalConstants.ServerWorkspaceID, It.IsAny<IResource>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
            resourceCatalog.Verify(catalog => catalog.GetResourceList(GlobalConstants.ServerWorkspaceID, It.IsAny<Dictionary<string, string>>()));
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var executeMessage = serializer.Deserialize<ExecuteMessage>(stringBuilder);
            Assert.IsNotNull(executeMessage);
            Assert.IsFalse(executeMessage.HasError);
        }
    }
}
