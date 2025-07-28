using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Moq;
using SignalRChatTest.Hub;
using SignalRChatTest.Models;
using SignalRChatTest.Service;

namespace SignalRChatTest.Tests;

public class VoiceHubTests
{
    public class HubCallerContextMock : HubCallerContext
    {
        private readonly string _connectionId;
        private readonly ClaimsPrincipal _user;

        public HubCallerContextMock(string connectionId, ClaimsPrincipal user)
        {
            _connectionId = connectionId;
            _user = user;
        }

        public override void Abort()
        {
            throw new NotImplementedException();
        }

        public override string ConnectionId => _connectionId;
        public override string? UserIdentifier { get; }
        public override ClaimsPrincipal User => _user;
        public override IDictionary<object, object?> Items { get; }
        public override IFeatureCollection Features { get; }
        public override CancellationToken ConnectionAborted { get; }
    }
    
    [Fact]
    public async Task OnConnectedAsync_AddConToExistsUser_Test()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string username = "user";
        string connectionId = "12345";
        
        // My interface's mocks
        Mock<IChatHistoryService> chatHistoryServiceMock = new Mock<IChatHistoryService>();
        Mock<IRedisService> mockRedisService = new Mock<IRedisService>();
        Mock<IConnectionManager> mockConnectionManager = new Mock<IConnectionManager>();

        // // HubCaller mock
        // Mock<HubCallerContext> mockHubCallerContext = new Mock<HubCallerContext>();
        
        // Claims
        List<Claim> claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Test");
        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        
        // mockHubCallerContext.Setup(x => x.ConnectionId).Returns(connectionId);
        // mockHubCallerContext.Setup(x => x.User).Returns(claimsPrincipal);

        ChatUser existingUser = new ChatUser()
        {
            Id = userId,
            Username = username,
        };
        
        mockConnectionManager.Setup(cm => cm.GetUserByIdAsync(userId.ToString())).ReturnsAsync(existingUser);
        
        // Mock<VoiceHub> mockVoiceHub = new Mock<VoiceHub>(mockConnectionManager.Object)
        // {
        //     CallBase = true // allow call base realization
        // };
        // mockVoiceHub.Setup(vh => vh.Context).Returns(mockHubCallerContext.Object);
        
        Mock<IHubCallerClients> mockHubCallerClients = new Mock<IHubCallerClients>();
        Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>(); // All.SendAsync
        Mock<IClientProxy> mockCallerProxy = new Mock<IClientProxy>(); // Caller.SendAsync
        
        VoiceHub voiceHub = new VoiceHub(mockConnectionManager.Object)
        {
            Clients = mockHubCallerClients.Object,
            Context = new HubCallerContextMock(connectionId, claimsPrincipal)
        };
        
        mockHubCallerClients.Setup(cl => cl.All).Returns(mockClientProxy.Object);
        mockHubCallerClients.Setup(cl => cl.Caller).Returns(mockCallerProxy.Object as ISingleClientProxy);
        // mockVoiceHub.SetupGet(vh => vh.Clients).Returns(mockHubCallerClients.Object);
        
        // ACT
        // await mockVoiceHub.Object.OnConnectedAsync();
        await voiceHub.OnConnectedAsync();
        
        // Assert
        // Проверяем, что GetUserByIdAsync был вызван один раз с правильным userId
        mockConnectionManager.Verify(cm => cm.GetUserByIdAsync(userId.ToString()), Times.Once);

        // Проверяем, что AddConnectionAsync был вызван один раз с правильными аргументами
        mockConnectionManager.Verify(cm => cm.AddConnectionAsync(userId.ToString(), HubType.Voice, connectionId), Times.Once);

        // Проверяем, что AddUserAsync НЕ был вызван
        mockConnectionManager.Verify(cm => cm.AddUserAsync(It.IsAny<ChatUser>()), Times.Never);

        // // Проверяем, что Clients.All.SendAsync был вызван дважды (UpdateUserList, UserConnected)
        // mockClientProxy.Verify(cp => cp.SendCoreAsync("UpdateUserList", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Once);
        // mockClientProxy.Verify(cp => cp.SendCoreAsync("UserConnected", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
     public async Task OnConnectedAsync_AddConToNullUser_Test()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string username = "user";
        string connectionId = "12345";
        
        // My interface's mocks
        Mock<IChatHistoryService> chatHistoryServiceMock = new Mock<IChatHistoryService>();
        Mock<IRedisService> mockRedisService = new Mock<IRedisService>();
        Mock<IConnectionManager> mockConnectionManager = new Mock<IConnectionManager>();

        // // HubCaller mock
        // Mock<HubCallerContext> mockHubCallerContext = new Mock<HubCallerContext>();
        
        // Claims
        List<Claim> claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Test");
        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        
        // mockHubCallerContext.Setup(x => x.ConnectionId).Returns(connectionId);
        // mockHubCallerContext.Setup(x => x.User).Returns(claimsPrincipal);

        ChatUser wantedUser = new ChatUser()
        {
            Id = userId,
            Username = username,
        };
        wantedUser.AddConnectionId(HubType.Voice, connectionId).Wait();
        
        mockConnectionManager.Setup(cm => cm.GetUserByIdAsync(userId.ToString())).ReturnsAsync(null as ChatUser);
        
        // Mock<VoiceHub> mockVoiceHub = new Mock<VoiceHub>(mockConnectionManager.Object)
        // {
        //     CallBase = true // allow call base realization
        // };
        // mockVoiceHub.Setup(vh => vh.Context).Returns(mockHubCallerContext.Object);
        
        Mock<IHubCallerClients> mockHubCallerClients = new Mock<IHubCallerClients>();
        Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>(); // All.SendAsync
        Mock<IClientProxy> mockCallerProxy = new Mock<IClientProxy>(); // Caller.SendAsync
        
        VoiceHub voiceHub = new VoiceHub(mockConnectionManager.Object)
        {
            Clients = mockHubCallerClients.Object,
            Context = new HubCallerContextMock(connectionId, claimsPrincipal)
        };
        
        mockHubCallerClients.Setup(cl => cl.All).Returns(mockClientProxy.Object);
        mockHubCallerClients.Setup(cl => cl.Caller).Returns(mockCallerProxy.Object as ISingleClientProxy);

        mockConnectionManager.Setup(cm => cm.AddUserAsync(new ChatUser()
        {
            Id = Guid.Parse(voiceHub.Context.User.FindFirst(ClaimTypes.NameIdentifier).Value),
            Username = voiceHub.Context.User.FindFirst(ClaimTypes.Name).Value,
        }));
        // mockVoiceHub.SetupGet(vh => vh.Clients).Returns(mockHubCallerClients.Object);
        
        // ACT
        // await mockVoiceHub.Object.OnConnectedAsync();
        await voiceHub.OnConnectedAsync();
        
        // Assert
        // Проверяем, что GetUserByIdAsync был вызван один раз с правильным userId
        mockConnectionManager.Verify(cm => cm.GetUserByIdAsync(userId.ToString()), Times.Once);
        
        // // Проверяем, что добавленный пользователь = ожидаемому
        // EqualsMethods.EqualsMethods.CheckTwoUserEquals(как узнать, какого пользователь добавил voiceHub, wantedUser);

        // Проверяем, что AddConnectionAsync был вызван один раз с правильными аргументами
        // mockConnectionManager.Verify(cm => cm.AddConnectionAsync(userId.ToString(), HubType.Voice, connectionId), Times.Once);
        mockConnectionManager.Verify(cm => cm.AddUserAsync(It.Is<ChatUser>(u =>
            u.Id == wantedUser.Id &&
            u.Username == wantedUser.Username &&
            u.GetConnectionIds(HubType.Voice).Result.Contains(connectionId)
        )), Times.Once);

        // Проверяем, что AddUserAsync НЕ был вызван
        mockConnectionManager.Verify(cm => cm.AddUserAsync(It.IsAny<ChatUser>()), Times.Once);

        // // Проверяем, что Clients.All.SendAsync был вызван дважды (UpdateUserList, UserConnected)
        // mockClientProxy.Verify(cp => cp.SendCoreAsync("UpdateUserList", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Once);
        // mockClientProxy.Verify(cp => cp.SendCoreAsync("UserConnected", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}