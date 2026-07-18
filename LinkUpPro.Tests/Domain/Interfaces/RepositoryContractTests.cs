using System.Reflection;
using LinkUpPro.Domain.Entities.Battleship;
using LinkUpPro.Domain.Entities.Friendship;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Domain.Interfaces.Repositories;
using DomainFriendship = LinkUpPro.Domain.Entities.Friendship.Friendship;

namespace LinkUpPro.Tests.Domain.Interfaces;

public class RepositoryContractTests
{
    [Theory]
    [InlineData(typeof(IPostRepository), typeof(Post), typeof(long))]
    [InlineData(typeof(ICommentRepository), typeof(Comment), typeof(long))]
    [InlineData(typeof(IReactionRepository), typeof(Reaction), typeof(long))]
    [InlineData(typeof(INotificationRepository), typeof(Notification), typeof(long))]
    [InlineData(typeof(IFriendshipRepository), typeof(DomainFriendship), typeof(long))]
    [InlineData(typeof(IFriendRequestRepository), typeof(FriendRequest), typeof(long))]
    [InlineData(typeof(IBattleshipRepository), typeof(BattleshipGame), typeof(long))]
    public void SpecificRepository_ExtendsExpectedGenericRepository(
        Type repositoryType,
        Type entityType,
        Type idType
    )
    {
        var expectedInterface = typeof(IGenericRepository<,>).MakeGenericType(entityType, idType);

        var implementsExpectedInterface = repositoryType
            .GetInterfaces()
            .Contains(expectedInterface);

        Assert.True(implementsExpectedInterface);
    }

    [Fact]
    public void GenericRepository_DoesNotExposePhysicalDeleteContract()
    {
        var deleteMethod = typeof(IGenericRepository<,>).GetMethod("Delete");

        Assert.Null(deleteMethod);
    }

    [Fact]
    public void RepositoryAsyncMethods_ExposeCancellationToken()
    {
        var repositoryInterfaces = new[]
        {
            typeof(IGenericRepository<,>),
            typeof(IPostRepository),
            typeof(ICommentRepository),
            typeof(IReactionRepository),
            typeof(INotificationRepository),
            typeof(IFriendshipRepository),
            typeof(IFriendRequestRepository),
            typeof(IBattleshipRepository),
        };

        var asyncMethodsWithoutCancellationToken = repositoryInterfaces
            .SelectMany(type => type.GetMethods())
            .Where(method => method.ReturnType == typeof(Task) || IsGenericTask(method.ReturnType))
            .Where(method =>
                method
                    .GetParameters()
                    .All(parameter => parameter.ParameterType != typeof(CancellationToken))
            )
            .Select(method => $"{method.DeclaringType?.Name}.{method.Name}")
            .ToArray();

        Assert.Empty(asyncMethodsWithoutCancellationToken);
    }

    [Fact]
    public void UnitOfWorkAsyncMethods_ExposeCancellationToken()
    {
        var asyncMethodsWithoutCancellationToken = typeof(IUnitOfWork)
            .GetMethods()
            .Where(method => method.ReturnType == typeof(Task) || IsGenericTask(method.ReturnType))
            .Where(method =>
                method
                    .GetParameters()
                    .All(parameter => parameter.ParameterType != typeof(CancellationToken))
            )
            .Select(method => method.Name)
            .ToArray();

        Assert.Empty(asyncMethodsWithoutCancellationToken);
    }

    [Fact]
    public void UnitOfWork_RequiresTransactionAndSaveContracts()
    {
        Assert.NotNull(typeof(IUnitOfWork).GetMethod(nameof(IUnitOfWork.BeginTransactionAsync)));
        Assert.NotNull(typeof(IUnitOfWork).GetMethod(nameof(IUnitOfWork.CommitAsync)));
        Assert.NotNull(typeof(IUnitOfWork).GetMethod(nameof(IUnitOfWork.RollbackAsync)));
        Assert.NotNull(typeof(IUnitOfWork).GetMethod(nameof(IUnitOfWork.SaveChangesAsync)));
    }

    private static bool IsGenericTask(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>);
}
