using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Infrastructure.Persistence.Repositories;

namespace LinkUpPro.Tests.Infrastructure.Repositories;

public sealed class NotificationRepositoryTests : PersistenceTestBase
{
    [Fact]
    public async Task GetByRecipientAsync_ReturnsRecipientNotifications()
    {
        var context = CreateContext();
        var repo = new NotificationRepository(context);
        var actorId = CreateUserId("actor");
        var recipientId = CreateUserId("recipient");

        var post = Post.Create(recipientId, "Post", PostContentType.Image, "/img.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var notif = Notification.CreateComment(recipientId, actorId, post.Id, "actor").Value;
        context.Notifications.Add(notif);
        await context.SaveChangesAsync();

        var result = await repo.GetByRecipientAsync(recipientId);

        result.Should().HaveCount(1);
        result.First().Message.Should().Contain("actor");
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsLimited()
    {
        var context = CreateContext();
        var repo = new NotificationRepository(context);
        var actorId = CreateUserId("actor");
        var recipientId = CreateUserId("recip");

        var post = Post.Create(recipientId, "P", PostContentType.Image, "/i.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        for (int i = 0; i < 5; i++)
        {
            context.Notifications.Add(
                Notification.CreateComment(recipientId, actorId, post.Id, "actor").Value
            );
        }
        await context.SaveChangesAsync();

        var result = await repo.GetRecentAsync(recipientId, 3);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCorrectCount()
    {
        var context = CreateContext();
        var repo = new NotificationRepository(context);
        var actorId = CreateUserId("actor");
        var recipientId = CreateUserId("recip");

        var post = Post.Create(recipientId, "P", PostContentType.Image, "/i.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var n1 = Notification.CreateComment(recipientId, actorId, post.Id, "actor").Value;
        var n2 = Notification.CreateComment(recipientId, actorId, post.Id, "actor").Value;
        context.Notifications.AddRange(n1, n2);
        await context.SaveChangesAsync();
        n1.MarkAsRead();
        await context.SaveChangesAsync();

        var unread = await repo.GetUnreadCountAsync(recipientId);

        unread.Should().Be(1);
    }

    [Fact]
    public async Task MarkAsReadAsync_UpdatesStatus()
    {
        var context = CreateContext();
        var repo = new NotificationRepository(context);
        var actorId = CreateUserId("actor");
        var recipientId = CreateUserId("recip");

        var post = Post.Create(recipientId, "P", PostContentType.Image, "/i.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var notif = Notification.CreateComment(recipientId, actorId, post.Id, "actor").Value;
        context.Notifications.Add(notif);
        await context.SaveChangesAsync();

        await repo.MarkAsReadAsync(notif.Id, recipientId);
        await context.SaveChangesAsync();

        var retrieved = await repo.GetForRecipientAsync(notif.Id, recipientId);
        retrieved!.IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAllAsReadAsync_MarksAllUnread()
    {
        var context = CreateContext();
        var repo = new NotificationRepository(context);
        var actorId = CreateUserId("actor");
        var recipientId = CreateUserId("recip");

        var post = Post.Create(recipientId, "P", PostContentType.Image, "/i.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        for (int i = 0; i < 3; i++)
        {
            context.Notifications.Add(
                Notification.CreateComment(recipientId, actorId, post.Id, "actor").Value
            );
        }
        await context.SaveChangesAsync();

        await repo.MarkAllAsReadAsync(recipientId);
        await context.SaveChangesAsync();

        var unread = await repo.GetUnreadCountAsync(recipientId);
        unread.Should().Be(0);
    }

    [Fact]
    public async Task GetForRecipientAsync_WrongRecipient_ReturnsNull()
    {
        var context = CreateContext();
        var repo = new NotificationRepository(context);
        var actorId = CreateUserId("actor");
        var recipientId = CreateUserId("recip");
        var otherId = CreateUserId("other");

        var post = Post.Create(recipientId, "P", PostContentType.Image, "/i.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var notif = Notification.CreateComment(recipientId, actorId, post.Id, "actor").Value;
        context.Notifications.Add(notif);
        await context.SaveChangesAsync();

        var result = await repo.GetForRecipientAsync(notif.Id, otherId);

        result.Should().BeNull();
    }
}
